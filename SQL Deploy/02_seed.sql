-- SGS Forms — بذرة الأدوار والصلاحيات
-- مولَّد من DbSeeder.DefaultPermissions — لا يُحرَّر يدويًا
USE `sgs_forms_db`;

INSERT INTO `roles` (`Key`,`NameAr`,`NameEn`,`Color`,`IsSystem`,`IsLocked`,`IsActive`,`CreatedAt`) VALUES
  ('admin','مدير النظام','Admin','#7C3AED',1,1,1,UTC_TIMESTAMP()),
  ('management','الإدارة','Management','#C9A84C',1,0,1,UTC_TIMESTAMP()),
  ('data_entry','مشرف مناولة','Handling Supervisor','#006C4E',1,0,1,UTC_TIMESTAMP());

INSERT INTO `rolepermissions` (`RoleId`,`Permission`)
  SELECT `Id`,'reports.view' FROM `roles` WHERE `Key`='admin'
UNION ALL
  SELECT `Id`,'reports.create' FROM `roles` WHERE `Key`='admin'
UNION ALL
  SELECT `Id`,'reports.delete' FROM `roles` WHERE `Key`='admin'
UNION ALL
  SELECT `Id`,'reports.approve' FROM `roles` WHERE `Key`='admin'
UNION ALL
  SELECT `Id`,'reports.pending' FROM `roles` WHERE `Key`='admin'
UNION ALL
  SELECT `Id`,'reports.drafts' FROM `roles` WHERE `Key`='admin'
UNION ALL
  SELECT `Id`,'reports.returned' FROM `roles` WHERE `Key`='admin'
UNION ALL
  SELECT `Id`,'reports.approved' FROM `roles` WHERE `Key`='admin'
UNION ALL
  SELECT `Id`,'coordination.view' FROM `roles` WHERE `Key`='admin'
UNION ALL
  SELECT `Id`,'coordination.create' FROM `roles` WHERE `Key`='admin'
UNION ALL
  SELECT `Id`,'coordination.delete' FROM `roles` WHERE `Key`='admin'
UNION ALL
  SELECT `Id`,'export.pdf' FROM `roles` WHERE `Key`='admin'
UNION ALL
  SELECT `Id`,'export.excel' FROM `roles` WHERE `Key`='admin'
UNION ALL
  SELECT `Id`,'dashboard.view' FROM `roles` WHERE `Key`='admin'
UNION ALL
  SELECT `Id`,'logs.view' FROM `roles` WHERE `Key`='admin'
UNION ALL
  SELECT `Id`,'admin.users' FROM `roles` WHERE `Key`='admin'
UNION ALL
  SELECT `Id`,'admin.roles' FROM `roles` WHERE `Key`='admin'
UNION ALL
  SELECT `Id`,'admin.stations' FROM `roles` WHERE `Key`='admin'
UNION ALL
  SELECT `Id`,'stations.viewAll' FROM `roles` WHERE `Key`='admin'
UNION ALL
  SELECT `Id`,'reports.pending' FROM `roles` WHERE `Key`='management'
UNION ALL
  SELECT `Id`,'reports.returned' FROM `roles` WHERE `Key`='management'
UNION ALL
  SELECT `Id`,'reports.approved' FROM `roles` WHERE `Key`='management'
UNION ALL
  SELECT `Id`,'export.pdf' FROM `roles` WHERE `Key`='management'
UNION ALL
  SELECT `Id`,'export.excel' FROM `roles` WHERE `Key`='management'
UNION ALL
  SELECT `Id`,'dashboard.view' FROM `roles` WHERE `Key`='management'
UNION ALL
  SELECT `Id`,'stations.viewAll' FROM `roles` WHERE `Key`='management'
UNION ALL
  SELECT `Id`,'reports.view' FROM `roles` WHERE `Key`='data_entry'
UNION ALL
  SELECT `Id`,'reports.create' FROM `roles` WHERE `Key`='data_entry'
UNION ALL
  SELECT `Id`,'reports.delete' FROM `roles` WHERE `Key`='data_entry'
UNION ALL
  SELECT `Id`,'reports.approve' FROM `roles` WHERE `Key`='data_entry'
UNION ALL
  SELECT `Id`,'reports.pending' FROM `roles` WHERE `Key`='data_entry'
UNION ALL
  SELECT `Id`,'reports.drafts' FROM `roles` WHERE `Key`='data_entry'
UNION ALL
  SELECT `Id`,'reports.returned' FROM `roles` WHERE `Key`='data_entry'
UNION ALL
  SELECT `Id`,'reports.approved' FROM `roles` WHERE `Key`='data_entry'
UNION ALL
  SELECT `Id`,'coordination.view' FROM `roles` WHERE `Key`='data_entry'
UNION ALL
  SELECT `Id`,'coordination.create' FROM `roles` WHERE `Key`='data_entry'
UNION ALL
  SELECT `Id`,'coordination.delete' FROM `roles` WHERE `Key`='data_entry'
UNION ALL
  SELECT `Id`,'export.pdf' FROM `roles` WHERE `Key`='data_entry'
UNION ALL
  SELECT `Id`,'export.excel' FROM `roles` WHERE `Key`='data_entry';

-- تاريخ الهجرات: بدونه يعيد التطبيق تطبيقها على قاعدة مبنيّة أصلًا فتفشل
INSERT INTO `__efmigrationshistory` (`MigrationId`,`ProductVersion`) VALUES
  ('20260618210532_InitialCreate','8.0.10'),
  ('20260621145449_AddArrivalBaggageAndAuditLog','8.0.10'),
  ('20260621154558_RenameStaffCountToPaxExec','8.0.10'),
  ('20260707194818_AddPaxVip','8.0.10'),
  ('20260711153011_AddReportListIndex','8.0.10'),
  ('20260818154138_AddCoordinationSheets','8.0.10'),
  ('20260818193242_DropCoordSvTcToc','8.0.10'),
  ('20260819142141_SimplifyCoordActivities','8.0.10'),
  ('20260819151731_DynamicRolesAndPermissions','8.0.10');
