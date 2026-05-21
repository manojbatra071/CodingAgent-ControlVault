using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace InvoiceRiskMonitor
{
    public static class ExcelWorkbook
    {
        private static readonly XNamespace Spreadsheet = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private static readonly XNamespace Relationships = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private static readonly XNamespace PackageRelationships = "http://schemas.openxmlformats.org/package/2006/relationships";

        public static List<Dictionary<string, string>> ReadSheet(string workbookPath, string sheetName)
        {
            using (ZipArchive archive = ZipFile.OpenRead(workbookPath))
            {
                List<string> sharedStrings = ReadSharedStrings(archive);
                ZipArchiveEntry sheetEntry = ResolveSheetEntry(archive, sheetName);
                XDocument sheet = LoadXml(sheetEntry);

                List<List<string>> rows = sheet
                    .Descendants(Spreadsheet + "row")
                    .Select(row => ReadRow(row, sharedStrings))
                    .Where(row => row.Count > 0)
                    .ToList();

                if (rows.Count == 0)
                {
                    return new List<Dictionary<string, string>>();
                }

                List<string> headers = rows[0].Select(value => value.Trim()).ToList();
                var records = new List<Dictionary<string, string>>();

                for (int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
                {
                    var record = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    for (int columnIndex = 0; columnIndex < headers.Count; columnIndex++)
                    {
                        string header = headers[columnIndex];
                        if (string.IsNullOrWhiteSpace(header))
                        {
                            continue;
                        }

                        string value = columnIndex < rows[rowIndex].Count ? rows[rowIndex][columnIndex] : string.Empty;
                        record[header] = value;
                    }

                    records.Add(record);
                }

                return records;
            }
        }

        public static void WriteSheet(string workbookPath, string sheetName, IList<string> headers, IList<IList<string>> rows)
        {
            string directory = Path.GetDirectoryName(workbookPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(workbookPath))
            {
                File.Delete(workbookPath);
            }

            using (ZipArchive archive = ZipFile.Open(workbookPath, ZipArchiveMode.Create))
            {
                AddText(archive, "[Content_Types].xml", ContentTypesXml());
                AddText(archive, "_rels/.rels", PackageRelsXml());
                AddText(archive, "xl/_rels/workbook.xml.rels", WorkbookRelsXml());
                AddText(archive, "xl/workbook.xml", WorkbookXml(sheetName));
                AddText(archive, "xl/styles.xml", StylesXml());
                AddText(archive, "xl/worksheets/sheet1.xml", SheetXml(headers, rows));
            }
        }

        private static List<string> ReadSharedStrings(ZipArchive archive)
        {
            ZipArchiveEntry entry = archive.GetEntry("xl/sharedStrings.xml");
            if (entry == null)
            {
                return new List<string>();
            }

            XDocument shared = LoadXml(entry);
            return shared
                .Descendants(Spreadsheet + "si")
                .Select(item => string.Concat(item.Descendants(Spreadsheet + "t").Select(text => text.Value)))
                .ToList();
        }

        private static ZipArchiveEntry ResolveSheetEntry(ZipArchive archive, string sheetName)
        {
            XDocument workbook = LoadXml(GetRequiredEntry(archive, "xl/workbook.xml"));
            XDocument workbookRels = LoadXml(GetRequiredEntry(archive, "xl/_rels/workbook.xml.rels"));

            XElement sheet = workbook
                .Descendants(Spreadsheet + "sheet")
                .FirstOrDefault(item => string.Equals((string)item.Attribute("name"), sheetName, StringComparison.OrdinalIgnoreCase));

            if (sheet == null)
            {
                throw new InvalidOperationException($"Sheet '{sheetName}' was not found.");
            }

            string relationshipId = (string)sheet.Attribute(Relationships + "id");
            XElement relationship = workbookRels
                .Descendants(PackageRelationships + "Relationship")
                .FirstOrDefault(item => string.Equals((string)item.Attribute("Id"), relationshipId, StringComparison.OrdinalIgnoreCase));

            if (relationship == null)
            {
                throw new InvalidOperationException($"Worksheet relationship '{relationshipId}' was not found.");
            }

            string target = ((string)relationship.Attribute("Target")).Replace("\\", "/");
            string entryPath = target.StartsWith("xl/", StringComparison.OrdinalIgnoreCase) ? target : "xl/" + target.TrimStart('/');
            return GetRequiredEntry(archive, entryPath);
        }

        private static List<string> ReadRow(XElement row, IList<string> sharedStrings)
        {
            var values = new List<string>();

            foreach (XElement cell in row.Elements(Spreadsheet + "c"))
            {
                int columnIndex = GetColumnIndex((string)cell.Attribute("r"));
                while (values.Count < columnIndex)
                {
                    values.Add(string.Empty);
                }

                values.Add(ReadCell(cell, sharedStrings));
            }

            return values;
        }

        private static string ReadCell(XElement cell, IList<string> sharedStrings)
        {
            string type = (string)cell.Attribute("t");

            if (type == "inlineStr")
            {
                return string.Concat(cell.Descendants(Spreadsheet + "t").Select(text => text.Value));
            }

            string rawValue = (string)cell.Element(Spreadsheet + "v") ?? string.Empty;
            if (type == "s" && int.TryParse(rawValue, out int sharedIndex) && sharedIndex >= 0 && sharedIndex < sharedStrings.Count)
            {
                return sharedStrings[sharedIndex];
            }

            return rawValue;
        }

        private static int GetColumnIndex(string cellReference)
        {
            if (string.IsNullOrWhiteSpace(cellReference))
            {
                return 0;
            }

            string letters = Regex.Match(cellReference, "^[A-Z]+", RegexOptions.IgnoreCase).Value.ToUpperInvariant();
            int index = 0;
            foreach (char letter in letters)
            {
                index = index * 26 + (letter - 'A' + 1);
            }

            return Math.Max(0, index - 1);
        }

        private static ZipArchiveEntry GetRequiredEntry(ZipArchive archive, string path)
        {
            ZipArchiveEntry entry = archive.GetEntry(path);
            if (entry == null)
            {
                throw new InvalidOperationException($"Workbook part '{path}' was not found.");
            }

            return entry;
        }

        private static XDocument LoadXml(ZipArchiveEntry entry)
        {
            using (Stream stream = entry.Open())
            {
                return XDocument.Load(stream);
            }
        }

        private static void AddText(ZipArchive archive, string path, string content)
        {
            ZipArchiveEntry entry = archive.CreateEntry(path);
            using (var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false)))
            {
                writer.Write(content);
            }
        }

        private static string SheetXml(IList<string> headers, IList<IList<string>> rows)
        {
            var xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            xml.AppendLine("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>");
            AppendRow(xml, 1, headers);

            for (int i = 0; i < rows.Count; i++)
            {
                AppendRow(xml, i + 2, rows[i]);
            }

            xml.AppendLine("</sheetData></worksheet>");
            return xml.ToString();
        }

        private static void AppendRow(StringBuilder xml, int rowNumber, IList<string> values)
        {
            xml.Append("<row r=\"").Append(rowNumber).Append("\">");
            for (int columnIndex = 0; columnIndex < values.Count; columnIndex++)
            {
                string cellReference = GetCellReference(columnIndex, rowNumber);
                string value = SecurityElement.Escape(values[columnIndex] ?? string.Empty);
                xml.Append("<c r=\"").Append(cellReference).Append("\" t=\"inlineStr\"><is><t>")
                    .Append(value)
                    .Append("</t></is></c>");
            }

            xml.AppendLine("</row>");
        }

        private static string GetCellReference(int columnIndex, int rowNumber)
        {
            int dividend = columnIndex + 1;
            string columnName = string.Empty;
            while (dividend > 0)
            {
                int modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName + rowNumber;
        }

        private static string ContentTypesXml()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                   "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
                   "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
                   "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
                   "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>" +
                   "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>" +
                   "<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>" +
                   "</Types>";
        }

        private static string PackageRelsXml()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                   "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                   "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>" +
                   "</Relationships>";
        }

        private static string WorkbookRelsXml()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                   "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                   "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>" +
                   "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>" +
                   "</Relationships>";
        }

        private static string WorkbookXml(string sheetName)
        {
            string safeSheetName = SecurityElement.Escape(sheetName);
            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                   "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">" +
                   "<sheets><sheet name=\"" + safeSheetName + "\" sheetId=\"1\" r:id=\"rId1\"/></sheets>" +
                   "</workbook>";
        }

        private static string StylesXml()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                   "<styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">" +
                   "<fonts count=\"1\"><font><sz val=\"11\"/><name val=\"Calibri\"/></font></fonts>" +
                   "<fills count=\"1\"><fill><patternFill patternType=\"none\"/></fill></fills>" +
                   "<borders count=\"1\"><border><left/><right/><top/><bottom/><diagonal/></border></borders>" +
                   "<cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs>" +
                   "<cellXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\"/></cellXfs>" +
                   "</styleSheet>";
        }
    }
}
