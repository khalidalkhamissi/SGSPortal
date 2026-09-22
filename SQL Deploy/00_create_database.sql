-- ============================================================================
-- 00_create_database.sql   (run ONCE per server, as a MySQL admin / root)
-- SGS Flight Handling Reports — database + dedicated app user
--
-- TEST server:  use database name  sgs_forms_test
-- PROD server:  use database name  sgs_forms_prod   (or sgs_forms_db)
-- Change the name below to match the environment you are creating.
-- ============================================================================

CREATE DATABASE IF NOT EXISTS `sgs_forms_db`
  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;   -- utf8mb4 = required for Arabic

-- ── Dedicated least-privilege application account (do NOT use root for the app) ──
-- Change the password to a strong one and put the SAME value in the app's connection string
-- (appsettings.Production.json or the environment variable ConnectionStrings__Default).
-- Host: 'localhost' when the app runs on this same server. If the app runs on ANOTHER server,
-- replace every 'localhost' below with that server's IP address (never '%' = any machine).
CREATE USER IF NOT EXISTS 'sgs_app'@'localhost' IDENTIFIED BY 'CHANGE_ME_to_a_strong_password';

-- Runtime data access:
GRANT SELECT, INSERT, UPDATE, DELETE ON `sgs_forms_db`.* TO 'sgs_app'@'localhost';

-- Schema rights — ONLY needed if you let the app auto-run EF migrations.
-- If a DBA applies 01_schema.sql manually, you can SKIP this GRANT (more secure).
GRANT CREATE, ALTER, INDEX, DROP, REFERENCES ON `sgs_forms_db`.* TO 'sgs_app'@'localhost';

FLUSH PRIVILEGES;
