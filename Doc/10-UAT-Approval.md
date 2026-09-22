# 10 — User Acceptance Testing (UAT) and Approval

| Item | Value |
|------|-------|
| System | SGS Flight Handling Reports |
| Version / build | ______________________ |
| UAT environment URL | ______________________ |
| UAT database | Separate test database (never production) |
| UAT period | From ____________ to ____________ |

## 1. Test accounts
Create one account per role in the UAT environment (or start the UAT copy with `Seed__DemoUsers=true` to get demo
accounts; never in production).

| Role | Account | Station |
|------|---------|---------|
| Administrator | ______________________ | — (all) |
| Management | ______________________ | — (all) |
| Handling Supervisor | ______________________ | e.g. RUH |
| Handling Supervisor (second station) | ______________________ | e.g. JED |

## 2. Test cases
Result: **P** = pass, **F** = fail (write the defect number in Remarks), **N/A** = not applicable.

### A. Sign-in and security
| ID | Scenario | Expected result | Result | Tester | Date | Remarks |
|----|----------|-----------------|--------|--------|------|---------|
| A01 | Sign in with a valid account | Home page for the role; menu shows only permitted items | | | | |
| A02 | Sign in with a wrong password | "Wrong e-mail or password"; no hint whether the e-mail exists | | | | |
| A03 | 11 wrong sign-ins within one minute from one device | 11th attempt refused with "too many attempts" | | | | |
| A04 | Administrator deactivates a signed-in user | That user is signed out on the next action | | | | |
| A05 | Administrator removes a permission from a role | Users of that role lose the function on the next action | | | | |
| A06 | Supervisor of RUH opens a JED report by changing the URL | Access refused | | | | |
| A07 | Enter `<b>test</b>` in a remarks field and view the report | Text is shown literally, not as bold | | | | |
| A08 | Sign out | Returned to the sign-in page; back button does not show data | | | | |

### B. Arrival and departure reports
| ID | Scenario | Expected result | Result | Tester | Date | Remarks |
|----|----------|-----------------|--------|--------|------|---------|
| B01 | Create an arrival report and save as draft | Appears in Drafts with hours left | | | | |
| B02 | Edit the draft and submit | Moves to Pending approval | | | | |
| B03 | Create a departure report with passengers and baggage | Totals calculated correctly (infants not in passenger total) | | | | |
| B04 | Add delays (code, reason, minutes) and staff productivity | Saved and shown when reopened | | | | |
| B05 | Submit without flight number or route | Required-field message; nothing saved | | | | |
| B06 | Enter a negative passenger count via the form | Refused | | | | |
| B07 | Delete a draft | Removed from the list; entry in activity log | | | | |
| B08 | Try to edit or delete an approved report | Refused | | | | |

### C. Airline approval
| ID | Scenario | Expected result | Result | Tester | Date | Remarks |
|----|----------|-----------------|--------|--------|------|---------|
| C01 | Open a pending report, rate 4 stars, sign, approve | Moves to Approved; rating and signature stored | | | | |
| C02 | Approve without rating or signature | Message; not approved | | | | |
| C03 | Return a pending report with a reason | Moves to Returned; reason visible to the supervisor | | | | |
| C04 | Correct and resubmit a returned report | Back in Pending approval | | | | |
| C05 | Airline field for a flyadeal flight (e.g. F3123) | Pre-filled as F3 | | | | |
| C06 | View an approved report | Read-only with rating, remarks and signature | | | | |

### D. Coordination sheets
| ID | Scenario | Expected result | Result | Tester | Date | Remarks |
|----|----------|-----------------|--------|--------|------|---------|
| D01 | Create a turnaround sheet with arrival and departure flights | Saved as draft | | | | |
| D02 | Fill activity times | Durations calculated; overnight times handled | | | | |
| D03 | STD / ATD entered | GAIN time calculated (not for terminating) | | | | |
| D04 | Add buses for arrival and departure | Saved and shown in view | | | | |
| D05 | Complete the sheet | Status "Completed" in the list | | | | |

### E. Lists, search and paging
| ID | Scenario | Expected result | Result | Tester | Date | Remarks |
|----|----------|-----------------|--------|--------|------|---------|
| E01 | Open a list with more than 50 reports | 50 per page; page numbers; "showing 1–50 of N" | | | | |
| E02 | Search by flight number | Only matching reports | | | | |
| E03 | Filter by date range, airport and airline | Only matching reports | | | | |
| E04 | Clear filters | Full list again | | | | |

### F. Dashboard and exports
| ID | Scenario | Expected result | Result | Tester | Date | Remarks |
|----|----------|-----------------|--------|--------|------|---------|
| F01 | Open the dashboard for 30 days | KPIs, trend, by station, by airline, satisfaction | | | | |
| F02 | Filter the dashboard by airline F3 | Only flyadeal flights counted | | | | |
| F03 | Export one report to PDF | PDF with all sections, rating and signature | | | | |
| F04 | Excel export: choose Departure, Approved, last month, airline SV | Count shown before export matches the rows in the file | | | | |
| F05 | Excel export of coordination sheets | Two sheets: sheets and activities | | | | |
| F06 | User without export permission | Export buttons hidden | | | | |

### G. Administration
| ID | Scenario | Expected result | Result | Tester | Date | Remarks |
|----|----------|-----------------|--------|--------|------|---------|
| G01 | Create a user with a station and role | User can sign in and sees only that station | | | | |
| G02 | Create a user with a password shorter than 8 characters | Refused | | | | |
| G03 | Create a custom role with selected permissions | Menu of its users matches the permissions | | | | |
| G04 | User-manager (without role administration) tries to create an administrator | Refused | | | | |
| G05 | Deactivate a station | Not offered for new reports; old reports kept | | | | |
| G06 | Open the activity log and search a flight | Matching entries with user, station and time | | | | |

## 3. Development verification (before UAT)
The development team executed automated checks of the same areas on a test database (21 September 2026): security
(escalation, token revocation, station isolation, XSS, rate limiting, headers), workflows, paging and search, exports,
airline codes, dashboard accuracy (old vs new implementation, 23 filter combinations) and load. All passed. These checks
do not replace UAT by business users.

## 4. Defects found during UAT
| # | Test ID | Description | Severity | Status | Fixed in build |
|---|---------|-------------|----------|--------|----------------|
| | | | | | |

## 5. Approval
By signing, the undersigned confirm that the system was tested against the cases above and is accepted for production
use, subject to any conditions listed below.

Conditions / open items: ________________________________________________________________

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Business owner (Operations) | | | |
| Station representative | | | |
| IT / Application owner | | | |
| Information security | | | |
| Project manager | | | |
