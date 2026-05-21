# ControlVault: Compliance Evidence Agent

**Built for:** UiPath Community Challenge — Build Automations Using UiPath for Coding Agents
**Category:** Compliance / Audit / Banking & Financial Services
**Project Type:** UiPath automation built and refined using a coding agent

---

## 1. Overview

**ControlVault** is a UiPath automation project that helps compliance and audit teams validate monthly control evidence, detect missing or incomplete evidence, identify overdue exceptions, calculate risk levels, and generate an audit-ready evidence pack.

The project demonstrates how **UiPath for Coding Agents** can support the full automation development lifecycle:

```text
Prompt → Build → Run → Validate → Detect Gaps → Generate Reports → Fail → Diagnose → Fix → Rerun
```

The coding agent was used not only to generate the initial UiPath automation, but also to improve validation, handle exceptions, diagnose a controlled failure, and rerun the process successfully.

---

## 2. Business Problem

Compliance and audit teams often spend significant time collecting, validating, and preparing evidence before internal audits, regulatory reviews, and control testing cycles.

Evidence may be spread across multiple files and teams, such as:

- Access review evidence
- Change management records
- SLA exception reports
- Policy exception reports
- Control owner mapping files

This manual process can create several risks:

- Mandatory evidence files may be missing.
- Required columns may not exist.
- Control owners may not be mapped.
- High-risk controls may lack supporting evidence.
- Overdue exceptions may remain unresolved.
- Audit packs may be inconsistent.
- Reviewers may not know which gaps require urgent attention.

**ControlVault** addresses this by automating evidence validation, risk scoring, gap identification, and audit pack generation.

---

## 3. What the Automation Does

The automation reads compliance evidence files from the `Data` folder, validates them against configurable rules from `Config.xlsx`, and creates structured output reports in the `Output` folder.

### Input Files

The automation uses the following sample input files:

```text
Data/Config.xlsx
Data/AccessReview.xlsx
Data/ChangeManagement.xlsx
Data/SLAExceptions.xlsx
Data/ControlOwnerMapping.xlsx
Data/PolicyExceptions.xlsx
```

### Output Files

The automation generates:

```text
Output/AuditEvidencePack.xlsx
Output/MissingEvidenceReport.xlsx
Output/ControlSummary.xlsx
Output/HighRiskReviewQueue.xlsx
```

---

## 4. Key Features

- Config-driven design using `Config.xlsx`
- No hardcoded file paths, thresholds, or rule values
- Validation for missing mandatory files
- Validation for missing required columns
- Risk scoring for compliance gaps
- Classification of controls as Pass, Medium Risk, or High Risk
- Human review queue for high-risk gaps
- Try/Catch exception handling
- Retry logic where suitable
- Meaningful logs for major steps
- Business exception handling for missing evidence and invalid input
- Audit-ready report generation
- Controlled failure recovery demo

---

## 5. Business Rules

The automation applies configurable compliance rules such as:

| Condition                               | Risk Classification |
| --------------------------------------- | ------------------- |
| Missing mandatory evidence file         | High Risk           |
| Missing required column                 | High Risk           |
| Control owner missing                   | Medium Risk         |
| Open exception past due date            | High Risk           |
| High-risk control with missing evidence | High Risk           |
| SLA breach unresolved                   | Medium Risk         |
| Policy exception without approval       | High Risk           |
| Complete evidence with no open gap      | Pass                |

---

## 6. Output Columns

The generated reports include fields such as:

```text
ControlID
ControlName
EvidenceFile
EvidenceStatus
Owner
RiskScore
RiskLevel
RiskReasons
RecommendedAction
HumanReviewRequired
ProcessingStatus
```

High-risk items are not automatically closed. They are added to `HighRiskReviewQueue.xlsx` with:

```text
HumanReviewRequired = Yes
```

This keeps the automation aligned with compliance governance and human oversight.

---

## 7. Project Structure

Recommended project structure:

```text
ControlVaultComplianceAgent/
│
├── Main.xaml
├── project.json
│
├── Data/
│   ├── Config.xlsx
│   ├── AccessReview.xlsx
│   ├── ChangeManagement.xlsx
│   ├── SLAExceptions.xlsx
│   ├── ControlOwnerMapping.xlsx
│   └── PolicyExceptions.xlsx
│
├── Output/
│   ├── AuditEvidencePack.xlsx
│   ├── MissingEvidenceReport.xlsx
│   ├── ControlSummary.xlsx
│   └── HighRiskReviewQueue.xlsx
│
├── Framework/
│   ├── Init.xaml
│   ├── ValidateEvidence.xaml
│   ├── ScoreRisk.xaml
│   ├── GenerateReport.xaml
│   └── EndProcess.xaml
│
└── README.md
```

The exact structure may vary depending on the generated UiPath project, but the solution follows a clean modular workflow approach.

---

## 8. Config.xlsx

`Config.xlsx` stores all configurable values used by the automation.

Example configuration areas:

```text
MandatoryEvidenceFiles
MandatoryColumns
RiskThresholds
OverdueDaysThreshold
HighRiskControlList
InputPaths
OutputPaths
RetryCount
LogLevel
```

Using `Config.xlsx` ensures that thresholds, paths, file names, and rules can be changed without modifying workflow logic.

---

## 9. Coding Agent Usage

The coding agent was used as a UiPath development assistant throughout the lifecycle.

It helped with:

