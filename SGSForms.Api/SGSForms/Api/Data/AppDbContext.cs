using Microsoft.EntityFrameworkCore;
using SGSForms.Api.Entities;

namespace SGSForms.Api.Data;

public class AppDbContext : DbContext
{
	public DbSet<Station> Stations => Set<Station>();

	public DbSet<User> Users => Set<User>();

	public DbSet<Role> Roles => Set<Role>();

	public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

	public DbSet<ArrivalReport> ArrivalReports => Set<ArrivalReport>();

	public DbSet<ArrivalService> ArrivalServices => Set<ArrivalService>();

	public DbSet<ArrivalDelay> ArrivalDelays => Set<ArrivalDelay>();

	public DbSet<DepartureReport> DepartureReports => Set<DepartureReport>();

	public DbSet<DepartureDelay> DepartureDelays => Set<DepartureDelay>();

	public DbSet<StaffProductivity> StaffProductivity => Set<StaffProductivity>();

	public DbSet<Approval> Approvals => Set<Approval>();

	public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

	public DbSet<CoordinationSheet> CoordinationSheets => Set<CoordinationSheet>();

	public DbSet<CoordinationActivity> CoordinationActivities => Set<CoordinationActivity>();

	public DbSet<CoordinationBus> CoordinationBuses => Set<CoordinationBus>();

