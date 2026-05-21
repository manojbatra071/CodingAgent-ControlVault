using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UiPath.Core;

namespace InvoiceRiskMonitor
{
    public static class InvoiceRiskEngine
    {
        public static void ValidateRequiredColumns(IEnumerable<Dictionary<string, string>> rows, IEnumerable<string> requiredColumns)
        {
            Dictionary<string, string> firstRow = rows.FirstOrDefault();
            if (firstRow == null)
            {
                throw new BusinessRuleException("Input file does not contain invoice data rows.");
            }

            List<string> missingColumns = requiredColumns
                .Where(column => !firstRow.ContainsKey(column))
                .ToList();

            if (missingColumns.Count > 0)
            {
                throw new BusinessRuleException("Input file is missing required column(s): " + string.Join(", ", missingColumns));
            }
        }

        public static InvoiceRecord ParseInvoice(Dictionary<string, string> row, int rowNumber, AutomationConfig config)
        {
            var missingFields = new List<string>();

            foreach (string column in config.MandatoryColumns)
            {
                if (!row.ContainsKey(column) || string.IsNullOrWhiteSpace(row[column]))
                {
                    missingFields.Add(column);
                }
            }

            if (missingFields.Count > 0)
            {
                throw new BusinessRuleException($"Row {rowNumber} has missing mandatory field(s): {string.Join(", ", missingFields)}");
            }

            string amountText = Value(row, "Amount");
            if (!decimal.TryParse(amountText, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount) || amount <= 0)
            {
                throw new BusinessRuleException($"Row {rowNumber} has invalid Amount: {amountText}");
            }

            return new InvoiceRecord
            {
                RowNumber = rowNumber,
                InvoiceNo = Value(row, "InvoiceNo"),
                VendorName = Value(row, "VendorName"),
                AmountText = amountText,
                Amount = amount,
                Country = Value(row, "Country"),
                POAvailable = Value(row, "POAvailable"),
                BankChanged = Value(row, "BankChanged")
            };
        }

        public static RiskResult ScoreInvoice(InvoiceRecord invoice, AutomationConfig config, bool isDuplicate)
        {
            var reasons = new List<string>();
            int score = 0;

            if (invoice.Amount > config.HighAmountThreshold)
            {
                score += config.HighAmountRiskScore;
                reasons.Add("Amount above configured threshold");
            }

            if (IsNo(invoice.POAvailable))
            {
                score += config.MissingPoRiskScore;
                reasons.Add("PO not available");
            }

            if (IsYes(invoice.BankChanged))
            {
                score += config.BankChangedRiskScore;
                reasons.Add("Vendor bank details changed");
            }

            if (config.HighRiskCountries.Contains(invoice.Country))
            {
                score += config.HighRiskCountryRiskScore;
                reasons.Add("High risk country");
            }

            if (isDuplicate)
            {
                score += config.DuplicateInvoiceRiskScore;
                reasons.Add("Duplicate invoice number");
            }

            return new RiskResult
            {
                RiskScore = score,
                RiskLevel = GetRiskLevel(score, config),
                Reasons = reasons
            };
        }

        public static InvoiceResult ToBusinessExceptionResult(Dictionary<string, string> row, string message)
        {
            return new InvoiceResult
            {
                InvoiceNo = row.ContainsKey("InvoiceNo") ? row["InvoiceNo"] : string.Empty,
                VendorName = row.ContainsKey("VendorName") ? row["VendorName"] : string.Empty,
                Amount = row.ContainsKey("Amount") ? row["Amount"] : string.Empty,
                Country = row.ContainsKey("Country") ? row["Country"] : string.Empty,
                RiskScore = string.Empty,
                RiskLevel = "BusinessException",
                RiskReasons = message,
                ProcessingStatus = "BusinessException"
            };
        }

        public static InvoiceResult ToSuccessResult(InvoiceRecord invoice, RiskResult risk)
        {
            return new InvoiceResult
            {
                InvoiceNo = invoice.InvoiceNo,
                VendorName = invoice.VendorName,
                Amount = invoice.Amount.ToString(CultureInfo.InvariantCulture),
                Country = invoice.Country,
                RiskScore = risk.RiskScore.ToString(CultureInfo.InvariantCulture),
                RiskLevel = risk.RiskLevel,
                RiskReasons = risk.Reasons.Count == 0 ? "No configured risk indicators" : string.Join("; ", risk.Reasons),
                ProcessingStatus = "Success"
            };
        }

        public static IList<string> ReportHeaders()
        {
            return new List<string>
            {
                "InvoiceNo",
                "VendorName",
                "Amount",
                "Country",
                "RiskScore",
                "RiskLevel",
                "RiskReasons",
                "ProcessingStatus"
            };
        }

        public static IList<IList<string>> ReportRows(IEnumerable<InvoiceResult> results)
        {
            return results
                .Select(result => (IList<string>)new List<string>
                {
                    result.InvoiceNo,
                    result.VendorName,
                    result.Amount,
                    result.Country,
                    result.RiskScore,
                    result.RiskLevel,
                    result.RiskReasons,
                    result.ProcessingStatus
                })
                .ToList();
        }

        private static string Value(Dictionary<string, string> row, string column)
        {
            return row.ContainsKey(column) ? row[column].Trim() : string.Empty;
        }

        private static bool IsYes(string value)
        {
            return string.Equals(value, "Yes", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsNo(string value)
        {
            return string.Equals(value, "No", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetRiskLevel(int score, AutomationConfig config)
        {
            if (score <= config.LowRiskMaxScore)
            {
                return "Low";
            }

            if (score <= config.MediumRiskMaxScore)
            {
                return "Medium";
            }

            return "High";
        }
    }
}
