using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SGSForms.Api.Auth;
using SGSForms.Api.Data;
using SGSForms.Api.Dtos;
using SGSForms.Api.Entities;

namespace SGSForms.Api.Services;

public partial class RoleService : IRoleService
{
	[GeneratedRegex("^[a-z][a-z0-9_]{1,39}$")]
	private static partial Regex KeyRegex();

	[GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
	private static partial Regex ColorRegex();

	private readonly AppDbContext _db;

	public RoleService(AppDbContext db)
	{
		_db = db;
	}

	public async Task<List<RoleDto>> ListAsync()
	{
		List<Role> roles = await _db.Roles.AsNoTracking().Include(r => r.Permissions)
			.OrderByDescending(r => r.IsSystem).ThenBy(r => r.Id)
			.ToListAsync();
		Dictionary<int, int> counts = await _db.Users.AsNoTracking()
			.GroupBy(u => u.RoleId)
			.Select(g => new { RoleId = g.Key, N = g.Count() })
			.ToDictionaryAsync(x => x.RoleId, x => x.N);
		return roles.Select(r => ToDto(r, counts.TryGetValue(r.Id, out int n) ? n : 0)).ToList();
	}

	public List<PermissionDto> Catalog()
	{
		return Permissions.Catalog.Select(p =>
		{
			(string Ar, string En) group = Permissions.Groups.TryGetValue(p.Group, out var g) ? g : (p.Group, p.Group);
			return new PermissionDto
			{
				Key = p.Key,
				Group = p.Group,
				GroupAr = group.Ar,
				GroupEn = group.En,
				NameAr = p.NameAr,
				NameEn = p.NameEn
			};
		}).ToList();
	}

	public async Task<RoleDto> SaveAsync(int? id, RoleInput dto)
	{
		string nameAr = Parse.Required(dto.NameAr, 80, "اسم الدور بالعربية");
		string nameEn = Parse.Required(dto.NameEn, 80, "اسم الدور بالإنجليزية");
		string? color = Parse.Text(dto.Color, 20, "اللون");
		if (color != null && !ColorRegex().IsMatch(color))
		{
			throw new AppException("اللون يجب أن يكون بصيغة ‎#RRGGBB");
		}
		List<string> perms = (dto.Permissions ?? new List<string>())
			.Select(p => (p ?? "").Trim())
			.Where(p => p.Length > 0)
			.Distinct()
			.ToList();
		string? unknown = perms.FirstOrDefault(p => !Permissions.IsKnown(p));
		if (unknown != null)
		{
			throw new AppException("صلاحية غير معروفة: " + unknown);
		}
		Role role;
		if (!id.HasValue)
		{
			string key = (dto.Key ?? "").Trim().ToLowerInvariant();
			if (key.Length == 0)
			{
				throw new AppException("مفتاح الدور مطلوب");
			}
			if (!KeyRegex().IsMatch(key))
			{
				throw new AppException("مفتاح الدور: حروف إنجليزية صغيرة وأرقام وشرطة سفلية فقط");
			}
			if (await _db.Roles.AnyAsync(r => r.Key == key))
			{
				throw new AppException("مفتاح الدور مستخدم مسبقًا");
			}
			role = new Role
			{
				Key = key,
				IsSystem = false,
				IsLocked = false,
				CreatedAt = DateTime.UtcNow
			};
			_db.Roles.Add(role);
		}
		else
		{
			role = await _db.Roles.Include(r => r.Permissions).FirstOrDefaultAsync(r => r.Id == id.Value)
				?? throw AppException.NotFound("الدور غير موجود");
		}
		role.NameAr = nameAr;
		role.NameEn = nameEn;
		if (color != null)
		{
			role.Color = color;
		}
		if (role.IsLocked)
		{
			// The locked admin role keeps its permissions and stays active so the system can never lock everyone out.
			if (!dto.IsActive)
			{
				throw new AppException("لا يمكن تعطيل دور مدير النظام");
			}
		}
		else
		{
			role.IsActive = dto.IsActive;
			List<RolePermission> current = role.Permissions.ToList();
			foreach (RolePermission rp in current.Where(rp => !perms.Contains(rp.Permission)))
			{
				_db.RolePermissions.Remove(rp);
			}
			foreach (string p in perms.Where(p => !current.Any(rp => rp.Permission == p)))
			{
				role.Permissions.Add(new RolePermission { Permission = p });
			}
		}
		await using (IDbContextTransaction tx = await _db.Database.BeginTransactionAsync())
		{
			await _db.SaveChangesAsync();
			await EnsureAdminReachableAsync();
			await tx.CommitAsync();
		}
		Role saved = await _db.Roles.AsNoTracking().Include(r => r.Permissions).FirstAsync(r => r.Id == role.Id);
		return ToDto(saved, await _db.Users.CountAsync(u => u.RoleId == role.Id));
	}

	public async Task DeleteAsync(int id)
	{
		Role role = await _db.Roles.FindAsync(id) ?? throw AppException.NotFound("الدور غير موجود");
		if (role.IsSystem)
		{
			throw new AppException("لا يمكن حذف دور أساسي", 409);
		}
		int users = await _db.Users.CountAsync(u => u.RoleId == id);
		if (users > 0)
		{
			throw new AppException($"الدور مُسنَد إلى {users} مستخدمًا — انقلهم إلى دور آخر أولًا", 409);
		}
		await using IDbContextTransaction tx = await _db.Database.BeginTransactionAsync();
		_db.Roles.Remove(role);
		await _db.SaveChangesAsync();
		await EnsureAdminReachableAsync();
		await tx.CommitAsync();
	}

	public async Task<List<string>> PermissionsForUserAsync(int userId)
	{
		return await (from u in _db.Users.AsNoTracking()
			join rp in _db.RolePermissions.AsNoTracking() on u.RoleId equals rp.RoleId
			where u.Id == userId && u.Role.IsActive
			select rp.Permission).Distinct().ToListAsync();
	}

	public async Task<List<string>> PermissionsForRoleAsync(int roleId)
	{
		return await _db.RolePermissions.AsNoTracking()
			.Where(rp => rp.RoleId == roleId)
			.Select(rp => rp.Permission)
			.ToListAsync();
	}

	/// <summary>Throws (inside the caller's transaction) if no active user could manage roles any more.</summary>
	public async Task EnsureAdminReachableAsync()
	{
		bool reachable = await (from u in _db.Users.AsNoTracking()
			join rp in _db.RolePermissions.AsNoTracking() on u.RoleId equals rp.RoleId
			where u.IsActive && u.Role.IsActive && rp.Permission == Permissions.AdminRoles
			select u.Id).AnyAsync();
		if (!reachable)
		{
			throw new AppException("يجب أن يبقى مستخدم نشط واحد على الأقل يملك صلاحية إدارة الأدوار", 409);
		}
	}

	private static RoleDto ToDto(Role r, int userCount)
	{
		return new RoleDto
		{
			Id = r.Id,
			Key = r.Key,
			NameAr = r.NameAr,
			NameEn = r.NameEn,
			Color = r.Color,
			IsSystem = r.IsSystem,
			IsLocked = r.IsLocked,
			IsActive = r.IsActive,
			UserCount = userCount,
			Permissions = r.Permissions.Select(p => p.Permission).OrderBy(p => p).ToList()
		};
	}
}