	public AppDbContext(DbContextOptions<AppDbContext> options)
		: base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder b)
	{
		base.OnModelCreating(b);
		b.Entity<Role>().Property((Role r) => r.Key).HasMaxLength(40)
			.IsRequired();
		b.Entity<Role>().Property((Role r) => r.NameAr).HasMaxLength(80)
			.IsRequired();
		b.Entity<Role>().Property((Role r) => r.NameEn).HasMaxLength(80)
			.IsRequired();
		b.Entity<Role>().Property((Role r) => r.Color).HasMaxLength(20);
		b.Entity<Role>().HasIndex((Role r) => r.Key).IsUnique();
		b.Entity<RolePermission>().Property((RolePermission p) => p.Permission).HasMaxLength(60)
			.IsRequired();
		b.Entity<RolePermission>().HasIndex((RolePermission p) => new { p.RoleId, p.Permission }).IsUnique();
		b.Entity<RolePermission>().HasOne((RolePermission p) => p.Role).WithMany((Role r) => r.Permissions)
			.HasForeignKey((RolePermission p) => p.RoleId)
			.OnDelete(DeleteBehavior.Cascade);
		b.Entity<ArrivalReport>().Property((ArrivalReport a) => a.Status).HasConversion<string>()
			.HasMaxLength(20);
		b.Entity<DepartureReport>().Property((DepartureReport d) => d.Status).HasConversion<string>()
			.HasMaxLength(20);
		b.Entity<DepartureReport>().Property((DepartureReport d) => d.BoardingMode).HasConversion<string>()
			.HasMaxLength(20);
		b.Entity<Approval>().Property((Approval a) => a.ReportKind).HasConversion<string>()
			.HasMaxLength(20);
		b.Entity<Approval>().Property((Approval a) => a.Decision).HasConversion<string>()
			.HasMaxLength(20);
		b.Entity<Station>().HasIndex((Station s) => s.Code).IsUnique();
		b.Entity<User>().HasIndex((User u) => u.Email).IsUnique();
		b.Entity<User>().HasOne((User u) => u.Station).WithMany((Station s) => s.Users)
			.HasForeignKey((User u) => u.StationId)
			.OnDelete(DeleteBehavior.SetNull);
		b.Entity<User>().HasOne((User u) => u.Role).WithMany((Role r) => r.Users)
			.HasForeignKey((User u) => u.RoleId)
			.OnDelete(DeleteBehavior.Restrict);
		b.Entity<ArrivalReport>().HasIndex((ArrivalReport a) => new { a.StationId, a.Status });
		b.Entity<ArrivalReport>().HasIndex((ArrivalReport a) => a.FlightDate);
		b.Entity<ArrivalReport>().HasIndex((ArrivalReport a) => new { a.Status, a.FlightDate });
		b.Entity<ArrivalReport>().HasOne((ArrivalReport a) => a.Station).WithMany((Station s) => s.ArrivalReports)
			.HasForeignKey((ArrivalReport a) => a.StationId)
			.OnDelete(DeleteBehavior.Restrict);
		b.Entity<ArrivalReport>().HasMany((ArrivalReport a) => a.Services).WithOne((ArrivalService s) => s.ArrivalReport)
			.HasForeignKey((ArrivalService s) => s.ArrivalReportId)
			.OnDelete(DeleteBehavior.Cascade);
		b.Entity<ArrivalReport>().HasMany((ArrivalReport a) => a.Delays).WithOne((ArrivalDelay d) => d.ArrivalReport)
			.HasForeignKey((ArrivalDelay d) => d.ArrivalReportId)
			.OnDelete(DeleteBehavior.Cascade);
		b.Entity<DepartureReport>().HasIndex((DepartureReport d) => new { d.StationId, d.Status });
		b.Entity<DepartureReport>().HasIndex((DepartureReport d) => d.FlightDate);
		b.Entity<DepartureReport>().HasIndex((DepartureReport d) => new { d.Status, d.FlightDate });
		b.Entity<DepartureReport>().Property((DepartureReport d) => d.ExcessSales).HasPrecision(12, 2);
		b.Entity<DepartureReport>().HasOne((DepartureReport d) => d.Station).WithMany((Station s) => s.DepartureReports)
			.HasForeignKey((DepartureReport d) => d.StationId)
			.OnDelete(DeleteBehavior.Restrict);
		b.Entity<DepartureReport>().HasMany((DepartureReport d) => d.Delays).WithOne((DepartureDelay x) => x.DepartureReport)
			.HasForeignKey((DepartureDelay x) => x.DepartureReportId)
			.OnDelete(DeleteBehavior.Cascade);
		b.Entity<DepartureReport>().HasMany((DepartureReport d) => d.StaffProductivity).WithOne((StaffProductivity x) => x.DepartureReport)
			.HasForeignKey((StaffProductivity x) => x.DepartureReportId)
			.OnDelete(DeleteBehavior.Cascade);
		b.Entity<Approval>().Property((Approval a) => a.SignatureImage).HasColumnType("LONGTEXT");
		b.Entity<Approval>().HasIndex((Approval a) => new { a.ReportKind, a.ArrivalReportId, a.DepartureReportId });
		b.Entity<Approval>().HasOne((Approval a) => a.ArrivalReport).WithMany()
			.HasForeignKey((Approval a) => a.ArrivalReportId)
			.OnDelete(DeleteBehavior.Cascade);
		b.Entity<Approval>().HasOne((Approval a) => a.DepartureReport).WithMany()
			.HasForeignKey((Approval a) => a.DepartureReportId)
			.OnDelete(DeleteBehavior.Cascade);
		b.Entity<AuditLog>().HasIndex((AuditLog l) => l.At);
		b.Entity<AuditLog>().Property((AuditLog l) => l.Action).HasMaxLength(20);
		b.Entity<AuditLog>().Property((AuditLog l) => l.ReportKind).HasMaxLength(20);
		b.Entity<CoordinationSheet>().Property((CoordinationSheet c) => c.Status).HasConversion<string>()
			.HasMaxLength(20);
		b.Entity<CoordinationSheet>().HasIndex((CoordinationSheet c) => new { c.StationId, c.Status });
		b.Entity<CoordinationSheet>().HasIndex((CoordinationSheet c) => c.FlightDate);
		b.Entity<CoordinationSheet>().HasIndex((CoordinationSheet c) => new { c.Status, c.FlightDate });
		b.Entity<CoordinationSheet>().HasOne((CoordinationSheet c) => c.Station).WithMany()
			.HasForeignKey((CoordinationSheet c) => c.StationId)
			.OnDelete(DeleteBehavior.Restrict);
		b.Entity<CoordinationSheet>().HasMany((CoordinationSheet c) => c.Activities).WithOne((CoordinationActivity x) => x.CoordinationSheet)
			.HasForeignKey((CoordinationActivity x) => x.CoordinationSheetId)
			.OnDelete(DeleteBehavior.Cascade);
		b.Entity<CoordinationSheet>().HasMany((CoordinationSheet c) => c.Buses).WithOne((CoordinationBus x) => x.CoordinationSheet)
			.HasForeignKey((CoordinationBus x) => x.CoordinationSheetId)
			.OnDelete(DeleteBehavior.Cascade);
		b.Entity<CoordinationActivity>().Property((CoordinationActivity a) => a.ActivityKey).HasMaxLength(40);
		b.Entity<CoordinationBus>().Property((CoordinationBus x) => x.Phase).HasMaxLength(10);
	}
}
