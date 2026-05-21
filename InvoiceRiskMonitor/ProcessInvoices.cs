using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UiPath.CodedWorkflows;
using UiPath.Core;

namespace InvoiceRiskMonitor
{
    public class ProcessInvoices : CodedWorkflow
    {
        [Workflow]
        public void Execute(string in_ConfigPath = "Config.xlsx")
        {
            string projectDirectory = Directory.GetCurrentDirectory();
            string configPath = ConfigLoader.ResolvePath(projectDirectory, in_ConfigPath);

            try
            {
                Log("Invoice risk monitor started.");
                Log($"Loading configuration from {configPath}.");

                AutomationConfig config = ConfigLoader.Load(configPath);
                string inputPath = ConfigLoader.ResolvePath(projectDirectory, config.InputFilePath);
                string outputPath = ConfigLoader.ResolvePath(projectDirectory, config.OutputFilePath);

                Log($"Input file resolved to {inputPath}.");
                Log($"Output file resolved to {outputPath}.");

                if (!File.Exists(inputPath))
                {
                    throw new BusinessRuleException($"Input invoice file was not found: {inputPath}");
                }

                List<Dictionary<string, string>> rows = RetryPolicy.Execute(
                    () => ExcelWorkbook.ReadSheet(inputPath, config.InputSheetName),
                    config.RetryCount,
                    config.RetryDelayMs,
                    message => Log(message));

                Log($"Read {rows.Count} invoice row(s).");
                InvoiceRiskEngine.ValidateRequiredColumns(rows, config.MandatoryColumns);

                Dictionary<string, int> invoiceCounts = rows
                    .Where(row => row.ContainsKey("InvoiceNo"))
                    .GroupBy(row => row["InvoiceNo"].Trim(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

                var results = new List<InvoiceResult>();

                for (int index = 0; index < rows.Count; index++)
                {
                    Dictionary<string, string> row = rows[index];
                    int rowNumber = index + 2;

                    try
                    {
                        InvoiceRecord invoice = InvoiceRiskEngine.ParseInvoice(row, rowNumber, config);
                        bool isDuplicate = invoiceCounts.ContainsKey(invoice.InvoiceNo) && invoiceCounts[invoice.InvoiceNo] > 1;
                        RiskResult risk = InvoiceRiskEngine.ScoreInvoice(invoice, config, isDuplicate);

                        Log($"Processed invoice {invoice.InvoiceNo}: score {risk.RiskScore}, level {risk.RiskLevel}.");
                        results.Add(InvoiceRiskEngine.ToSuccessResult(invoice, risk));
                    }
                    catch (BusinessRuleException ex)
                    {
                        Log($"Business exception for input row {rowNumber}: {ex.Message}");
                        results.Add(InvoiceRiskEngine.ToBusinessExceptionResult(row, ex.Message));
                    }
                    catch (Exception ex)
                    {
                        Log($"System exception for input row {rowNumber}: {ex.Message}");
                        throw;
                    }
                }

                RetryPolicy.Execute(
                    () => ExcelWorkbook.WriteSheet(outputPath, config.OutputSheetName, InvoiceRiskEngine.ReportHeaders(), InvoiceRiskEngine.ReportRows(results)),
                    config.RetryCount,
                    config.RetryDelayMs,
                    message => Log(message));

                Log($"Risk report written to {outputPath}.");
                Log("Invoice risk monitor completed.");
            }
            catch (BusinessRuleException ex)
            {
                Log($"Business exception stopped processing: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Log($"Unhandled system exception stopped processing: {ex}");
                throw;
            }
        }
    }
}
