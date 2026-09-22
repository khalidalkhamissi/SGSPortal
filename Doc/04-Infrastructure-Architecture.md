# 04 — Infrastructure Architecture Diagram

## 1. Current installation (development / pilot)
Everything runs on one Windows PC. The application is started from `run.bat` (`dotnet run`, Debug build) and stops
when that window is closed.

```mermaid
flowchart LR
    subgraph PC["Windows 11 Pro · Intel i5-14400F (16 threads) · 32 GB RAM"]
        APP["SGS Forms<br/>dotnet run → Kestrel<br/>http://localhost:5200"]
        MY[("MySQL Server 8.0<br/>service MySQL80 · port 3306<br/>sgs_forms_db")]
        APP -- "localhost:3306" --> MY
    end
    U["Users' browsers"] -- "HTTP :5200" --> APP
```

## 2. Recommended production deployment
```mermaid
flowchart TB
    subgraph Users["Users"]
        B1["Station supervisors"]
        B2["Airline representatives<br/>(on the supervisor's device)"]
        B3["Management / administrators"]
    end

    subgraph DMZ["Company network edge"]
        RP["Reverse proxy / TLS termination<br/>IIS + ARR, nginx or load balancer<br/>HTTPS :443 · certificate"]
    end

    subgraph AppZone["Application server — Windows Server 2022"]
        SVC["SGS Forms (published, self-contained win-x64)<br/>Windows service · Kestrel http://127.0.0.1:5200<br/>env vars: ConnectionStrings__Default, Jwt__Key"]
        LOGS["Logs (Windows Event Log / file)"]
        SVC --- LOGS
    end

    subgraph DbZone["Database server"]
        MYSQL[("MySQL 8.0 · InnoDB · utf8mb4<br/>sgs_forms_db · user sgs_app")]
        BK["Nightly backup<br/>mysqldump / snapshot → backup storage"]
        MYSQL --- BK
    end

    Users -- "HTTPS 443" --> RP
    RP -- "HTTP 5200 (internal)" --> SVC
    SVC -- "TCP 3306" --> MYSQL
```

## 3. Components
| Component | Specification |
|-----------|---------------|
| Application runtime | .NET 8 (8.0.18); published self-contained so no .NET install is needed on the server |
| Web server | Kestrel inside the application, behind a reverse proxy that provides HTTPS |
| Process management | Windows service or scheduled task with automatic restart (see `install-service.ps1` of the original package) |
| Database | MySQL Community / Enterprise 8.0, InnoDB, utf8mb4 |
| Secrets | Machine environment variables `ConnectionStrings__Default` and `Jwt__Key` (or `appsettings.Production.json` with restricted file permissions) |
| Backups | Daily full backup of `sgs_forms_db`; keep at least 30 days; test a restore quarterly |
| Monitoring | Windows service state, HTTP check of `/index.html`, disk space of the database volume |

## 4. Environments
| Environment | Database | Demo accounts |
|-------------|----------|---------------|
| Test / UAT | separate database (e.g. `sgs_forms_test`) | Optional: `Seed__DemoUsers=true` |
| Production | `sgs_forms_db` / `sgs_forms_prod` | Never — first administrator via `SQL Deploy/02_seed_production.sql` |

## 5. Availability options
| Level | Setup | Result |
|-------|-------|--------|
| Basic (current design) | One application server + one database server + nightly backup | Restart-on-failure; restore from backup after a server loss |
| Improved | Two application instances behind the load balancer; MySQL replica | Survives loss of one application server; faster database recovery |

Note for more than one application instance: the login rate limit is kept in memory per instance, and all instances
must share the same `Jwt__Key`.
