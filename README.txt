================================================================
 SGS Flight Handling Reports — Source code (recovered)
================================================================

The C# source in SGSForms.Api\ was recovered by decompiling
app\SGSForms.Api.dll with ILSpy. Original comments are not
recoverable; some local variable names are generic (num2, flag...).

--------------------------------------------------------------
CONTENTS
--------------------------------------------------------------
  SGSForms.Api\                Back-end source (ASP.NET Core 8 Web API)
    SGSForms.Api.csproj        Project file (open in Visual Studio / VS Code)
    Program.cs                 Startup and configuration
    SGSForms\Api\...           Controllers, Services, Entities, Auth, Data, Migrations
    Forms Source\              Front-end UI (HTML / CSS / JS)
    appsettings.json           Non-secret settings (port, JWT issuer, logging)
    appsettings.Production.json  Secrets of this machine (DB password, JWT key)
                               NOT in git — copy appsettings.Production.example.json
  lib\                         Third-party DLLs the project references
                               (EF Core, Pomelo MySQL, ClosedXML, PdfSharp, JWT...)
  SQL Deploy\                  Database creation scripts + DEPLOYMENT_GUIDE.md
  Doc\                         Technical documentation (HLD, LLD, architecture
                               diagrams, database design, server requirements,
                               UAT approval, GitHub, code review)
  .gitignore                   Keeps build output and secrets out of git
  global.json                  Pins the .NET SDK (8.0.412, rolls forward)
  run.bat                      Build and run from source

--------------------------------------------------------------
REQUIREMENTS
--------------------------------------------------------------
  - .NET SDK 8 or newer
  - MySQL Server with the database built from "SQL Deploy"

--------------------------------------------------------------
RUN
--------------------------------------------------------------
  Double-click run.bat   -> http://localhost:5200
  or:  cd SGSForms.Api  &&  dotnet run

--------------------------------------------------------------
SETTINGS (appsettings.Production.json, or environment variables)
--------------------------------------------------------------
  Jwt__Key               REQUIRED, at least 32 characters. The app refuses
                         to start without it.
  ConnectionStrings__Default
                         MySQL connection (use the sgs_app user, not root).
  Seed__DemoUsers=true   TEST ONLY: creates demo accounts (admin@sgs.sa /
                         admin123 ...) on an empty database. Off by default;
                         on production create the admin with
                         SQL Deploy\02_seed_production.sql.

  Security behaviour:
  - Login is limited to 10 attempts per minute per IP address.
  - Deactivating a user, disabling a role, changing a password or
    changing a role's permissions takes effect immediately (existing
    sessions are re-checked on every request).
  - Passwords must be at least 8 characters.
  - After upgrading from the old app everyone has to sign in once again.

--------------------------------------------------------------
PUBLISH (produce a deployable folder like the original app\)
--------------------------------------------------------------
  cd SGSForms.Api
  dotnet publish -c Release -r win-x64 --self-contained true -o ..\publish
