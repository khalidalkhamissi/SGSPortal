using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using SGSForms.Api.Data;

namespace SGSForms.Api.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260819151731_DynamicRolesAndPermissions")]
public class DynamicRolesAndPermissions : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable("Roles", delegate(ColumnsBuilder table)
		{
			OperationBuilder<AddColumnOperation> id = table.Column<int>("int").Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);
			int? maxLength = 40;
			OperationBuilder<AddColumnOperation> key = table.Column<string>("varchar(40)", null, maxLength).Annotation("MySql:CharSet", "utf8mb4");
			maxLength = 80;
			OperationBuilder<AddColumnOperation> nameAr = table.Column<string>("varchar(80)", null, maxLength).Annotation("MySql:CharSet", "utf8mb4");
			maxLength = 80;
			OperationBuilder<AddColumnOperation> nameEn = table.Column<string>("varchar(80)", null, maxLength).Annotation("MySql:CharSet", "utf8mb4");
			maxLength = 20;
			return new
			{
				Id = id,
				Key = key,
				NameAr = nameAr,
				NameEn = nameEn,
				Color = table.Column<string>("varchar(20)", null, maxLength).Annotation("MySql:CharSet", "utf8mb4"),
				IsSystem = table.Column<bool>("bit(1)"),
				IsLocked = table.Column<bool>("bit(1)"),
				IsActive = table.Column<bool>("bit(1)"),
				CreatedAt = table.Column<DateTime>("datetime(6)")
			};
		}, null, table =>
		{
			table.PrimaryKey("PK_Roles", x => x.Id);
		}).Annotation("MySql:CharSet", "utf8mb4");
		migrationBuilder.CreateTable("RolePermissions", delegate(ColumnsBuilder table)
		{
			OperationBuilder<AddColumnOperation> id = table.Column<int>("int").Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);
			OperationBuilder<AddColumnOperation> roleId = table.Column<int>("int");
			int? maxLength = 60;
			return new
			{
				Id = id,
				RoleId = roleId,
				Permission = table.Column<string>("varchar(60)", null, maxLength).Annotation("MySql:CharSet", "utf8mb4")
			};
		}, null, table =>
		{
			table.PrimaryKey("PK_RolePermissions", x => x.Id);
			table.ForeignKey("FK_RolePermissions_Roles_RoleId", x => x.RoleId, "Roles", "Id", null, ReferentialAction.NoAction, ReferentialAction.Cascade);
		}).Annotation("MySql:CharSet", "utf8mb4");
		migrationBuilder.CreateIndex("IX_Roles_Key", "Roles", "Key", null, unique: true);
		migrationBuilder.CreateIndex("IX_RolePermissions_RoleId_Permission", "RolePermissions", new string[2] { "RoleId", "Permission" }, null, unique: true);
		migrationBuilder.Sql("INSERT INTO Roles (`Key`, NameAr, NameEn, Color, IsSystem, IsLocked, IsActive, CreatedAt) VALUES ('admin',      'مدير النظام', 'Admin',               '#7C3AED', 1, 1, 1, UTC_TIMESTAMP()), ('management', 'الإدارة',     'Management',          '#C9A84C', 1, 0, 1, UTC_TIMESTAMP()), ('data_entry', 'مشرف مناولة', 'Handling Supervisor', '#006C4E', 1, 0, 1, UTC_TIMESTAMP());");
		migrationBuilder.Sql("INSERT INTO RolePermissions (RoleId, Permission) SELECT r.Id, p.perm FROM Roles r JOIN (   SELECT 'admin' AS rkey, 'reports.view' AS perm UNION ALL   SELECT 'admin', 'reports.create'           UNION ALL   SELECT 'admin', 'reports.delete'           UNION ALL   SELECT 'admin', 'reports.approve'          UNION ALL   SELECT 'admin', 'reports.pending'          UNION ALL   SELECT 'admin', 'reports.drafts'           UNION ALL   SELECT 'admin', 'reports.returned'         UNION ALL   SELECT 'admin', 'reports.approved'         UNION ALL   SELECT 'admin', 'coordination.view'        UNION ALL   SELECT 'admin', 'coordination.create'      UNION ALL   SELECT 'admin', 'coordination.delete'      UNION ALL   SELECT 'admin', 'export.pdf'               UNION ALL   SELECT 'admin', 'export.excel'             UNION ALL   SELECT 'admin', 'dashboard.view'           UNION ALL   SELECT 'admin', 'logs.view'                UNION ALL   SELECT 'admin', 'admin.users'              UNION ALL   SELECT 'admin', 'admin.roles'              UNION ALL   SELECT 'admin', 'admin.stations'           UNION ALL   SELECT 'admin', 'stations.viewAll'         UNION ALL   SELECT 'management', 'reports.pending'     UNION ALL   SELECT 'management', 'reports.returned'    UNION ALL   SELECT 'management', 'reports.approved'    UNION ALL   SELECT 'management', 'export.pdf'          UNION ALL   SELECT 'management', 'export.excel'        UNION ALL   SELECT 'management', 'dashboard.view'      UNION ALL   SELECT 'management', 'stations.viewAll'    UNION ALL   SELECT 'data_entry', 'reports.view'        UNION ALL   SELECT 'data_entry', 'reports.create'      UNION ALL   SELECT 'data_entry', 'reports.delete'      UNION ALL   SELECT 'data_entry', 'reports.approve'     UNION ALL   SELECT 'data_entry', 'reports.pending'     UNION ALL   SELECT 'data_entry', 'reports.drafts'      UNION ALL   SELECT 'data_entry', 'reports.returned'    UNION ALL   SELECT 'data_entry', 'reports.approved'    UNION ALL   SELECT 'data_entry', 'coordination.view'   UNION ALL   SELECT 'data_entry', 'coordination.create' UNION ALL   SELECT 'data_entry', 'coordination.delete' UNION ALL   SELECT 'data_entry', 'export.pdf'          UNION ALL   SELECT 'data_entry', 'export.excel' ) p ON p.rkey = r.`Key`;");
		migrationBuilder.AddColumn<int>("RoleId", "Users", "int", null, null, rowVersion: false, null, nullable: true);
		migrationBuilder.Sql("UPDATE Users u JOIN Roles r ON r.`Key` = CASE u.Role         WHEN 'Admin'      THEN 'admin'         WHEN 'Management' THEN 'management'         ELSE                   'data_entry' END SET u.RoleId = r.Id;");
		Type typeFromHandle = typeof(int);
		migrationBuilder.AlterColumn<int>("RoleId", "Users", "int", null, null, rowVersion: false, null, nullable: false, null, null, null, typeFromHandle, "int", null, null, oldRowVersion: false, oldNullable: true);
		migrationBuilder.CreateIndex("IX_Users_RoleId", "Users", "RoleId");
		migrationBuilder.AddForeignKey("FK_Users_Roles_RoleId", "Users", "RoleId", "Roles", null, null, "Id", ReferentialAction.NoAction, ReferentialAction.Restrict);
		migrationBuilder.DropColumn("Role", "Users");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		int? maxLength = 20;
		migrationBuilder.AddColumn<string>("Role", "Users", "varchar(20)", null, maxLength, rowVersion: false, null, nullable: false, "").Annotation("MySql:CharSet", "utf8mb4");
		migrationBuilder.Sql("UPDATE Users u JOIN Roles r ON r.Id = u.RoleId SET u.Role = CASE r.`Key`         WHEN 'admin'      THEN 'Admin'         WHEN 'management' THEN 'Management'         ELSE                   'DataEntry' END;");
		migrationBuilder.DropForeignKey("FK_Users_Roles_RoleId", "Users");
		migrationBuilder.DropIndex("IX_Users_RoleId", "Users");
		migrationBuilder.DropColumn("RoleId", "Users");
		migrationBuilder.DropTable("RolePermissions");
		migrationBuilder.DropTable("Roles");
	}

	protected override void BuildTargetModel(ModelBuilder modelBuilder)
	{
		modelBuilder.HasAnnotation("ProductVersion", "8.0.10").HasAnnotation("Relational:MaxIdentifierLength", 64);
		modelBuilder.AutoIncrementColumns();
		modelBuilder.Entity("SGSForms.Api.Entities.Approval", delegate(EntityTypeBuilder b)
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseMySqlIdentityColumn();
			b.Property<string>("AirlineCompany").IsRequired().HasColumnType("longtext");
			b.Property<string>("AirlineRemarks").HasColumnType("longtext");
			b.Property<string>("AirlineRepName").IsRequired().HasColumnType("longtext");
			b.Property<string>("AirlineRepPosition").HasColumnType("longtext");
			b.Property<DateTime>("ApprovedAt").HasColumnType("datetime(6)");
			b.Property<int>("ApprovedByUserId").HasColumnType("int");
			b.Property<int?>("ArrivalReportId").HasColumnType("int");
			b.Property<string>("Decision").IsRequired().HasMaxLength(20)
				.HasColumnType("varchar(20)");
			b.Property<int?>("DepartureReportId").HasColumnType("int");
			b.Property<string>("ReportKind").IsRequired().HasMaxLength(20)
				.HasColumnType("varchar(20)");
			b.Property<string>("ReturnReason").HasColumnType("longtext");
			b.Property<int>("Satisfaction").HasColumnType("int");
			b.Property<string>("SignatureImage").HasColumnType("LONGTEXT");
			b.Property<int>("StationId").HasColumnType("int");
			b.HasKey("Id");
			b.HasIndex("ArrivalReportId");
			b.HasIndex("DepartureReportId");
			b.HasIndex("ReportKind", "ArrivalReportId", "DepartureReportId");
			b.ToTable("Approvals");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.ArrivalDelay", delegate(EntityTypeBuilder b)
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseMySqlIdentityColumn();
			b.Property<int>("ArrivalReportId").HasColumnType("int");
			b.Property<string>("Code").HasColumnType("longtext");
			b.Property<int>("DurationMinutes").HasColumnType("int");
			b.Property<string>("Reason").HasColumnType("longtext");
			b.HasKey("Id");
			b.HasIndex("ArrivalReportId");
			b.ToTable("ArrivalDelays");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.ArrivalReport", delegate(EntityTypeBuilder b)
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseMySqlIdentityColumn();
			b.Property<int>("ActualPax").HasColumnType("int");
			b.Property<string>("AircraftReg").HasColumnType("longtext");
			b.Property<TimeOnly?>("Ata").HasColumnType("time(6)");
			b.Property<int>("BagTotal").HasColumnType("int");
			b.Property<DateTime>("CreatedAt").HasColumnType("datetime(6)");
			b.Property<int>("CreatedByUserId").HasColumnType("int");
			b.Property<DateTime?>("DraftExpiresAt").HasColumnType("datetime(6)");
			b.Property<TimeOnly?>("FirstPaxTime").HasColumnType("time(6)");
			b.Property<DateOnly>("FlightDate").HasColumnType("date");
			b.Property<string>("FlightNo").IsRequired().HasColumnType("longtext");
			b.Property<TimeOnly?>("GateClose").HasColumnType("time(6)");
			b.Property<string>("GateNo").HasColumnType("longtext");
			b.Property<TimeOnly?>("GateOpen").HasColumnType("time(6)");
			b.Property<TimeOnly?>("LastPaxTime").HasColumnType("time(6)");
			b.Property<int>("NoShow").HasColumnType("int");
			b.Property<int>("Offloaded").HasColumnType("int");
			b.Property<int>("PaxVip").HasColumnType("int");
			b.Property<string>("Remarks").HasColumnType("longtext");
			b.Property<DateTime?>("ReportTime").HasColumnType("datetime(6)");
			b.Property<string>("Route").IsRequired().HasColumnType("longtext");
			b.Property<TimeOnly?>("Sta").HasColumnType("time(6)");
			b.Property<int>("StationId").HasColumnType("int");
			b.Property<string>("Status").IsRequired().HasMaxLength(20)
				.HasColumnType("varchar(20)");
			b.Property<DateTime?>("SubmittedAt").HasColumnType("datetime(6)");
			b.Property<string>("SupervisorName").HasColumnType("longtext");
			b.Property<int>("TtlPaxArr").HasColumnType("int");
			b.Property<int>("TtlWchr").HasColumnType("int");
			b.Property<DateTime>("UpdatedAt").HasColumnType("datetime(6)");
			b.HasKey("Id");
			b.HasIndex("FlightDate");
			b.HasIndex("StationId", "Status");
			b.HasIndex("Status", "FlightDate");
			b.ToTable("ArrivalReports");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.ArrivalService", delegate(EntityTypeBuilder b)
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseMySqlIdentityColumn();
			b.Property<int>("ArrivalReportId").HasColumnType("int");
			b.Property<int>("Count").HasColumnType("int");
			b.Property<string>("Name").IsRequired().HasColumnType("longtext");
			b.HasKey("Id");
			b.HasIndex("ArrivalReportId");
			b.ToTable("ArrivalServices");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.AuditLog", delegate(EntityTypeBuilder b)
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseMySqlIdentityColumn();
			b.Property<string>("Action").IsRequired().HasMaxLength(20)
				.HasColumnType("varchar(20)");
			b.Property<DateTime>("At").HasColumnType("datetime(6)");
			b.Property<string>("Details").HasColumnType("longtext");
			b.Property<string>("FlightNo").HasColumnType("longtext");
			b.Property<int?>("ReportId").HasColumnType("int");
			b.Property<string>("ReportKind").IsRequired().HasMaxLength(20)
				.HasColumnType("varchar(20)");
			b.Property<string>("StationCode").HasColumnType("longtext");
			b.Property<int?>("StationId").HasColumnType("int");
			b.Property<int?>("UserId").HasColumnType("int");
			b.Property<string>("UserName").IsRequired().HasColumnType("longtext");
			b.Property<string>("UserRole").IsRequired().HasColumnType("longtext");
			b.HasKey("Id");
			b.HasIndex("At");
			b.ToTable("AuditLogs");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.CoordinationActivity", delegate(EntityTypeBuilder b)
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseMySqlIdentityColumn();
			b.Property<string>("ActivityKey").IsRequired().HasMaxLength(40)
				.HasColumnType("varchar(40)");
			b.Property<TimeOnly?>("ActualFinish").HasColumnType("time(6)");
			b.Property<TimeOnly?>("ActualStart").HasColumnType("time(6)");
			b.Property<int>("CoordinationSheetId").HasColumnType("int");
			b.Property<string>("Remarks").HasColumnType("longtext");
			b.HasKey("Id");
			b.HasIndex("CoordinationSheetId");
			b.ToTable("CoordinationActivities");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.CoordinationBus", delegate(EntityTypeBuilder b)
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseMySqlIdentityColumn();
			b.Property<string>("BusNo").HasColumnType("longtext");
			b.Property<int>("CoordinationSheetId").HasColumnType("int");
			b.Property<string>("Phase").IsRequired().HasMaxLength(10)
				.HasColumnType("varchar(10)");
			b.Property<TimeOnly?>("Time").HasColumnType("time(6)");
			b.HasKey("Id");
			b.HasIndex("CoordinationSheetId");
			b.ToTable("CoordinationBuses");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.CoordinationSheet", delegate(EntityTypeBuilder b)
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseMySqlIdentityColumn();
			b.Property<string>("AcReg").HasColumnType("longtext");
			b.Property<string>("AcType").HasColumnType("longtext");
			b.Property<string>("ArrFlightNo").HasColumnType("longtext");
			b.Property<string>("ArrFrom").HasColumnType("longtext");
			b.Property<int>("ArrPaxF").HasColumnType("int");
			b.Property<int>("ArrPaxJ").HasColumnType("int");
			b.Property<int>("ArrPaxY").HasColumnType("int");
			b.Property<TimeOnly?>("Ata").HasColumnType("time(6)");
			b.Property<TimeOnly?>("Atd").HasColumnType("time(6)");
			b.Property<DateTime>("CreatedAt").HasColumnType("datetime(6)");
			b.Property<int>("CreatedByUserId").HasColumnType("int");
			b.Property<string>("DelayCode").HasColumnType("longtext");
			b.Property<string>("DepFlightNo").HasColumnType("longtext");
			b.Property<int>("DepPaxF").HasColumnType("int");
			b.Property<int>("DepPaxJ").HasColumnType("int");
			b.Property<int>("DepPaxY").HasColumnType("int");
			b.Property<string>("DepTo").HasColumnType("longtext");
			b.Property<int>("DlyAmount").HasColumnType("int");
			b.Property<DateTime?>("DraftExpiresAt").HasColumnType("datetime(6)");
			b.Property<DateOnly>("FlightDate").HasColumnType("date");
			b.Property<string>("GainTime").HasColumnType("longtext");
			b.Property<bool>("Originating").HasColumnType("bit(1)");
			b.Property<string>("Remarks").HasColumnType("longtext");
			b.Property<DateTime?>("ReportTime").HasColumnType("datetime(6)");
			b.Property<TimeOnly?>("Sta").HasColumnType("time(6)");
			b.Property<int>("StationId").HasColumnType("int");
			b.Property<string>("Status").IsRequired().HasMaxLength(20)
				.HasColumnType("varchar(20)");
			b.Property<TimeOnly?>("Std").HasColumnType("time(6)");
			b.Property<DateTime?>("SubmittedAt").HasColumnType("datetime(6)");
			b.Property<string>("SupervisorName").HasColumnType("longtext");
			b.Property<bool>("Terminating").HasColumnType("bit(1)");
			b.Property<bool>("Transit").HasColumnType("bit(1)");
			b.Property<bool>("Turnaround").HasColumnType("bit(1)");
			b.Property<DateTime>("UpdatedAt").HasColumnType("datetime(6)");
			b.HasKey("Id");
			b.HasIndex("FlightDate");
			b.HasIndex("StationId", "Status");
			b.HasIndex("Status", "FlightDate");
			b.ToTable("CoordinationSheets");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.DepartureDelay", delegate(EntityTypeBuilder b)
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseMySqlIdentityColumn();
			b.Property<string>("Code").HasColumnType("longtext");
			b.Property<int>("DepartureReportId").HasColumnType("int");
			b.Property<int>("DurationMinutes").HasColumnType("int");
			b.Property<string>("Reason").HasColumnType("longtext");
			b.HasKey("Id");
			b.HasIndex("DepartureReportId");
			b.ToTable("DepartureDelays");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.DepartureReport", delegate(EntityTypeBuilder b)
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseMySqlIdentityColumn();
			b.Property<int>("ActualPax").HasColumnType("int");
			b.Property<string>("AircraftReg").HasColumnType("longtext");
			b.Property<TimeOnly?>("Atd").HasColumnType("time(6)");
			b.Property<int>("BagAvih").HasColumnType("int");
			b.Property<int>("BagCbbg").HasColumnType("int");
			b.Property<int>("BagHajj").HasColumnType("int");
			b.Property<int>("BagNormal").HasColumnType("int");
			b.Property<int>("BagStcr").HasColumnType("int");
			b.Property<int>("BagTotal").HasColumnType("int");
			b.Property<int>("BagVip").HasColumnType("int");
			b.Property<int>("BagWchr").HasColumnType("int");
			b.Property<int>("BagZamzam").HasColumnType("int");
			b.Property<TimeOnly?>("BoardingCompleted").HasColumnType("time(6)");
			b.Property<string>("BoardingGate").HasColumnType("longtext");
			b.Property<string>("BoardingMode").HasMaxLength(20).HasColumnType("varchar(20)");
			b.Property<TimeOnly?>("BoardingStarted").HasColumnType("time(6)");
			b.Property<string>("CounterNo").HasColumnType("longtext");
			b.Property<TimeOnly?>("CountersStartedAt").HasColumnType("time(6)");
			b.Property<DateTime>("CreatedAt").HasColumnType("datetime(6)");
			b.Property<int>("CreatedByUserId").HasColumnType("int");
			b.Property<DateTime?>("DraftExpiresAt").HasColumnType("datetime(6)");
			b.Property<decimal>("ExcessSales").HasPrecision(12, 2).HasColumnType("decimal(12,2)");
			b.Property<int>("ExcessTickets").HasColumnType("int");
			b.Property<DateOnly>("FlightDate").HasColumnType("date");
			b.Property<string>("FlightNo").IsRequired().HasColumnType("longtext");
			b.Property<TimeOnly?>("GateClosed").HasColumnType("time(6)");
			b.Property<TimeOnly?>("GateOpened").HasColumnType("time(6)");
			b.Property<int>("NoShow").HasColumnType("int");
			b.Property<int>("Offloaded").HasColumnType("int");
			b.Property<int>("PaxExec").HasColumnType("int");
			b.Property<int>("PaxF").HasColumnType("int");
			b.Property<int>("PaxHajj").HasColumnType("int");
			b.Property<int>("PaxInf").HasColumnType("int");
			b.Property<int>("PaxJ").HasColumnType("int");
			b.Property<int>("PaxTotal").HasColumnType("int");
			b.Property<int>("PaxVip").HasColumnType("int");
			b.Property<int>("PaxW").HasColumnType("int");
			b.Property<int>("PaxY").HasColumnType("int");
			b.Property<string>("Remarks").HasColumnType("longtext");
			b.Property<DateTime?>("ReportTime").HasColumnType("datetime(6)");
			b.Property<string>("Route").IsRequired().HasColumnType("longtext");
			b.Property<string>("SpecialHandling").HasColumnType("longtext");
			b.Property<int>("StationId").HasColumnType("int");
			b.Property<string>("Status").IsRequired().HasMaxLength(20)
				.HasColumnType("varchar(20)");
			b.Property<TimeOnly?>("Std").HasColumnType("time(6)");
			b.Property<DateTime?>("SubmittedAt").HasColumnType("datetime(6)");
			b.Property<string>("SupervisorName").HasColumnType("longtext");
			b.Property<int>("TotalBuses").HasColumnType("int");
			b.Property<DateTime>("UpdatedAt").HasColumnType("datetime(6)");
			b.HasKey("Id");
			b.HasIndex("FlightDate");
			b.HasIndex("StationId", "Status");
			b.HasIndex("Status", "FlightDate");
			b.ToTable("DepartureReports");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.Role", delegate(EntityTypeBuilder b)
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseMySqlIdentityColumn();
			b.Property<string>("Color").IsRequired().HasMaxLength(20)
				.HasColumnType("varchar(20)");
			b.Property<DateTime>("CreatedAt").HasColumnType("datetime(6)");
			b.Property<bool>("IsActive").HasColumnType("bit(1)");
			b.Property<bool>("IsLocked").HasColumnType("bit(1)");
			b.Property<bool>("IsSystem").HasColumnType("bit(1)");
			b.Property<string>("Key").IsRequired().HasMaxLength(40)
				.HasColumnType("varchar(40)");
			b.Property<string>("NameAr").IsRequired().HasMaxLength(80)
				.HasColumnType("varchar(80)");
			b.Property<string>("NameEn").IsRequired().HasMaxLength(80)
				.HasColumnType("varchar(80)");
			b.HasKey("Id");
			b.HasIndex("Key").IsUnique();
			b.ToTable("Roles");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.RolePermission", delegate(EntityTypeBuilder b)
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseMySqlIdentityColumn();
			b.Property<string>("Permission").IsRequired().HasMaxLength(60)
				.HasColumnType("varchar(60)");
			b.Property<int>("RoleId").HasColumnType("int");
			b.HasKey("Id");
			b.HasIndex("RoleId", "Permission").IsUnique();
			b.ToTable("RolePermissions");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.StaffProductivity", delegate(EntityTypeBuilder b)
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseMySqlIdentityColumn();
			b.Property<string>("Comments").HasColumnType("longtext");
			b.Property<int>("DepartureReportId").HasColumnType("int");
			b.Property<int>("PassengersServed").HasColumnType("int");
			b.Property<string>("StaffName").IsRequired().HasColumnType("longtext");
			b.HasKey("Id");
			b.HasIndex("DepartureReportId");
			b.ToTable("StaffProductivity");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.Station", delegate(EntityTypeBuilder b)
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseMySqlIdentityColumn();
			b.Property<string>("Code").IsRequired().HasColumnType("varchar(255)");
			b.Property<DateTime>("CreatedAt").HasColumnType("datetime(6)");
			b.Property<bool>("IsActive").HasColumnType("bit(1)");
			b.Property<string>("NameAr").IsRequired().HasColumnType("longtext");
			b.Property<string>("NameEn").IsRequired().HasColumnType("longtext");
			b.HasKey("Id");
			b.HasIndex("Code").IsUnique();
			b.ToTable("Stations");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.User", delegate(EntityTypeBuilder b)
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseMySqlIdentityColumn();
			b.Property<DateTime>("CreatedAt").HasColumnType("datetime(6)");
			b.Property<string>("Email").IsRequired().HasColumnType("varchar(255)");
			b.Property<bool>("IsActive").HasColumnType("bit(1)");
			b.Property<DateTime?>("LastLogin").HasColumnType("datetime(6)");
			b.Property<string>("Name").IsRequired().HasColumnType("longtext");
			b.Property<string>("PasswordHash").IsRequired().HasColumnType("longtext");
			b.Property<int>("RoleId").HasColumnType("int");
			b.Property<int?>("StationId").HasColumnType("int");
			b.Property<DateTime>("UpdatedAt").HasColumnType("datetime(6)");
			b.HasKey("Id");
			b.HasIndex("Email").IsUnique();
			b.HasIndex("RoleId");
			b.HasIndex("StationId");
			b.ToTable("Users");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.Approval", delegate(EntityTypeBuilder b)
		{
			b.HasOne("SGSForms.Api.Entities.ArrivalReport", "ArrivalReport").WithMany().HasForeignKey("ArrivalReportId")
				.OnDelete(DeleteBehavior.Cascade);
			b.HasOne("SGSForms.Api.Entities.DepartureReport", "DepartureReport").WithMany().HasForeignKey("DepartureReportId")
				.OnDelete(DeleteBehavior.Cascade);
			b.Navigation("ArrivalReport");
			b.Navigation("DepartureReport");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.ArrivalDelay", delegate(EntityTypeBuilder b)
		{
			b.HasOne("SGSForms.Api.Entities.ArrivalReport", "ArrivalReport").WithMany("Delays").HasForeignKey("ArrivalReportId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.Navigation("ArrivalReport");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.ArrivalReport", delegate(EntityTypeBuilder b)
		{
			b.HasOne("SGSForms.Api.Entities.Station", "Station").WithMany("ArrivalReports").HasForeignKey("StationId")
				.OnDelete(DeleteBehavior.Restrict)
				.IsRequired();
			b.Navigation("Station");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.ArrivalService", delegate(EntityTypeBuilder b)
		{
			b.HasOne("SGSForms.Api.Entities.ArrivalReport", "ArrivalReport").WithMany("Services").HasForeignKey("ArrivalReportId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.Navigation("ArrivalReport");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.CoordinationActivity", delegate(EntityTypeBuilder b)
		{
			b.HasOne("SGSForms.Api.Entities.CoordinationSheet", "CoordinationSheet").WithMany("Activities").HasForeignKey("CoordinationSheetId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.Navigation("CoordinationSheet");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.CoordinationBus", delegate(EntityTypeBuilder b)
		{
			b.HasOne("SGSForms.Api.Entities.CoordinationSheet", "CoordinationSheet").WithMany("Buses").HasForeignKey("CoordinationSheetId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.Navigation("CoordinationSheet");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.CoordinationSheet", delegate(EntityTypeBuilder b)
		{
			b.HasOne("SGSForms.Api.Entities.Station", "Station").WithMany().HasForeignKey("StationId")
				.OnDelete(DeleteBehavior.Restrict)
				.IsRequired();
			b.Navigation("Station");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.DepartureDelay", delegate(EntityTypeBuilder b)
		{
			b.HasOne("SGSForms.Api.Entities.DepartureReport", "DepartureReport").WithMany("Delays").HasForeignKey("DepartureReportId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.Navigation("DepartureReport");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.DepartureReport", delegate(EntityTypeBuilder b)
		{
			b.HasOne("SGSForms.Api.Entities.Station", "Station").WithMany("DepartureReports").HasForeignKey("StationId")
				.OnDelete(DeleteBehavior.Restrict)
				.IsRequired();
			b.Navigation("Station");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.RolePermission", delegate(EntityTypeBuilder b)
		{
			b.HasOne("SGSForms.Api.Entities.Role", "Role").WithMany("Permissions").HasForeignKey("RoleId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.Navigation("Role");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.StaffProductivity", delegate(EntityTypeBuilder b)
		{
			b.HasOne("SGSForms.Api.Entities.DepartureReport", "DepartureReport").WithMany("StaffProductivity").HasForeignKey("DepartureReportId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.Navigation("DepartureReport");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.User", delegate(EntityTypeBuilder b)
		{
			b.HasOne("SGSForms.Api.Entities.Role", "Role").WithMany("Users").HasForeignKey("RoleId")
				.OnDelete(DeleteBehavior.Restrict)
				.IsRequired();
			b.HasOne("SGSForms.Api.Entities.Station", "Station").WithMany("Users").HasForeignKey("StationId")
				.OnDelete(DeleteBehavior.SetNull);
			b.Navigation("Role");
			b.Navigation("Station");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.ArrivalReport", delegate(EntityTypeBuilder b)
		{
			b.Navigation("Delays");
			b.Navigation("Services");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.CoordinationSheet", delegate(EntityTypeBuilder b)
		{
			b.Navigation("Activities");
			b.Navigation("Buses");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.DepartureReport", delegate(EntityTypeBuilder b)
		{
			b.Navigation("Delays");
			b.Navigation("StaffProductivity");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.Role", delegate(EntityTypeBuilder b)
		{
			b.Navigation("Permissions");
			b.Navigation("Users");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.Station", delegate(EntityTypeBuilder b)
		{
			b.Navigation("ArrivalReports");
			b.Navigation("DepartureReports");
			b.Navigation("Users");
		});
	}
}
