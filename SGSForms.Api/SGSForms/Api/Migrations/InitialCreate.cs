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
[Migration("20260618210532_InitialCreate")]
public class InitialCreate : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterDatabase().Annotation("MySql:CharSet", "utf8mb4");
		migrationBuilder.CreateTable("Stations", (ColumnsBuilder table) => new
		{
			Id = table.Column<int>("int").Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
			Code = table.Column<string>("varchar(255)").Annotation("MySql:CharSet", "utf8mb4"),
			NameAr = table.Column<string>("longtext").Annotation("MySql:CharSet", "utf8mb4"),
			NameEn = table.Column<string>("longtext").Annotation("MySql:CharSet", "utf8mb4"),
			IsActive = table.Column<bool>("bit(1)"),
			CreatedAt = table.Column<DateTime>("datetime(6)")
		}, null, table =>
		{
			table.PrimaryKey("PK_Stations", x => x.Id);
		}).Annotation("MySql:CharSet", "utf8mb4");
		migrationBuilder.CreateTable("ArrivalReports", delegate(ColumnsBuilder table)
		{
			OperationBuilder<AddColumnOperation> id = table.Column<int>("int").Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);
			OperationBuilder<AddColumnOperation> stationId = table.Column<int>("int");
			int? maxLength = 20;
			return new
			{
				Id = id,
				StationId = stationId,
				Status = table.Column<string>("varchar(20)", null, maxLength).Annotation("MySql:CharSet", "utf8mb4"),
				FlightNo = table.Column<string>("longtext").Annotation("MySql:CharSet", "utf8mb4"),
				Route = table.Column<string>("longtext").Annotation("MySql:CharSet", "utf8mb4"),
				AircraftReg = table.Column<string>("longtext", null, null, rowVersion: false, null, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
				FlightDate = table.Column<DateOnly>("date"),
				Sta = table.Column<TimeOnly>("time(6)", null, null, rowVersion: false, null, nullable: true),
				Ata = table.Column<TimeOnly>("time(6)", null, null, rowVersion: false, null, nullable: true),
				SupervisorName = table.Column<string>("longtext", null, null, rowVersion: false, null, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
				ReportTime = table.Column<DateTime>("datetime(6)", null, null, rowVersion: false, null, nullable: true),
				GateNo = table.Column<string>("longtext", null, null, rowVersion: false, null, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
				GateOpen = table.Column<TimeOnly>("time(6)", null, null, rowVersion: false, null, nullable: true),
				GateClose = table.Column<TimeOnly>("time(6)", null, null, rowVersion: false, null, nullable: true),
				FirstPaxTime = table.Column<TimeOnly>("time(6)", null, null, rowVersion: false, null, nullable: true),
				LastPaxTime = table.Column<TimeOnly>("time(6)", null, null, rowVersion: false, null, nullable: true),
				TtlPaxArr = table.Column<int>("int"),
				TtlWchr = table.Column<int>("int"),
				ActualPax = table.Column<int>("int"),
				NoShow = table.Column<int>("int"),
				Offloaded = table.Column<int>("int"),
				Remarks = table.Column<string>("longtext", null, null, rowVersion: false, null, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
				CreatedByUserId = table.Column<int>("int"),
				CreatedAt = table.Column<DateTime>("datetime(6)"),
				UpdatedAt = table.Column<DateTime>("datetime(6)"),
				SubmittedAt = table.Column<DateTime>("datetime(6)", null, null, rowVersion: false, null, nullable: true),
				DraftExpiresAt = table.Column<DateTime>("datetime(6)", null, null, rowVersion: false, null, nullable: true)
			};
		}, null, table =>
		{
			table.PrimaryKey("PK_ArrivalReports", x => x.Id);
			table.ForeignKey("FK_ArrivalReports_Stations_StationId", x => x.StationId, "Stations", "Id", null, ReferentialAction.NoAction, ReferentialAction.Restrict);
		}).Annotation("MySql:CharSet", "utf8mb4");
		migrationBuilder.CreateTable("DepartureReports", delegate(ColumnsBuilder table)
		{
			OperationBuilder<AddColumnOperation> id = table.Column<int>("int").Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);
			OperationBuilder<AddColumnOperation> stationId = table.Column<int>("int");
			int? maxLength = 20;
			OperationBuilder<AddColumnOperation> status = table.Column<string>("varchar(20)", null, maxLength).Annotation("MySql:CharSet", "utf8mb4");
			OperationBuilder<AddColumnOperation> flightNo = table.Column<string>("longtext").Annotation("MySql:CharSet", "utf8mb4");
			OperationBuilder<AddColumnOperation> route = table.Column<string>("longtext").Annotation("MySql:CharSet", "utf8mb4");
			OperationBuilder<AddColumnOperation> aircraftReg = table.Column<string>("longtext", null, null, rowVersion: false, null, nullable: true).Annotation("MySql:CharSet", "utf8mb4");
			OperationBuilder<AddColumnOperation> flightDate = table.Column<DateOnly>("date");
			OperationBuilder<AddColumnOperation> std = table.Column<TimeOnly>("time(6)", null, null, rowVersion: false, null, nullable: true);
			OperationBuilder<AddColumnOperation> atd = table.Column<TimeOnly>("time(6)", null, null, rowVersion: false, null, nullable: true);
			OperationBuilder<AddColumnOperation> counterNo = table.Column<string>("longtext", null, null, rowVersion: false, null, nullable: true).Annotation("MySql:CharSet", "utf8mb4");
			OperationBuilder<AddColumnOperation> countersStartedAt = table.Column<TimeOnly>("time(6)", null, null, rowVersion: false, null, nullable: true);
			OperationBuilder<AddColumnOperation> supervisorName = table.Column<string>("longtext", null, null, rowVersion: false, null, nullable: true).Annotation("MySql:CharSet", "utf8mb4");
			OperationBuilder<AddColumnOperation> reportTime = table.Column<DateTime>("datetime(6)", null, null, rowVersion: false, null, nullable: true);
			OperationBuilder<AddColumnOperation> staffCount = table.Column<int>("int");
			OperationBuilder<AddColumnOperation> paxF = table.Column<int>("int");
			OperationBuilder<AddColumnOperation> paxJ = table.Column<int>("int");
			OperationBuilder<AddColumnOperation> paxW = table.Column<int>("int");
			OperationBuilder<AddColumnOperation> paxY = table.Column<int>("int");
			OperationBuilder<AddColumnOperation> paxInf = table.Column<int>("int");
			OperationBuilder<AddColumnOperation> paxHajj = table.Column<int>("int");
			OperationBuilder<AddColumnOperation> paxTotal = table.Column<int>("int");
			OperationBuilder<AddColumnOperation> bagNormal = table.Column<int>("int");
			OperationBuilder<AddColumnOperation> bagWchr = table.Column<int>("int");
			OperationBuilder<AddColumnOperation> bagCbbg = table.Column<int>("int");
			OperationBuilder<AddColumnOperation> bagStcr = table.Column<int>("int");
			OperationBuilder<AddColumnOperation> bagAvih = table.Column<int>("int");
			OperationBuilder<AddColumnOperation> bagVip = table.Column<int>("int");
			OperationBuilder<AddColumnOperation> bagZamzam = table.Column<int>("int");
			OperationBuilder<AddColumnOperation> bagHajj = table.Column<int>("int");
			OperationBuilder<AddColumnOperation> bagTotal = table.Column<int>("int");
			OperationBuilder<AddColumnOperation> excessTickets = table.Column<int>("int");
			maxLength = 12;
			int? scale = 2;
			OperationBuilder<AddColumnOperation> excessSales = table.Column<decimal>("decimal(12,2)", null, null, rowVersion: false, null, nullable: false, null, null, null, null, null, null, maxLength, scale);
			OperationBuilder<AddColumnOperation> boardingGate = table.Column<string>("longtext", null, null, rowVersion: false, null, nullable: true).Annotation("MySql:CharSet", "utf8mb4");
			scale = 20;
			return new
			{
				Id = id,
				StationId = stationId,
				Status = status,
				FlightNo = flightNo,
				Route = route,
				AircraftReg = aircraftReg,
				FlightDate = flightDate,
				Std = std,
				Atd = atd,
				CounterNo = counterNo,
				CountersStartedAt = countersStartedAt,
				SupervisorName = supervisorName,
				ReportTime = reportTime,
				StaffCount = staffCount,
				PaxF = paxF,
				PaxJ = paxJ,
				PaxW = paxW,
				PaxY = paxY,
				PaxInf = paxInf,
				PaxHajj = paxHajj,
				PaxTotal = paxTotal,
				BagNormal = bagNormal,
				BagWchr = bagWchr,
				BagCbbg = bagCbbg,
				BagStcr = bagStcr,
				BagAvih = bagAvih,
				BagVip = bagVip,
				BagZamzam = bagZamzam,
				BagHajj = bagHajj,
				BagTotal = bagTotal,
				ExcessTickets = excessTickets,
				ExcessSales = excessSales,
				BoardingGate = boardingGate,
				BoardingMode = table.Column<string>("varchar(20)", null, scale, rowVersion: false, null, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
				BoardingStarted = table.Column<TimeOnly>("time(6)", null, null, rowVersion: false, null, nullable: true),
				BoardingCompleted = table.Column<TimeOnly>("time(6)", null, null, rowVersion: false, null, nullable: true),
				GateOpened = table.Column<TimeOnly>("time(6)", null, null, rowVersion: false, null, nullable: true),
				GateClosed = table.Column<TimeOnly>("time(6)", null, null, rowVersion: false, null, nullable: true),
				TotalBuses = table.Column<int>("int"),
				SpecialHandling = table.Column<string>("longtext", null, null, rowVersion: false, null, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
				ActualPax = table.Column<int>("int"),
				NoShow = table.Column<int>("int"),
				Offloaded = table.Column<int>("int"),
				Remarks = table.Column<string>("longtext", null, null, rowVersion: false, null, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
				CreatedByUserId = table.Column<int>("int"),
				CreatedAt = table.Column<DateTime>("datetime(6)"),
				UpdatedAt = table.Column<DateTime>("datetime(6)"),
				SubmittedAt = table.Column<DateTime>("datetime(6)", null, null, rowVersion: false, null, nullable: true),
				DraftExpiresAt = table.Column<DateTime>("datetime(6)", null, null, rowVersion: false, null, nullable: true)
			};
		}, null, table =>
		{
			table.PrimaryKey("PK_DepartureReports", x => x.Id);
			table.ForeignKey("FK_DepartureReports_Stations_StationId", x => x.StationId, "Stations", "Id", null, ReferentialAction.NoAction, ReferentialAction.Restrict);
		}).Annotation("MySql:CharSet", "utf8mb4");
		migrationBuilder.CreateTable("Users", delegate(ColumnsBuilder table)
		{
			OperationBuilder<AddColumnOperation> id = table.Column<int>("int").Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);
			OperationBuilder<AddColumnOperation> name = table.Column<string>("longtext").Annotation("MySql:CharSet", "utf8mb4");
			OperationBuilder<AddColumnOperation> email = table.Column<string>("varchar(255)").Annotation("MySql:CharSet", "utf8mb4");
			OperationBuilder<AddColumnOperation> passwordHash = table.Column<string>("longtext").Annotation("MySql:CharSet", "utf8mb4");
			int? maxLength = 20;
			return new
			{
				Id = id,
				Name = name,
				Email = email,
				PasswordHash = passwordHash,
				Role = table.Column<string>("varchar(20)", null, maxLength).Annotation("MySql:CharSet", "utf8mb4"),
				StationId = table.Column<int>("int", null, null, rowVersion: false, null, nullable: true),
				IsActive = table.Column<bool>("bit(1)"),
				LastLogin = table.Column<DateTime>("datetime(6)", null, null, rowVersion: false, null, nullable: true),
				CreatedAt = table.Column<DateTime>("datetime(6)"),
				UpdatedAt = table.Column<DateTime>("datetime(6)")
			};
		}, null, table =>
		{
			table.PrimaryKey("PK_Users", x => x.Id);
			table.ForeignKey("FK_Users_Stations_StationId", x => x.StationId, "Stations", "Id", null, ReferentialAction.NoAction, ReferentialAction.SetNull);
		}).Annotation("MySql:CharSet", "utf8mb4");
		migrationBuilder.CreateTable("ArrivalDelays", (ColumnsBuilder table) => new
		{
			Id = table.Column<int>("int").Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
			ArrivalReportId = table.Column<int>("int"),
			Code = table.Column<string>("longtext", null, null, rowVersion: false, null, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
			Reason = table.Column<string>("longtext", null, null, rowVersion: false, null, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
			DurationMinutes = table.Column<int>("int")
		}, null, table =>
		{
			table.PrimaryKey("PK_ArrivalDelays", x => x.Id);
			table.ForeignKey("FK_ArrivalDelays_ArrivalReports_ArrivalReportId", x => x.ArrivalReportId, "ArrivalReports", "Id", null, ReferentialAction.NoAction, ReferentialAction.Cascade);
		}).Annotation("MySql:CharSet", "utf8mb4");
		migrationBuilder.CreateTable("ArrivalServices", (ColumnsBuilder table) => new
		{
			Id = table.Column<int>("int").Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
			ArrivalReportId = table.Column<int>("int"),
			Name = table.Column<string>("longtext").Annotation("MySql:CharSet", "utf8mb4"),
			Count = table.Column<int>("int")
		}, null, table =>
		{
			table.PrimaryKey("PK_ArrivalServices", x => x.Id);
			table.ForeignKey("FK_ArrivalServices_ArrivalReports_ArrivalReportId", x => x.ArrivalReportId, "ArrivalReports", "Id", null, ReferentialAction.NoAction, ReferentialAction.Cascade);
		}).Annotation("MySql:CharSet", "utf8mb4");
		migrationBuilder.CreateTable("Approvals", delegate(ColumnsBuilder table)
		{
			OperationBuilder<AddColumnOperation> id = table.Column<int>("int").Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);
			int? maxLength = 20;
			OperationBuilder<AddColumnOperation> reportKind = table.Column<string>("varchar(20)", null, maxLength).Annotation("MySql:CharSet", "utf8mb4");
			OperationBuilder<AddColumnOperation> stationId = table.Column<int>("int");
			OperationBuilder<AddColumnOperation> arrivalReportId = table.Column<int>("int", null, null, rowVersion: false, null, nullable: true);
			OperationBuilder<AddColumnOperation> departureReportId = table.Column<int>("int", null, null, rowVersion: false, null, nullable: true);
			OperationBuilder<AddColumnOperation> satisfaction = table.Column<int>("int");
			OperationBuilder<AddColumnOperation> airlineRemarks = table.Column<string>("longtext", null, null, rowVersion: false, null, nullable: true).Annotation("MySql:CharSet", "utf8mb4");
			OperationBuilder<AddColumnOperation> airlineRepName = table.Column<string>("longtext").Annotation("MySql:CharSet", "utf8mb4");
			OperationBuilder<AddColumnOperation> airlineCompany = table.Column<string>("longtext").Annotation("MySql:CharSet", "utf8mb4");
			OperationBuilder<AddColumnOperation> airlineRepPosition = table.Column<string>("longtext", null, null, rowVersion: false, null, nullable: true).Annotation("MySql:CharSet", "utf8mb4");
			OperationBuilder<AddColumnOperation> signatureImage = table.Column<string>("LONGTEXT", null, null, rowVersion: false, null, nullable: true).Annotation("MySql:CharSet", "utf8mb4");
			maxLength = 20;
			return new
			{
				Id = id,
				ReportKind = reportKind,
				StationId = stationId,
				ArrivalReportId = arrivalReportId,
				DepartureReportId = departureReportId,
				Satisfaction = satisfaction,
				AirlineRemarks = airlineRemarks,
				AirlineRepName = airlineRepName,
				AirlineCompany = airlineCompany,
				AirlineRepPosition = airlineRepPosition,
				SignatureImage = signatureImage,
				Decision = table.Column<string>("varchar(20)", null, maxLength).Annotation("MySql:CharSet", "utf8mb4"),
				ReturnReason = table.Column<string>("longtext", null, null, rowVersion: false, null, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
				ApprovedByUserId = table.Column<int>("int"),
				ApprovedAt = table.Column<DateTime>("datetime(6)")
			};
		}, null, table =>
		{
			table.PrimaryKey("PK_Approvals", x => x.Id);
			table.ForeignKey("FK_Approvals_ArrivalReports_ArrivalReportId", x => x.ArrivalReportId, "ArrivalReports", "Id", null, ReferentialAction.NoAction, ReferentialAction.Cascade);
			table.ForeignKey("FK_Approvals_DepartureReports_DepartureReportId", x => x.DepartureReportId, "DepartureReports", "Id", null, ReferentialAction.NoAction, ReferentialAction.Cascade);
		}).Annotation("MySql:CharSet", "utf8mb4");
		migrationBuilder.CreateTable("DepartureDelays", (ColumnsBuilder table) => new
		{
			Id = table.Column<int>("int").Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
			DepartureReportId = table.Column<int>("int"),
			Code = table.Column<string>("longtext", null, null, rowVersion: false, null, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
			Reason = table.Column<string>("longtext", null, null, rowVersion: false, null, nullable: true).Annotation("MySql:CharSet", "utf8mb4"),
			DurationMinutes = table.Column<int>("int")
		}, null, table =>
		{
			table.PrimaryKey("PK_DepartureDelays", x => x.Id);
			table.ForeignKey("FK_DepartureDelays_DepartureReports_DepartureReportId", x => x.DepartureReportId, "DepartureReports", "Id", null, ReferentialAction.NoAction, ReferentialAction.Cascade);
		}).Annotation("MySql:CharSet", "utf8mb4");
		migrationBuilder.CreateTable("StaffProductivity", (ColumnsBuilder table) => new
		{
			Id = table.Column<int>("int").Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
			DepartureReportId = table.Column<int>("int"),
			StaffName = table.Column<string>("longtext").Annotation("MySql:CharSet", "utf8mb4"),
			PassengersServed = table.Column<int>("int"),
			Comments = table.Column<string>("longtext", null, null, rowVersion: false, null, nullable: true).Annotation("MySql:CharSet", "utf8mb4")
		}, null, table =>
		{
			table.PrimaryKey("PK_StaffProductivity", x => x.Id);
			table.ForeignKey("FK_StaffProductivity_DepartureReports_DepartureReportId", x => x.DepartureReportId, "DepartureReports", "Id", null, ReferentialAction.NoAction, ReferentialAction.Cascade);
		}).Annotation("MySql:CharSet", "utf8mb4");
		migrationBuilder.CreateIndex("IX_Approvals_ArrivalReportId", "Approvals", "ArrivalReportId");
		migrationBuilder.CreateIndex("IX_Approvals_DepartureReportId", "Approvals", "DepartureReportId");
		migrationBuilder.CreateIndex("IX_Approvals_ReportKind_ArrivalReportId_DepartureReportId", "Approvals", new string[3] { "ReportKind", "ArrivalReportId", "DepartureReportId" });
		migrationBuilder.CreateIndex("IX_ArrivalDelays_ArrivalReportId", "ArrivalDelays", "ArrivalReportId");
		migrationBuilder.CreateIndex("IX_ArrivalReports_FlightDate", "ArrivalReports", "FlightDate");
		migrationBuilder.CreateIndex("IX_ArrivalReports_StationId_Status", "ArrivalReports", new string[2] { "StationId", "Status" });
		migrationBuilder.CreateIndex("IX_ArrivalServices_ArrivalReportId", "ArrivalServices", "ArrivalReportId");
		migrationBuilder.CreateIndex("IX_DepartureDelays_DepartureReportId", "DepartureDelays", "DepartureReportId");
		migrationBuilder.CreateIndex("IX_DepartureReports_FlightDate", "DepartureReports", "FlightDate");
		migrationBuilder.CreateIndex("IX_DepartureReports_StationId_Status", "DepartureReports", new string[2] { "StationId", "Status" });
		migrationBuilder.CreateIndex("IX_StaffProductivity_DepartureReportId", "StaffProductivity", "DepartureReportId");
		migrationBuilder.CreateIndex("IX_Stations_Code", "Stations", "Code", null, unique: true);
		migrationBuilder.CreateIndex("IX_Users_Email", "Users", "Email", null, unique: true);
		migrationBuilder.CreateIndex("IX_Users_StationId", "Users", "StationId");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable("Approvals");
		migrationBuilder.DropTable("ArrivalDelays");
		migrationBuilder.DropTable("ArrivalServices");
		migrationBuilder.DropTable("DepartureDelays");
		migrationBuilder.DropTable("StaffProductivity");
		migrationBuilder.DropTable("Users");
		migrationBuilder.DropTable("ArrivalReports");
		migrationBuilder.DropTable("DepartureReports");
		migrationBuilder.DropTable("Stations");
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
			b.Property<int>("PaxF").HasColumnType("int");
			b.Property<int>("PaxHajj").HasColumnType("int");
			b.Property<int>("PaxInf").HasColumnType("int");
			b.Property<int>("PaxJ").HasColumnType("int");
			b.Property<int>("PaxTotal").HasColumnType("int");
			b.Property<int>("PaxW").HasColumnType("int");
			b.Property<int>("PaxY").HasColumnType("int");
			b.Property<string>("Remarks").HasColumnType("longtext");
			b.Property<DateTime?>("ReportTime").HasColumnType("datetime(6)");
			b.Property<string>("Route").IsRequired().HasColumnType("longtext");
			b.Property<string>("SpecialHandling").HasColumnType("longtext");
			b.Property<int>("StaffCount").HasColumnType("int");
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
			b.ToTable("DepartureReports");
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
			b.Property<string>("Role").IsRequired().HasMaxLength(20)
				.HasColumnType("varchar(20)");
			b.Property<int?>("StationId").HasColumnType("int");
			b.Property<DateTime>("UpdatedAt").HasColumnType("datetime(6)");
			b.HasKey("Id");
			b.HasIndex("Email").IsUnique();
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
		modelBuilder.Entity("SGSForms.Api.Entities.StaffProductivity", delegate(EntityTypeBuilder b)
		{
			b.HasOne("SGSForms.Api.Entities.DepartureReport", "DepartureReport").WithMany("StaffProductivity").HasForeignKey("DepartureReportId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.Navigation("DepartureReport");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.User", delegate(EntityTypeBuilder b)
		{
			b.HasOne("SGSForms.Api.Entities.Station", "Station").WithMany("Users").HasForeignKey("StationId")
				.OnDelete(DeleteBehavior.SetNull);
			b.Navigation("Station");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.ArrivalReport", delegate(EntityTypeBuilder b)
		{
			b.Navigation("Delays");
			b.Navigation("Services");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.DepartureReport", delegate(EntityTypeBuilder b)
		{
			b.Navigation("Delays");
			b.Navigation("StaffProductivity");
		});
		modelBuilder.Entity("SGSForms.Api.Entities.Station", delegate(EntityTypeBuilder b)
		{
			b.Navigation("ArrivalReports");
			b.Navigation("DepartureReports");
			b.Navigation("Users");
		});
	}
}
