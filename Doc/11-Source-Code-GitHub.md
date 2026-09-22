# 11 — Source Code to be Uploaded to GitHub

## 1. What goes into the repository
The repository root is the `Decompiled Source` folder.

| Path | Include | Notes |
|------|---------|-------|
| `SGSForms.Api/` (code, `Forms Source/`, `appsettings.json`) | Yes | `appsettings.json` holds no secrets |
| `SGSForms.Api/appsettings.Production.example.json` | Yes | Template for each environment's secrets |
| `lib/` | Yes | Third-party assemblies the project references (≈ 18 MB) |
| `SQL Deploy/` | Yes | Database creation, schema, roles and first-admin scripts — **no data rows** |
| `Doc/` | Yes | This documentation |
| `run.bat`, `global.json`, `README.md`, `.gitignore` | Yes | `README.md` is the repository front page |
| `SGSForms.Api/appsettings.Production.json` | **No** (ignored) | Real database password and JWT key of an environment |
| `bin/`, `obj/`, `publish/` | **No** (ignored) | Build output |
| `app/` (the original compiled package, outside this folder) | **No** | Keep as the rollback copy |

`.gitignore` is already in the folder with these rules.

## 2. Secrets
| Secret | Where it lives now | In git? |
|--------|--------------------|---------|
| Database connection string | `appsettings.Production.json` on each machine, or env var `ConnectionStrings__Default` | No |
| JWT signing key | `appsettings.Production.json`, or env var `Jwt__Key` | No |
| Admin password | Only as a BCrypt hash in the database | No |

Before the first push, confirm that no secret is staged:
```bash
git grep -n -I -E "password=[^;C]|Jwt.*Key.*[A-Za-z0-9+/]{30,}" -- . ":!Doc"
```
The only expected hits are placeholder lines (`password=YOUR_STRONG_DB_PASSWORD` in `run.bat` and
`password=...` in `SQL Deploy/DEPLOYMENT_GUIDE.md`). Any other hit is a real secret: remove it before committing.
If a secret was ever committed, change the password / key — deleting the file later does not remove it from the history.

## 3. Upload steps
Create an **empty private** repository on GitHub first (no README, no .gitignore), then from the `Decompiled Source`
folder:

```bash
git init -b main
git add .
git status
git commit -m "SGS Flight Handling Reports: recovered source, security fixes, documentation"
git remote add origin https://github.com/<organisation>/<repository>.git
git push -u origin main
```
Check `git status` before committing: `appsettings.Production.json`, `bin/` and `obj/` must not be listed.

## 4. Repository settings
| Setting | Recommendation |
|---------|----------------|
| Visibility | **Private** — the code carries the company's branding and internal business rules |
| Branch protection on `main` | Require a pull request and one review; block force-push |
| Secret scanning / push protection | Enable (GitHub Advanced Security, or the free secret scanning on public repositories) |
| Dependabot alerts | Enable; note that `lib/` assemblies are not tracked by Dependabot (see §6) |
| Access | Team-based, least privilege |

## 5. Building from the repository
```bash
cd SGSForms.Api
dotnet build
dotnet publish -c Release -r win-x64 --self-contained true -o ../publish
```
Requires the .NET 8 SDK (`global.json` pins 8.0.412 with roll-forward). The output in `publish/` is what gets installed on the server.

## 6. Recommended follow-up
| Item | Why |
|------|-----|
| Replace `lib/*.dll` references with NuGet `PackageReference`s (same versions: EF Core 8.0.10, Pomelo 8.0.2, ClosedXML 0.104.1, PDFsharp-MigraDoc 6.2.4, BCrypt.Net-Next 4.0.3, JwtBearer 8.0.10; `System.IO.Packaging` to ≥ 8.0.1) | Smaller repository, automatic security alerts, easy upgrades |
| Add a CI workflow (GitHub Actions: `dotnet build` on every pull request) | Catches build breaks before deployment |
| Add automated tests | The project has none; the checks run during the review were manual scripts |
| Add a `LICENSE` / third-party notices file | The libraries are MIT / permissive and require their notices to be kept |

Ownership note: the code was recovered from a deployed build. Confirm with the business owner that the company holds the
rights to the code before placing it in any repository outside the company.
