using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SGSForms.Api.Auth;
using SGSForms.Api.Entities;

namespace SGSForms.Api.Data;

public static class DbSeeder
{
	public static readonly Dictionary<string, string[]> DefaultPermissions = new Dictionary<string, string[]>
	{
		["admin"] = Permissions.Catalog.Select((PermissionDef p) => p.Key).ToArray(),
		["management"] = new string[7] { "reports.pending", "reports.returned", "reports.approved", "export.pdf", "export.excel", "dashboard.view", "stations.viewAll" },
		["data_entry"] = new string[13]
		{
			"reports.view", "reports.create", "reports.delete", "reports.approve", "reports.pending", "reports.drafts", "reports.returned", "reports.approved", "coordination.view", "coordination.create",
			"coordination.delete", "export.pdf", "export.excel"
		}
	};

	public static readonly (string Key, string NameAr, string NameEn, string Color, bool Locked)[] SystemRoles = new(string, string, string, string, bool)[3]
	{
		("admin", "مدير النظام", "Admin", "#7C3AED", true),
		("management", "الإدارة", "Management", "#C9A84C", false),
		("data_entry", "مشرف مناولة", "Handling Supervisor", "#006C4E", false)
	};

	/// <summary>
	/// Creates the system roles and default stations on an empty database. Demo accounts (with well-known
	/// passwords) are created only when <paramref name="demoUsers"/> is true (config Seed:DemoUsers) — never
	/// enable it in production; create the first administrator with SQL Deploy/02_seed_production.sql instead.
	/// </summary>
	public static async Task SeedAsync(AppDbContext db, bool demoUsers, ILogger log)
	{
		if (!(await db.Roles.AnyAsync()))
		{
			(string, string, string, string, bool)[] systemRoles = SystemRoles;
			for (int i = 0; i < systemRoles.Length; i++)
			{
				var (key, nameAr, nameEn, color, locked) = systemRoles[i];
				db.Roles.Add(new Role
				{
					Key = key,
					NameAr = nameAr,
					NameEn = nameEn,
					Color = color,
					IsSystem = true,
					IsLocked = locked,
					IsActive = true,
					Permissions = DefaultPermissions[key].Select((string p) => new RolePermission
					{
						Permission = p
					}).ToList()
				});
			}
			await db.SaveChangesAsync();
		}
		if (!(await db.Stations.AnyAsync()))
		{
			db.Stations.AddRange(new Station
			{
				Code = "RUH",
				NameAr = "مطار الملك خالد الدولي",
				NameEn = "King Khalid Intl"
			}, new Station
			{
				Code = "JED",
				NameAr = "مطار الملك عبدالعزيز الدولي",
				NameEn = "King Abdulaziz Intl"
			}, new Station
			{
				Code = "DMM",
				NameAr = "مطار الملك فهد الدولي",
				NameEn = "King Fahd Intl"
			});
			await db.SaveChangesAsync();
		}
		if (!(await db.Users.AnyAsync()))
		{
			if (!demoUsers)
			{
				log.LogWarning("No users exist. Create an administrator with SQL Deploy/02_seed_production.sql, or set Seed__DemoUsers=true on a test machine.");
				return;
			}
			log.LogWarning("Seeding DEMO accounts with default passwords (Seed:DemoUsers=true). Do not use on production.");
			Station ruh = await db.Stations.FirstAsync((Station s) => s.Code == "RUH");
			Station jed = await db.Stations.FirstAsync((Station s) => s.Code == "JED");
			db.Users.AddRange(new User
			{
				Name = "مدير النظام",
				Email = "admin@sgs.sa",
				PasswordHash = Hash("admin123"),
				RoleId = RoleId("admin")
			}, new User
			{
				Name = "الإدارة",
				Email = "management@sgs.sa",
				PasswordHash = Hash("manage123"),
				RoleId = RoleId("management")
			}, new User
			{
				Name = "مشرف مناولة RUH",
				Email = "ruh@sgs.sa",
				PasswordHash = Hash("entry123"),
				RoleId = RoleId("data_entry"),
				StationId = ruh.Id
			}, new User
			{
				Name = "مشرف مناولة JED",
				Email = "jed@sgs.sa",
				PasswordHash = Hash("entry123"),
				RoleId = RoleId("data_entry"),
				StationId = jed.Id
			});
			await db.SaveChangesAsync();
		}
		static string Hash(string p)
		{
			return BCrypt.Net.BCrypt.HashPassword(p);
		}
		int RoleId(string text)
		{
			return db.Roles.First((Role r) => r.Key == text).Id;
		}
	}
}
