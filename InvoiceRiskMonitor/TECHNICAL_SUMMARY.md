# InvoiceRiskMonitor Technical Summary

## Overview

InvoiceRiskMonitor is a UiPath C# coded workflow process that reads vendor invoice data from Excel, validates each invoice, calculates a configurable risk score, and writes a risk report to Excel.

## Project Structure

- `project.json` defines the UiPath process project and the `ProcessInvoices.cs` entry point.
- `ProcessInvoices.cs` orchestrates configuration loading, input validation, invoice processing, retry handling, logging, and report writing.
- `ConfigLoader.cs` reads configurable values from `Config.xlsx`.
- `InvoiceRiskEngine.cs` validates invoice rows, applies business rules, calculates risk scores, and shapes report rows.
- `ExcelWorkbook.cs` reads and writes `.xlsx` files using Open XML package parts.
- `RetryPolicy.cs` provides retry behavior for Excel read/write operations.
- `Data/InputInvoices.xlsx` contains sample input invoice data.
- `Output/RiskReport.xlsx` contains the generated risk report.

## Configuration

The automation uses `Config.xlsx` for configurable values, including:

- Input and output file paths
- Input and output sheet names
- High amount threshold
- Risk score weights
- Risk level thresholds
- Retry count and delay
- High-risk country list
- Mandatory invoice columns

## Business Rules Implemented

- Amount greater than `HighAmountThreshold` increases risk.
- `POAvailable = No` increases risk.
- `BankChanged = Yes` increases risk.
- Country listed in `HighRiskCountries` increases risk.
- Duplicate invoice numbers increase risk.
- Missing mandatory fields are treated as business exceptions.
- Invalid or non-positive amount values are treated as business exceptions.

## Output

The generated report is written to `Output/RiskReport.xlsx` with these columns:

- `InvoiceNo`
- `VendorName`
- `Amount`
- `Country`
- `RiskScore`
- `RiskLevel`
- `RiskReasons`
- `ProcessingStatus`

## Run Command Used

```powershell
$p=(Resolve-Path .).Path; uip.cmd rpa run --file-path "$p\ProcessInvoices.cs" --log-level Information
```

## Validation Commands Used

```powershell
$p=(Resolve-Path .).Path; uip.cmd rpa validate --file-path "$p\ProcessInvoices.cs" --project-dir "$p" --output json
```

Result: `No diagnostics found.`

```powershell
$p=(Resolve-Path .).Path; uip.cmd rpa build "$p" --log-level Warn --output json
```

Result: Build succeeded.

## First Run Result

The first validated run completed successfully and generated `Output/RiskReport.xlsx`.

The run processed six invoice rows:

- 4 rows completed with `Success`.
- 1 row was marked as `BusinessException` for invalid amount.
- 1 row was marked as `BusinessException` for missing mandatory `VendorName`.

## Notes

UiPath analyzer returned non-blocking warnings:

- The organization requires an Automation Hub URL.
- `Main.xaml` does not use a Log Message activity. The actual process entry point is `ProcessInvoices.cs`, which logs major processing steps.
