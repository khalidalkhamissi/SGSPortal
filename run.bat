@echo off
title SGS Forms - Run from source
echo ============================================
echo   SGS Forms - Run from source (port 5200)
echo ============================================
echo.
echo Requires: .NET SDK 8 or newer, and MySQL running with the database
echo configured in SGSForms.Api\appsettings.json (build it once using the
echo "SQL Deploy" scripts).
echo.

REM ============================================================================
REM  SECURITY: keep secrets OUT of appsettings.json. The .NET app reads these
REM  environment variables and they OVERRIDE the file (note the double "__").
REM
REM  set ConnectionStrings__Default=server=localhost;port=3306;database=sgs_forms_db;user=sgs_app;password=YOUR_STRONG_DB_PASSWORD;TreatTinyAsBoolean=false
REM  set Jwt__Key=YOUR_LONG_RANDOM_PER_ENVIRONMENT_SECRET
REM ============================================================================

echo Opening browser at http://localhost:5200 in 15 seconds...
start "" cmd /c "timeout /t 15 >nul & start http://localhost:5200"
echo Building and starting... (press Ctrl+C to stop)
echo.
cd /d "%~dp0SGSForms.Api"
dotnet run -- --urls http://localhost:5200
pause
