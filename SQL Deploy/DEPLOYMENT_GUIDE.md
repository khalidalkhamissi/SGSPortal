# SGS Forms — Database Deployment Guide (Company Servers)

This folder builds the MySQL database for the SGS Flight Handling Reports app on your
company servers. You will have **two separate environments**, each with its **own database**:

| Environment | Database name (suggested) | Demo accounts? |
|-------------|---------------------------|----------------|
| Test        | `sgs_forms_test`          | Only if `Seed__DemoUsers=true` |
| Production  | `sgs_forms_prod`          | **No**         |

> Keep them on **separate databases** (ideally separate MySQL servers). Never point the
> test app at the production database.

---

## Files

| File | Purpose |
|------|---------|
| `00_create_database.sql` | Creates the database + a least-privilege app user (`sgs_app`). Run once per server as root. |
| `01_schema.sql` | Creates **all tables** + EF migration history. Idempotent (safe to re-run). Generated directly from the app's EF migrations, so it matches the app exactly. |
| `02_seed.sql` | Roles and their permissions (admin, management, data_entry). **Required before** `02_seed_production.sql`, which looks up the admin role. |
| `02_seed_production.sql` | **Production only.** Inserts real stations + **one** admin, so the app does **not** create demo accounts. |

---

## TEST server — steps
1. Edit `00_create_database.sql` → set DB name to `sgs_forms_test`, set a strong `sgs_app` password. Run it as root.
2. Run `01_schema.sql` against `sgs_forms_test`.  *(Or skip and let the app build it — see note.)*
3. Point the test app's connection string at `sgs_forms_test` and start it.
4. On an empty DB the app creates the roles and 3 demo stations. Demo **accounts** are created only when the
   setting `Seed:DemoUsers` is `true` (environment variable `Seed__DemoUsers=true`) — handy for testing.
   **SECURITY:** demo accounts use weak, well-known passwords. Enable them on the TEST
   environment ONLY, and never expose the test server to the internet. Without the setting the app logs a warning
   and waits for an administrator created with `02_seed_production.sql`.

## PRODUCTION server — steps (no demo accounts)
1. Edit `00_create_database.sql` → DB name `sgs_forms_prod`, strong `sgs_app` password. Run as root.
2. Run `01_schema.sql` against `sgs_forms_prod`.
3. Run `02_seed.sql` against `sgs_forms_prod` (creates the roles — without them step 4 silently creates **no** administrator).
4. Edit `02_seed_production.sql`: put your **real stations**, the **real admin email**, and a
   **fresh BCrypt hash** of a UNIQUE strong password you choose (see "Generating a new admin
   password hash" below). There is **no default password**. Run it.
   - **Order matters:** this MUST run *before* the app starts the first time, so the seeder
     sees a non-empty DB and skips demo accounts.
   - Check: `SELECT Email FROM users;` must return your admin e-mail.
5. Start the production app pointed at `sgs_forms_prod`.
6. Log in with the admin password you set, **change it immediately**, then create
   managers/supervisors from **Manage Users**.

> SECURITY: never ship a known/default admin password. Generate a unique password + fresh hash
> per deployment, store the password only in a password manager, and force a change at first login.

---

## App configuration per environment (`appsettings.json`)
Use a **separate** config (or environment variables) for each server — do **not** ship the dev values:

- `ConnectionStrings:Default` → point to the right DB + the `sgs_app` user/password (not `root`).
- `Jwt:Key` → **change to a long random secret** (the shipped value is a dev placeholder). Use a
  *different* key for test vs prod. (≥ 32 random chars.)
- `Urls` → the port the app binds (e.g., `http://localhost:5108`).

Env-var form (overrides appsettings, good for servers):
```
ConnectionStrings__Default="server=DBHOST;port=3306;database=sgs_forms_prod;user=sgs_app;password=...;TreatTinyAsBoolean=false"
Jwt__Key="<long-random-secret>"
```

---

## Recommendations for your setup
1. **Separate DBs + separate JWT keys** for test and prod (above). A test token must never work on prod.
2. **Don't use `root`** for the app — use the `sgs_app` least-privilege user.
3. **Auto-migration vs DBA-controlled:** the app calls `MigrateAsync()` on startup. Two options:
   - *Convenient:* give `sgs_app` DDL rights (the schema GRANT in `00_…`) and let the app build/upgrade the schema itself.
   - *Enterprise/locked-down:* a DBA runs `01_schema.sql` (it includes the migration history, so the app's `MigrateAsync` becomes a no-op) and you can revoke DDL rights from `sgs_app`. Re-run the updated `01_schema.sql` for future versions.
4. **HTTPS:** put the app behind a reverse proxy (IIS / Nginx) with TLS, or your Cloudflare tunnel; don't expose raw Kestrel publicly.
5. **Backups:** schedule regular `mysqldump` of the prod DB (it holds reports, approvals/signatures, and the audit log).
6. **utf8mb4** everywhere (this script uses it) so Arabic station names/remarks store correctly.
7. **No demo accounts in prod:** guaranteed in code — `DbSeeder` creates demo accounts only when `Seed:DemoUsers` is `true` (default `false`).
8. **Time:** the app stores timestamps in **UTC**; display/convert in the UI as needed.

---

## Generating a new admin password hash
The password is stored as a BCrypt hash. To create one for your own password, run a tiny .NET program:
```
dotnet new console -o hashgen && cd hashgen
dotnet add package BCrypt.Net-Next --version 4.0.3
# Program.cs:  Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("YOUR_PASSWORD", 12));
dotnet run
```
Copy the `$2a$...` output into `02_seed_production.sql` (replace `__PASTE_BCRYPT_HASH_HERE__`).
Use work factor **12**. Store the plaintext password only in a password manager.