- Creating the UiPath project structure
- Creating sample input files
- Designing `Config.xlsx`
- Implementing configurable validation rules
- Adding missing file validation
- Adding missing column validation
- Implementing compliance risk scoring
- Generating output reports
- Adding Try/Catch handling
- Adding meaningful logs
- Diagnosing a controlled failure
- Improving validation and rerunning the process

The important point is that the coding agent was not used only for code suggestions. It was used to help build, run, debug, fix, and harden the automation.

---

## 10. Demo Flow

The demo follows this lifecycle:

```text
1. A compliance automation requirement is given to the coding agent.
2. The coding agent creates the UiPath project.
3. Config.xlsx and sample evidence files are created.
4. The automation runs and validates compliance evidence.
5. Missing evidence, overdue exceptions, and high-risk gaps are identified.
6. Output reports are generated.
7. A controlled failure is introduced.
8. The coding agent diagnoses the issue.
9. Validation and exception handling are improved.
10. The automation is rerun successfully.
11. High-risk gaps are routed to the human review queue.
```

---

## 11. Controlled Failure Scenario

To demonstrate resilience, a mandatory evidence file or required column can be intentionally removed.

Example:

```text
Rename Data/AccessReview.xlsx to Data/AccessReview_Missing.xlsx
```

Expected behavior:

- The automation detects the missing mandatory evidence file.
- The issue is treated as a business exception.
- The root cause is logged.
- The gap is added to `MissingEvidenceReport.xlsx`.
- Other evidence files continue to be processed where possible.
- The output summary clearly shows the missing evidence issue.

This demonstrates failure detection, graceful handling, and improvement through the coding agent.

---

## 12. Human-in-the-Loop Design

ControlVault does not auto-close high-risk compliance gaps.

If a gap is classified as High Risk, the item is added to:

```text
Output/HighRiskReviewQueue.xlsx
```

This allows a compliance reviewer to take the final decision.

This pattern supports governance by ensuring that sensitive or high-risk compliance outcomes remain under human supervision.

---

## 13. How to Run

### Prerequisites

- UiPath Studio installed
- UiPath Robot available
- UiPath CLI installed, if running from terminal
- Required Excel files available in the `Data` folder

### Optional UiPath CLI Setup

```bash
npm install -g @uipath/cli
uip skills install
```

### Run from UiPath Studio

1. Open the project in UiPath Studio.
2. Verify that all files exist inside the `Data` folder.
3. Open `Config.xlsx` and confirm paths/rules are correct.
4. Run `Main.xaml`.
5. Review generated reports in the `Output` folder.

### Run from CLI

The exact command may depend on your local UiPath CLI setup and project path. Use the command generated or recommended by your coding agent.

Example pattern:

```bash
uip run
```

or:

```bash
uip project run --project-path .
```

If your environment uses a different command, replace the above with the command shown during your successful run.

---

## 14. Expected Output

After a successful run, the automation should generate:

### AuditEvidencePack.xlsx

A consolidated evidence validation report.

### MissingEvidenceReport.xlsx

A report of missing mandatory files, missing columns, or incomplete evidence.

### ControlSummary.xlsx

A summary of total controls processed, passed controls, medium-risk gaps, and high-risk gaps.

### HighRiskReviewQueue.xlsx

A list of high-risk controls requiring human review.

Example summary:

```text
Total controls checked: 25
Passed controls: 18
Medium-risk gaps: 4
High-risk gaps: 3
Human review required: Yes
```

---

## 15. Challenge Highlights

This project demonstrates several important capabilities expected from UiPath for Coding Agents:

| Capability                                         | Demonstrated |
| -------------------------------------------------- | ------------ |
| Build automation from natural language requirement | Yes          |
| Use Config.xlsx for configurable values            | Yes          |
| Avoid hardcoding                                   | Yes          |
| Generate sample data                               | Yes          |
| Validate input files and columns                   | Yes          |
| Add exception handling                             | Yes          |
| Generate reports                                   | Yes          |
| Demonstrate failure recovery                       | Yes          |
| Support human review for high-risk items           | Yes          |
| Use coding agent beyond snippet generation         | Yes          |

---

## 16. Enterprise Value

ControlVault can help compliance and audit teams by:

- Reducing manual evidence checking effort
- Improving audit readiness
- Detecting missing evidence earlier
- Prioritizing high-risk compliance gaps
- Creating consistent evidence packs
- Improving traceability
- Supporting human review for sensitive decisions
- Accelerating compliance automation development using coding agents

The bigger value is the pattern: coding agents can help RPA teams create, test, debug, and harden compliance automations faster while still following configuration, validation, exception handling, and human review practices.

---

## 17. Suggested Forum Submission Summary

ControlVault is a compliance evidence automation built using UiPath for Coding Agents. It validates monthly control evidence files, detects missing or incomplete evidence, scores risk, and generates an audit-ready evidence pack. The coding agent helped create the UiPath project, configure rules through `Config.xlsx`, generate sample input files, add validation and exception handling, produce output reports, and improve the process after a controlled failure. High-risk gaps are routed to a human review queue instead of being auto-closed, making the process suitable for enterprise compliance and audit scenarios.

---

## 18. Disclaimer

This project is a challenge/demo implementation. It demonstrates an enterprise compliance automation pattern and is not a replacement for a full production compliance platform, regulatory audit system, or legal control framework. Production deployment would require organization-specific controls, data security review, access management, audit requirements, and validation.

---

## 19. Author

**Manoj Batra**
Built for the UiPath Community Challenge: Build Automations Using UiPath for Coding Agents.