# 07 — Data Flow Diagram

## Level 0 — context
```mermaid
flowchart LR
    SUP(["Station supervisor"])
    REP(["Airline representative"])
    MGT(["Management"])
    ADM(["Administrator"])
    SYS["0<br/>SGS Flight Handling Reports"]

    SUP -- "flight handling data, coordination sheets" --> SYS
    SYS -- "lists, drafts, returned reports, PDFs" --> SUP
    SYS -- "report to review" --> REP
    REP -- "rating, remarks, signature / return reason" --> SYS
    SYS -- "dashboard, Excel, PDF" --> MGT
    ADM -- "users, roles, stations" --> SYS
    SYS -- "activity log" --> ADM
```

## Level 1 — processes and data stores
```mermaid
flowchart TB
    SUP(["Supervisor"])
    REP(["Airline rep"])
    MGT(["Management"])
    ADM(["Administrator"])

    P1["1.0 Authenticate"]
    P2["2.0 Record flight handling<br/>(arrival / departure)"]
    P3["3.0 Record coordination sheet"]
    P4["4.0 Approve / return"]
    P5["5.0 Report & analyse<br/>(lists, dashboard)"]
    P6["6.0 Export<br/>(PDF / Excel)"]
    P7["7.0 Administer"]
    P8["8.0 Audit"]

    D1[("D1 Users · Roles · Permissions")]
    D2[("D2 Stations")]
    D3[("D3 Arrival / Departure reports<br/>+ services, delays, staff")]
    D4[("D4 Coordination sheets<br/>+ activities, buses")]
    D5[("D5 Approvals<br/>rating, signature")]
    D6[("D6 Audit log")]

    SUP & REP & MGT & ADM -- "e-mail, password" --> P1
    P1 <--> D1
    P1 -- "token + permissions" --> SUP

    SUP -- "report data" --> P2
    P2 <--> D3
    P2 -- "check station" --> D2
    SUP -- "sheet data" --> P3
    P3 <--> D4

    REP -- "rating, signature / reason" --> P4
    D3 -- "submitted report" --> P4
    P4 -- "status Approved / Returned" --> D3
    P4 --> D5

    D3 & D4 & D5 --> P5
    P5 -- "lists, KPIs, charts" --> MGT
    D3 & D4 & D5 --> P6
    P6 -- ".pdf / .xlsx" --> MGT

    ADM -- "users, roles, stations" --> P7
    P7 <--> D1
    P7 <--> D2

    P2 & P3 & P4 --> P8
    P8 --> D6
    D6 -- "activity log" --> ADM
```

## Sequence — submit, approve, export
```mermaid
sequenceDiagram
    actor S as Supervisor
    actor R as Airline rep
    participant UI as Browser
    participant API as SGS Forms API
    participant DB as MySQL

    S->>UI: fill departure report, press Submit
    UI->>API: POST /departure {submit:true} + token
    API->>DB: check token owner active, permissions
    API->>DB: validate station, INSERT report, delays, staff
    API->>DB: INSERT audit log "submit"
    API-->>UI: {id, status:"submitted"}
    R->>UI: review report, rate 4, sign
    UI->>API: POST /approvals {rating, rep, signature}
    API->>DB: report must be Submitted → set Approved, INSERT approval
    API->>DB: INSERT audit log "approve"
    API-->>UI: {id}
    S->>UI: Export PDF
    UI->>API: GET /export/report/departure/{id}/pdf
    API->>DB: read report + latest approval
    API-->>UI: PDF with rating and signature
```

## Data classification
| Data | Store | Sensitivity | Protection |
|------|-------|-------------|------------|
| Passwords | D1 `users.PasswordHash` | Secret | BCrypt hash only; never returned by the API |
| Signatures | D5 `approvals.SignatureImage` | Personal | Visible only to users with report access for that station |
| Operational flight data | D3, D4 | Internal | Permission + station checks |
| Audit log | D6 | Internal | `logs.view` only; written by the server, not editable from the UI |
