# 09 — Server Requirements (Web Application and Database)

## 1. Measured capacity
Load test run on 21 September 2026 against a test copy of the application with 2,000 reports (and 100,000 reports for the
dashboard), on one PC hosting both the application (Debug build) and MySQL:

**Test machine:** Windows 11 Pro, Intel Core i5-14400F (10 cores / 16 threads), 32 GB RAM, MySQL 8.0 on the same machine.

Virtual users sent requests back-to-back with no pause (much heavier than real use). Zero errors in every scenario.

| Operation | Concurrent virtual users | Requests / second | Average response |
|-----------|--------------------------|-------------------|------------------|
| List page (50 reports) | 200 | 692 | 272 ms |
| Open one report | 100 | 4,192 | 22 ms |
| Save a draft | 50 | 1,941 | 26 ms |
| PDF export | 20 | 262 | 76 ms |
| Dashboard, 9 months of data | 20 | 50 | 396 ms |
| Dashboard, 30 days (100,000 reports in database) | 1 | — | 8 ms |
| Dashboard, 4 years / 100,000 reports | 1 | — | ≈ 400 ms |
| Static page | 100 | 5,265 | 18 ms |

A real user issues roughly one request every 5–10 seconds, so the test machine can serve **several hundred to about
1,000 simultaneously active users** with a safety margin.

## 2. Web application server
| Item | Minimum (≤ 100 active users) | Recommended (≤ 1,000 active users) |
|------|------------------------------|------------------------------------|
| Operating system | Windows Server 2019 / 2022 (64-bit) | Windows Server 2022 (64-bit) |
| CPU | 2 vCPU | 4–8 vCPU |
| Memory | 4 GB | 8–16 GB |
| Disk | 20 GB (OS excluded) | 40 GB SSD (application ≈ 150 MB, logs, updates) |
| Runtime | Self-contained publish (no .NET install) or ASP.NET Core Runtime 8.0 | same |
| Web front end | IIS with URL Rewrite + ARR, or another reverse proxy, with a TLS certificate | same, or a load balancer with two application servers |
| Fonts | Windows fonts (Arial) — used to render PDFs | same |
| Network | Port 5200 reachable from the reverse proxy only; outbound 3306 to the database | same |

## 3. Database server
| Item | Minimum | Recommended |
|------|---------|-------------|
| Database | MySQL 8.0 (Community or Enterprise) | MySQL 8.0 latest patch |
| Operating system | Windows Server 2019+ or a Linux distribution supported by MySQL 8 | Dedicated server or managed MySQL service |
| CPU | 2 vCPU | 4 vCPU |
| Memory | 4 GB (`innodb_buffer_pool_size` ≈ 2 GB) | 8–16 GB (`innodb_buffer_pool_size` ≈ 50–70 % of RAM) |
| Disk | 50 GB SSD | 100–200 GB SSD; separate volume for backups |
| Character set | utf8mb4 | utf8mb4 |
| Growth | ≈ 0.4 GB / year at 50 reports per day; ≈ 1.5 GB / year at 200 per day (document 08 §5) | |
| Backup | Daily `mysqldump` or volume snapshot, 30-day retention | Daily full + binary logs for point-in-time recovery |

The application and database may share one server for small installations (≤ 100 users): use the recommended memory
(16 GB) and keep MySQL listening on `localhost` only.

## 4. Software prerequisites
| Software | Where | Notes |
|----------|-------|-------|
| MySQL Server 8.0 | Database server | Create the database and `sgs_app` user with `SQL Deploy/00_create_database.sql` |
| .NET 8 SDK | Build machine only | Needed to build from source (`dotnet publish`) |
| Reverse proxy (IIS + ARR or nginx) | Web server | HTTPS and host-name binding |
| TLS certificate | Reverse proxy | Company CA or public CA |
| Modern browser | Users | Current Chrome, Edge, Safari or Firefox |

## 5. Configuration checklist
| Setting | Value |
|---------|-------|
| `ConnectionStrings__Default` (env var) | `server=<db>;port=3306;database=sgs_forms_db;user=sgs_app;password=<secret>;TreatTinyAsBoolean=false` |
| `Jwt__Key` (env var) | Random, at least 32 characters, different per environment |
| `Urls` | `http://127.0.0.1:5200` behind a local reverse proxy |
| `AllowedHosts` | The public host name |
| `Seed__DemoUsers` | `false` in production |
| `ReverseProxy__KnownProxies__0` | IP address of the reverse proxy (e.g. `127.0.0.1`), so the login limit sees real client addresses |
| .NET runtime | Latest .NET 8 patch (the development machine has 8.0.18; later patches contain security fixes) |

## 6. Known limits
| Limit | Impact | Mitigation |
|-------|--------|------------|
| Excel export limited to 20,000 records per file | Very large exports are refused | Export by period (e.g. per quarter) |
| Login rate limit is 10 attempts per minute **per IP address** | Many users behind one NAT address signing in within the same minute may be asked to wait | Raise the per-IP limit or change it to per account (see document 12, open items) |
| Dashboard reads one light row per report in the period | ≈ 0.4 s for 100,000 reports; grows linearly beyond | Acceptable to ≈ 1 million reports; archive older years if needed |
| Single application instance holds the rate-limit counters in memory | Counters are per instance when scaled out | Acceptable; or move rate limiting to the reverse proxy |
