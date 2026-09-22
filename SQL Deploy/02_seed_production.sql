-- ============================================================================
-- 02_seed_production.sql   (PRODUCTION only)
-- Run AFTER 01_schema.sql and BEFORE starting the app the first time.
--
-- Why before first start: the app auto-seeds DEMO accounts only when the Users
-- table is EMPTY. By inserting real stations + one real admin here first, the
-- app's seeder sees a non-empty DB and SKIPS the demo accounts entirely.
--   => PRODUCTION will have NO demo accounts.
-- (TEST: skip this file and just start the app — it will auto-seed demo data.)
-- ============================================================================

USE `sgs_forms_db`;   -- change to your PROD database name if different

-- ── Real stations (EDIT to your actual stations; add as many as needed) ──
INSERT INTO `stations` (`Code`,`NameAr`,`NameEn`,`IsActive`,`CreatedAt`) VALUES
  ('RUH', 'مطار الملك خالد الدولي',    'King Khalid International',    b'1', UTC_TIMESTAMP(6)),
  ('JED', 'مطار الملك عبدالعزيز الدولي','King Abdulaziz International', b'1', UTC_TIMESTAMP(6)),
  ('DMM', 'مطار الملك فهد الدولي',      'King Fahd International',      b'1', UTC_TIMESTAMP(6));

-- ── ONE real administrator (sees all stations: StationId = NULL) ──
-- SECURITY: do NOT ship a known/default password. Generate a UNIQUE strong
-- password per deployment and a fresh BCrypt hash, then paste it below.
--   1) Choose a strong password (store it only in your password manager).
--   2) Generate a BCrypt hash (work factor >= 12), e.g. on the app machine:
--        - online/offline BCrypt tool, or
--        - PowerShell with the bundled BCrypt.Net-Next.dll, or
--        - see SQL Deploy/DEPLOYMENT_GUIDE.md for the exact command.
--   3) Replace __PASTE_BCRYPT_HASH_HERE__ and the email below.
--   4) Force a password change at first login.
-- Never commit the real hash/password to source control or share this file.
-- المستخدم الأول: الدور يُشار إليه بمفتاحه الأجنبي RoleId لا بنصّ.
-- عمود Role النصّي حُذف في هجرة DynamicRolesAndPermissions.
INSERT INTO `users`
  (`Name`,`Email`,`PasswordHash`,`RoleId`,`StationId`,`IsActive`,`LastLogin`,`CreatedAt`,`UpdatedAt`)
SELECT
  'System Administrator',
  'admin@yourcompany.com',          -- غيّره إلى بريد حقيقي
  '__PASTE_BCRYPT_HASH_HERE__',     -- غيّره: بصمة BCrypt جديدة لكل تثبيت
  `Id`, NULL, b'1', NULL, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)
FROM `roles` WHERE `Key` = 'admin';
