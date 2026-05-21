using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UiPath.Core;

namespace InvoiceRiskMonitor
{
    public static class ConfigLoader
    {
        public static AutomationConfig Load(string configPath)
        {
            if (!File.Exists(configPath))
            {
                throw new BusinessRuleException($"Config file was not found: {configPath}");
            }

            Dictionary<string, string> values = ExcelWorkbook.ReadSheet(configPath, "Settings")
                .Where(row => row.ContainsKey("Key"))
                .ToDictionary(
                    row => row["Key"].Trim(),
                    row => row.ContainsKey("Value") ? row["Value"].Trim() : string.Empty,
                    StringComparer.OrdinalIgnoreCase);

            return new AutomationConfig
            {
                InputFilePath = Require(values, "InputFilePath"),
                OutputFilePath = Require(values, "OutputFilePath"),
                InputSheetName = Require(values, "InputSheetName"),
                OutputSheetName = Require(values, "OutputSheetName"),
                HighAmountThreshold = RequireDecimal(values, "HighAmountThreshold"),
                HighAmountRiskScore = RequireInt(values, "HighAmountRiskScore"),
                MissingPoRiskScore = RequireInt(values, "MissingPoRiskScore"),
                BankChangedRiskScore = RequireInt(values, "BankChangedRiskScore"),
                HighRiskCountryRiskScore = RequireInt(values, "HighRiskCountryRiskScore"),
                DuplicateInvoiceRiskScore = RequireInt(values, "DuplicateInvoiceRiskScore"),
                LowRiskMaxScore = RequireInt(values, "LowRiskMaxScore"),
                MediumRiskMaxScore = RequireInt(values, "MediumRiskMaxScore"),
                RetryCount = RequireInt(values, "RetryCount"),
                RetryDelayMs = RequireInt(values, "RetryDelayMs"),
                HighRiskCountries = SplitSet(Require(values, "HighRiskCountries")),
                MandatoryColumns = SplitList(Require(values, "MandatoryColumns"))
            };
        }

        public static string ResolvePath(string baseDirectory, string configuredPath)
        {
            if (Path.IsPathRooted(configuredPath))
            {
                return configuredPath;
            }

            return Path.GetFullPath(Path.Combine(baseDirectory, configuredPath));
        }

        private static string Require(Dictionary<string, string> values, string key)
        {
            if (!values.ContainsKey(key) || string.IsNullOrWhiteSpace(values[key]))
            {
                throw new BusinessRuleException($"Config key '{key}' is missing or blank.");
            }

            return values[key];
        }

        private static int RequireInt(Dictionary<string, string> values, string key)
        {
            string value = Require(values, key);
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
            {
                throw new BusinessRuleException($"Config key '{key}' must be an integer. Current value: {value}");
            }

            return result;
        }

        private static decimal RequireDecimal(Dictionary<string, string> values, string key)
        {
            string value = Require(values, key);
            if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result))
            {
                throw new BusinessRuleException($"Config key '{key}' must be a number. Current value: {value}");
            }

            return result;
        }

        private static HashSet<string> SplitSet(string value)
        {
            return new HashSet<string>(SplitList(value), StringComparer.OrdinalIgnoreCase);
        }

        private static List<string> SplitList(string value)
        {
            return value
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim())
                .Where(item => item.Length > 0)
                .ToList();
        }
    }
}
