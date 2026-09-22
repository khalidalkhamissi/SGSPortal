# 03 — Solution Architecture Diagram

The solution is a three-tier web application packaged as one deployable: the ASP.NET Core process serves the user
interface (static files) and the REST API, and persists data in MySQL.

```mermaid
flowchart TB
    subgraph Client["Presentation tier — browser"]
        UI["15 HTML pages + nav.js<br/>Arabic / English, RTL / LTR"]
        CH["Chart.js 4.4.1 (local file)"]
        LS[("localStorage<br/>token + profile")]
        UI --- CH
        UI --- LS
    end

    subgraph App["Application tier — SGS Forms (ASP.NET Core 8, Kestrel)"]
        direction TB
        STATIC["Static file host<br/>Forms Source/"]
        subgraph Cross["Cross-cutting"]
            ERRMW["Error handling"]
            SEC["Security headers"]
            JWT["JWT authentication<br/>+ per-request account check"]
            RATE["Login rate limiter"]
            AUTHZ["Permission policies"]
        end
        subgraph API["Controllers"]
            C1["auth"]
            C2["arrival · departure · reports"]
            C3["approvals"]
            C4["coordination"]
            C5["dashboard · export · logs"]
            C6["users · roles · stations"]
        end
        subgraph SVC["Services"]
            S1["AuthService"]
            S2["Arrival / Departure / Report services"]
            S3["ApprovalService"]
            S4["CoordinationService"]
            S5["DashboardService · ExportService (PDF, Excel)"]
            S6["AdminService · RoleService"]
            S7["AuditService"]
        end
        DAL["AppDbContext<br/>EF Core 8 + Pomelo MySQL"]
    end

    subgraph Data["Data tier"]
        DB[("MySQL 8<br/>sgs_forms_db<br/>16 tables")]
    end

    UI -- "GET pages / scripts" --> STATIC
    UI -- "JSON over HTTP(S)<br/>Bearer token" --> Cross
    Cross --> API
    API --> SVC
    SVC --> DAL
    S2 & S3 & S4 & S6 --> S7
    DAL -- "MySQL protocol :3306" --> DB
```

## Component responsibilities
| Tier | Component | Responsibility |
|------|-----------|----------------|
| Presentation | Pages | Forms, lists, dashboard, administration; call the API with `fetch` |
| Presentation | `nav.js` | Menu by permission, escaping, paging, export dialog, notifications |
| Application | Cross-cutting middleware | Error mapping, headers, authentication, rate limiting, authorisation |
| Application | Controllers | Routes and endpoint permissions |
| Application | Services | Validation, business rules, station isolation, audit |
| Application | AppDbContext | Object-relational mapping, migrations |
| Data | MySQL | Persistent storage of forms, approvals (including signatures), users, roles, audit log |

## Design decisions
| Decision | Reason |
|----------|--------|
| One process for UI and API | Simple deployment, no cross-origin configuration (CORS is disabled) |
| Stateless bearer tokens | No server session store; scales horizontally |
| Per-request account check | Deactivation, password change and permission change take effect immediately |
| Permissions instead of role names | Administrators can define new roles without code changes |
| Documents generated on the server | Consistent PDF / Excel layout; permission and station checks apply to exports |
