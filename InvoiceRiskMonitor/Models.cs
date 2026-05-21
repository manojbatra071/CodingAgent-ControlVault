using System;
using System.Collections.Generic;

namespace InvoiceRiskMonitor
{
    public class AutomationConfig
    {
        public string InputFilePath { get; set; }
        public string OutputFilePath { get; set; }
        public string InputSheetName { get; set; }
        public string OutputSheetName { get; set; }
        public decimal HighAmountThreshold { get; set; }
        public int HighAmountRiskScore { get; set; }
        public int MissingPoRiskScore { get; set; }
        public int BankChangedRiskScore { get; set; }
        public int HighRiskCountryRiskScore { get; set; }
        public int DuplicateInvoiceRiskScore { get; set; }
        public int LowRiskMaxScore { get; set; }
        public int MediumRiskMaxScore { get; set; }
        public int RetryCount { get; set; }
        public int RetryDelayMs { get; set; }
        public HashSet<string> HighRiskCountries { get; set; }
        public List<string> MandatoryColumns { get; set; }
    }

    public class InvoiceRecord
    {
        public int RowNumber { get; set; }
        public string InvoiceNo { get; set; }
        public string VendorName { get; set; }
        public string AmountText { get; set; }
        public decimal Amount { get; set; }
        public string Country { get; set; }
        public string POAvailable { get; set; }
        public string BankChanged { get; set; }
    }

    public class RiskResult
    {
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; }
        public List<string> Reasons { get; set; }
    }

    public class InvoiceResult
    {
        public string InvoiceNo { get; set; }
        public string VendorName { get; set; }
        public string Amount { get; set; }
        public string Country { get; set; }
        public string RiskScore { get; set; }
        public string RiskLevel { get; set; }
        public string RiskReasons { get; set; }
        public string ProcessingStatus { get; set; }
    }
}
