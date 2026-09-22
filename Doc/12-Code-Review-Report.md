# 12 — Code Review Report

| Item | Value |
|------|-------|
| Scope | Entire back end (≈ 5,600 lines of C#, excluding EF migrations) and front end (15 HTML pages, `nav.js`, `Themes.css`, ≈ 7,400 lines) |
| Origin | Source recovered by decompiling the deployed `SGSForms.Api.dll` (ILSpy); line-by-line review followed |
| Method | Manual review of every file; each fix verified by build (0 errors, 0 warnings) and by scripted tests against a disposable test database; browser checks of every page |
| Date | September 2026 |

## 1. Summary
| Category | Findings | Fixed | Open |
|----------|----------|-------|------|
| Security — high | 6 | 6 | 0 |
| Security — medium | 9 | 9 | 0 |
| Correctness | 14 | 14 | 0 |
| Performance | 3 | 3 | 0 |
| Dead code / maintainability | 12 | 12 | 0 |
| Second pass — security | 9 | 9 | 0 |
| Second pass — dead code | 3 | 3 | 0 |
| Recommendations (not defects) | 12 | — | 12 |

Build before review: 194 compiler warnings. After: **0 errors, 0 warnings**.

## 2. Security findings
| ID | Severity | Finding | Fix | Verified by |
|----|----------|---------|-----|-------------|
| SEC-01 | High | Stored cross-site scripting: flight numbers, remarks, names, station names, role names and colours were inserted into pages as HTML (`innerHTML`, attribute values, a JSON object inside a single-quoted `onclick`). A data-entry user could run script in an administrator's browser. | Shared `SGS.esc` escaping on every page; row handlers pass numeric ids only; role colours accepted only as `#RRGGBB` (server and client) | `<img src=x onerror=…>` saved as flight number and delay reason — rendered as text, no element created |
| SEC-02 | High | Privilege escalation: a user with only `admin.users` could create an administrator or reset the administrator's password. | Managers without `admin.roles` can grant / manage only roles whose permissions they hold | Attempt returned 403 |
| SEC-03 | High | Revoked access stayed valid: deactivated users, disabled roles, removed permissions and changed passwords kept working until the token expired. | Every request re-checks the account and reloads permissions; tokens carry a security stamp of the password hash | Old token → 401 immediately after deactivation / password change; removed permission → 403 |
| SEC-04 | High | Demo accounts with well-known passwords (`admin123`) were created automatically on any empty database. | Created only when `Seed:DemoUsers=true` | Start-up on an empty database without the flag creates no users |
| SEC-05 | High | PDF and Excel exports did not check the `export.pdf` / `export.excel` permissions. | Policies on every export endpoint, plus report-reading / list permission per status | Users without the permission → 403 |
| SEC-06 | High | Report lists ignored the per-list permissions, and a user with neither a station nor `stations.viewAll` saw every station's reports. | List permission per status; `StationScope` returns "nothing" for such users | Management → drafts 403; station-less viewer → empty list |
| SEC-07 | Medium | No protection against password guessing. | Login rate limit: 10 attempts / minute / IP (HTTP 429) | 11th attempt → 429 |
| SEC-08 | Medium | CORS allowed any origin. | CORS removed (UI and API are same-origin) | No `Access-Control-Allow-Origin` header |
| SEC-09 | Medium | No browser security headers. | CSP, `X-Frame-Options: DENY`, `nosniff`, `Referrer-Policy`, `Permissions-Policy` | Headers present on every response |
| SEC-10 | Medium | A report could be approved while still a draft, or approved twice. | Approve / return only when the status is Submitted | 409 on draft and on second approval |
| SEC-11 | Medium | Signature accepted any text (stored and later placed in an `<img src>`). | Must be a PNG / JPEG data URL ≤ 2 MB, checked on server and client | `javascript:` signature → 400 |
| SEC-12 | Medium | Login timing revealed whether an e-mail exists (no hash check for unknown users). | Constant-work path using a dummy hash | Code review |
| SEC-13 | Medium | No input limits: negative counts, unlimited text and list sizes, unvalidated station ids (500 errors). | Central `Parse` rules: ranges, lengths, list sizes, station must exist and be active; request body ≤ 5 MB | Negative / invalid values → 400 |
| SEC-14 | Medium | JWT signing key not validated (missing or short key accepted). | Start-up refuses a key shorter than 32 bytes | Code review |
| SEC-15 | Medium | Chart.js loaded from a public CDN on the dashboard. | Uses the local copy already shipped with the UI | Dashboard renders with CSP `script-src 'self'` |

## 3. Correctness findings
| ID | Finding | Fix |
|----|---------|-----|
| COR-01 | Station name never shown in the side menu (not stored at login) | Stored at login |
| COR-02 | Dashboard station filter tied to role names instead of the `stations.viewAll` permission | Uses the permission |
| COR-03 | Airline codes containing a digit (flyadeal `F3`, `6E`) read as one letter everywhere | Shared rule `FlightNo.Prefix` / `SGS.airlineCode` for lists, filters, dashboard, exports and the approval page |
| COR-04 | Invalid dates silently replaced by today's date; invalid times silently dropped | Rejected with a 400 message |
| COR-05 | Duplicate activity keys in a coordination sheet crashed the PDF export (500) | Duplicates and unknown keys ignored on save; export tolerant |
| COR-06 | Empty delay rows saved as data | Skipped |
| COR-07 | Duplicate e-mail on user update caused a database error (500) | Checked; clear message |
| COR-08 | Activity-log filter had no entry for coordination | Added |
| COR-09 | Error middleware could fail when the response had already started | Checks `HasStarted` |
| COR-10 | Audit failures swallowed silently and left a broken entity tracked | Logged; entity detached |
| COR-11 | Logs, reports and coordination accepted any page size (up to 100) and unbounded page numbers | Max 50 rows per page, page ≤ 100,000 |
| COR-12 | The "unknown page" handler (first version during the review) captured every `.html` request | Replaced by a check after static files; all pages verified |
| COR-13 | Nullable reference analysis disabled by the decompiled project (implicit `[Required]` lost) | Re-enabled; all warnings resolved |
| COR-14 | Deployment guide described automatic demo accounts | Updated |

## 4. Performance findings
| ID | Finding | Fix | Result |
|----|---------|-----|--------|
| PERF-01 | Dashboard loaded every approval ever recorded on each request, whatever the period | Only approvals of approved reports inside the period | 30-day dashboard 107 ms → 8 ms (100,000 reports); identical results in 22 of 23 compared cases, the one difference being ties on identical timestamps in synthetic data |
| PERF-02 | Dashboard satisfaction query first written as a correlated sub-query timed out at 44,000 approvals (found in testing, never released) | Replaced by one pass in memory | ≈ 0.4 s for 4 years / 100,000 reports |
| PERF-03 | Export queries tracked entities unnecessarily | `AsNoTracking` | — |

## 5. Dead code and maintainability
| ID | Item | Action |
|----|------|--------|
| DC-01 | `localStorage` fallback in the coordination form (`API_READY`, `lsSave`, …) that could never run | Removed |
| DC-02 | Development API address `http://localhost:8000` in two pages | Removed (same origin) |
| DC-03 | Unused `PermissionClaims`, `StationsAsync()` overload, `ReportKind.Coordination`, `nowStr`, `setHint`, `XPrefix` | Removed |
| DC-04 | Logo base64 embedded twice in the export service | One constant |
| DC-05 | Same prefix function written four times; activity list duplicated | One shared helper each |
| DC-06 | Misleading comments ("demo data", "saved in localStorage") | Corrected |
| DC-07 | Decompiler artefacts (`[CompilerGenerated]`, `_ = 1;`, redundant casts and variables, an anonymous type name that did not compile) | Cleaned |
| DC-08 | `AllowUnsafeBlocks` without unsafe code | Removed |
| DC-09 | Mixed validation spread across services | Centralised in `Parse`, `ReportRules` |
| DC-10 | Permission strings repeated as literals | Constants from `Permissions` |
| DC-11 | Export logic for coordination sheets added in a separate partial file | `ExportService.Coordination.cs` |
| DC-12 | Secrets inside `appsettings.json` | Moved to `appsettings.Production.json` (git-ignored) / environment variables |

## 6. Second review pass (September 2026)
A second full pass after the features added later (search, paging, export dialog, coordination export, dashboard
rewrite, airline codes). Automated dead-code analysis (IDE0005/0051/0052/0059/0060) plus a scan of public members,
request fields and every value inserted into the pages.

| ID | Type | Finding | Action |
|----|------|---------|--------|
| R2-01 | Security | Deployment guide skipped the roles script, so `02_seed_production.sql` inserted **no** administrator; the original app then created demo accounts (`admin123`) on the production database | Guide fixed: run `02_seed.sql` first, then verify the admin row |
| R2-02 | Security | Swagger UI in production (token accepted from the URL and a cookie) | Removed with its 4 libraries; the API is documented in document 02 |
| R2-03 | Security | Behind a reverse proxy every user would share the proxy's IP, and a forged `X-Forwarded-For` could bypass the login limit | `ReverseProxy:KnownProxies` setting; forwarded headers trusted only from those addresses |
| R2-04 | Security | Excel export had no size limit (memory exhaustion with 100,000+ rows) | Limit 20,000 records per file with a clear message |
| R2-05 | Security | Airline filter text copied into the download file name unsanitised | Letters and digits only |
| R2-06 | Security | JWT algorithm not pinned | Only HS256 accepted |
| R2-07 | Security | Failed sign-ins were not logged | Logged as warnings (e-mail only, never the password) |
| R2-08 | Security | App database account `sgs_app@'%'` could connect from any machine | Script now creates `sgs_app@'localhost'` with instructions for a separate app server |
| R2-09 | Security | Session token and unused profile fields exposed on the global `SGS` object | Removed from the object |
| R2-10 | Dead code | `DB/` table dumps were outdated (pre-roles schema) and duplicated `01_schema.sql` | Deleted |
| R2-11 | Dead code | `ER Digram.mwb` Workbench model outdated (no coordination tables) | Deleted; ER diagram in document 08 |
| R2-12 | Dead code | Decompiled `AssemblyInfo.cs`, `CoordinationInput.GainTime` (never read), `token_type` in the login response, `TC_FIELDS`, `airport_id` storage, 4 unused `using` lines | Removed |

Verified: build 0 errors / 0 warnings (including the dead-code analyzers), 21 regression and security checks passed
(tampered and `alg=none` tokens rejected, forged `X-Forwarded-For` cannot bypass the login limit, export file name
sanitised, Swagger gone, export limit enforced at 20,202 records).

Library check: no known vulnerabilities in the JWT libraries (7.1.2 includes the CVE-2024-21319 fix), EF Core 8.0.10,
MySqlConnector 2.3.5 or BCrypt 4.0.3. `System.IO.Packaging` 8.0.0 has CVE-2024-43483/43484 (denial of service when
*reading* crafted packages); the application only *writes* Excel files, so it is not exposed — update it with the move to NuGet.

## 7. Open items and recommendations
| ID | Priority | Recommendation |
|----|----------|----------------|
| REC-01 | High | Serve the system over HTTPS (reverse proxy with certificate) and set `AllowedHosts` to the real host name. |
| REC-02 | High | Run the application with the `sgs_app` MySQL account (not `root`), limit that account to the application server's address, and use a strong password. |
| REC-03 | High | Generate a new JWT key per environment and keep it in an environment variable. |
| REC-04 | Medium | Login rate limit is per IP; users behind one shared address may be throttled at shift start — change to per account or raise the per-IP limit. |
| REC-05 | Medium | Run the published build as a Windows service with automatic restart (today it runs from `run.bat` in Debug). |
| REC-06 | Medium | Drafts show an expiry (24 h) but are never closed or removed — decide the business rule. |
| REC-07 | Medium | Users without a station cannot create reports (the form has no station picker) — add one if administrators must file reports. |
| REC-08 | Medium | Add automated tests and a CI build; replace `lib/` DLLs with NuGet packages. |
| REC-09 | Low | The CSP still needs `'unsafe-inline'` because the pages use inline scripts and handlers; moving them to files would allow a strict policy. |
| REC-10 | Low | The session token is kept in `localStorage`; with XSS fixed this is acceptable, an HttpOnly cookie would be stronger. |
| REC-11 | Medium | Install the latest .NET 8 runtime patch (the machine has 8.0.18); the .NET 6 runtimes on the machine are out of support and can be removed if nothing else uses them. |
| REC-12 | Low | Set `ReverseProxy:KnownProxies` when the reverse proxy is installed. |

## 8. Conclusion
All defects found during the review are fixed and verified. The system is suitable for UAT. Before production, apply
REC-01 to REC-03 and decide REC-04 to REC-07.
