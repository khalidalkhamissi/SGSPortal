using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SGSForms.Api.Auth;
using SGSForms.Api.Data;
using SGSForms.Api.Dtos;
using SGSForms.Api.Entities;

namespace SGSForms.Api.Services;

public partial class AdminService : IAdminService
{
	public const int MinPasswordLength = 8;

	[GeneratedRegex("^[A-Z]{3}$")]
	private static partial Regex IataRegex();

	private readonly AppDbContext _db;

	private readonly CurrentUser _me;

	private readonly IRoleService _roles;

	public AdminService(AppDbContext db, CurrentUser me, IRoleService roles)
	{
		_db = db;
		_me = me;
		_roles = roles;
	}

	public async Task<List<StationDto>> StationsAsync(bool includeInactive)
	{
		IQueryable<Station> q = _db.Stations.AsNoTracking();
		if (!includeInactive)
		{
			q = q.Where(s => s.IsActive);
		}
		return await q.OrderBy(s => s.Code).Select(s => new StationDto
		{
			Id = s.Id,
			Code = s.Code,
			NameAr = s.NameAr,
			NameEn = s.NameEn,
			IsActive = s.IsActive
		}).ToListAsync();
	}

	public async Task<StationDto> SaveStationAsync(int? id, StationInput dto)
	{
		string code = (dto.Code ?? "").Trim().ToUpperInvariant();
		if (!IataRegex().IsMatch(code))
		{
			throw new AppException("رمز المطار: ثلاثة أحرف إنجليزية (IATA)");
		}
		if (string.IsNullOrWhiteSpace(dto.NameAr) || string.IsNullOrWhiteSpace(dto.NameEn))
		{
			throw new AppException("اسم المطار بالعربية والإنجليزية مطلوب");
		}
		string nameAr = Parse.Required(dto.NameAr, 150, "اسم المطار بالعربية");
		string nameEn = Parse.Required(dto.NameEn, 150, "اسم المطار بالإنجليزية");
		if (await _db.Stations.AnyAsync(s => s.Code == code && (!id.HasValue || s.Id != id.Value)))
		{
			throw new AppException("رمز المطار مستخدم مسبقًا");
		}
		Station st;
		if (!id.HasValue)
		{
			st = new Station();
			_db.Stations.Add(st);
		}
		else
		{
			st = await _db.Stations.FindAsync(id.Value) ?? throw AppException.NotFound("المطار غير موجود");
		}
		st.Code = code;
		st.NameAr = nameAr;
		st.NameEn = nameEn;
		st.IsActive = dto.IsActive;
		await _db.SaveChangesAsync();
		return new StationDto
		{
			Id = st.Id,
			Code = st.Code,
			NameAr = st.NameAr,
			NameEn = st.NameEn,
			IsActive = st.IsActive
		};
	}

	public async Task DeleteStationAsync(int id)
	{
		Station st = await _db.Stations.FindAsync(id) ?? throw AppException.NotFound("المطار غير موجود");
		int users = await _db.Users.CountAsync(u => u.StationId == id);
		int reports = await _db.ArrivalReports.CountAsync(r => r.StationId == id)
			+ await _db.DepartureReports.CountAsync(r => r.StationId == id)
			+ await _db.CoordinationSheets.CountAsync(c => c.StationId == id);
		if (users + reports > 0)
		{
			throw new AppException($"المطار مرتبط بـ {users} مستخدمًا و{reports} تقريرًا — عطّله بدل حذفه", 409);
		}
		_db.Stations.Remove(st);
		await _db.SaveChangesAsync();
	}

	public async Task<List<UserDto>> UsersAsync()
	{
		return await _db.Users.AsNoTracking()
			.OrderBy(u => u.Id)
			.Select(u => new UserDto
			{
				Id = u.Id,
				Name = u.Name,
				Email = u.Email,
				Role = u.Role.Key,
				RoleNameAr = u.Role.NameAr,
				RoleNameEn = u.Role.NameEn,
				StationId = u.StationId,
				StationCode = u.Station != null ? u.Station.Code : null,
				IsActive = u.IsActive
			})
			.ToListAsync();
	}

	public async Task<UserDto> SaveUserAsync(int? id, UserInput dto)
	{
		string name = Parse.Required(dto.Name, 100, "الاسم");
		string email = Parse.Required(dto.Email, 255, "البريد");
		if (!IsEmail(email))
		{
			throw new AppException("البريد الإلكتروني غير صالح");
		}
		string? password = string.IsNullOrEmpty(dto.Password) ? null : dto.Password;
		if (password != null && (password.Length < MinPasswordLength || password.Length > 200))
		{
			throw new AppException($"كلمة المرور يجب أن تكون {MinPasswordLength} أحرف على الأقل");
		}
		Role role = await ResolveRoleAsync(dto.Role);
		await EnsureCanGrantAsync(role.Id);
		if (dto.StationId.HasValue && !await _db.Stations.AnyAsync(s => s.Id == dto.StationId.Value))
		{
			throw new AppException("المحطة غير موجودة");
		}
		if (await _db.Users.AnyAsync(x => x.Email == email && (!id.HasValue || x.Id != id.Value)))
		{
			throw new AppException("البريد مستخدم مسبقاً");
		}

		User u;
		if (!id.HasValue)
		{
			if (password == null)
			{
				throw new AppException("كلمة المرور مطلوبة");
			}
			u = new User { CreatedAt = DateTime.UtcNow };
			_db.Users.Add(u);
		}
		else
		{
			u = await _db.Users.FindAsync(id.Value) ?? throw AppException.NotFound("المستخدم غير موجود");
			// Editing someone (e.g. resetting their password) is as powerful as holding their role.
			await EnsureCanGrantAsync(u.RoleId);
			if (id.Value == _me.Id)
			{
				if (u.RoleId != role.Id)
				{
					throw new AppException("لا يمكنك تغيير دور حسابك", 409);
				}
				if (!dto.IsActive)
				{
					throw new AppException("لا يمكنك تعطيل حسابك", 409);
				}
			}
		}
		u.Name = name;
		u.Email = email;
		u.RoleId = role.Id;
		u.StationId = dto.StationId;
		u.IsActive = dto.IsActive;
		u.UpdatedAt = DateTime.UtcNow;
		if (password != null)
		{
			u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
		}
		await using (IDbContextTransaction tx = await _db.Database.BeginTransactionAsync())
		{
			await _db.SaveChangesAsync();
			await _roles.EnsureAdminReachableAsync();
			await tx.CommitAsync();
		}
		string? stationCode = u.StationId.HasValue
			? await _db.Stations.Where(s => s.Id == u.StationId.Value).Select(s => s.Code).FirstOrDefaultAsync()
			: null;
		return new UserDto
		{
			Id = u.Id,
			Name = u.Name,
			Email = u.Email,
			Role = role.Key,
			RoleNameAr = role.NameAr,
			RoleNameEn = role.NameEn,
			StationId = u.StationId,
			StationCode = stationCode,
			IsActive = u.IsActive
		};
	}

	public async Task DeleteUserAsync(int id)
	{
		User user = await _db.Users.FindAsync(id) ?? throw AppException.NotFound("المستخدم غير موجود");
		if (id == _me.Id)
		{
			throw new AppException("لا يمكنك تعطيل حسابك", 409);
		}
		await EnsureCanGrantAsync(user.RoleId);
		user.IsActive = false;
		user.UpdatedAt = DateTime.UtcNow;
		await using IDbContextTransaction tx = await _db.Database.BeginTransactionAsync();
		await _db.SaveChangesAsync();
		await _roles.EnsureAdminReachableAsync();
		await tx.CommitAsync();
	}

	public async Task<PagedLogs> LogsAsync(string? action, string? kind, string? date, string? q, int page, int pageSize)
	{
		(page, pageSize) = Parse.Paging(page, pageSize);
		IQueryable<AuditLog> source = _db.AuditLogs.AsNoTracking();
		if (!string.IsNullOrWhiteSpace(action))
		{
			string a = action.Trim().ToLowerInvariant();
			source = source.Where(l => l.Action == a);
		}
		if (!string.IsNullOrWhiteSpace(kind))
		{
			string k = kind.Trim().ToLowerInvariant();
			source = source.Where(l => l.ReportKind == k);
		}
		if (Parse.OptionalDate(date) is DateOnly day)
		{
			DateTime start = day.ToDateTime(TimeOnly.MinValue);
			DateTime end = start.AddDays(1.0);
			source = source.Where(l => l.At >= start && l.At < end);
		}
		if (Parse.Search(q) is string term)
		{
			source = source.Where(l => l.UserName.Contains(term) || (l.FlightNo != null && l.FlightNo.Contains(term))
				|| (l.Details != null && l.Details.Contains(term)) || (l.StationCode != null && l.StationCode.Contains(term)));
		}
		int total = await source.CountAsync();
		List<AuditLog> rows = await source.OrderByDescending(l => l.At)
			.Skip((page - 1) * pageSize).Take(pageSize)
			.ToListAsync();
		return new PagedLogs
		{
			Items = rows.Select(l => new LogDto
			{
				Id = l.Id,
				At = l.At.ToString("yyyy-MM-dd HH:mm"),
				UserName = l.UserName,
				UserRole = l.UserRole,
				StationCode = l.StationCode,
				Action = l.Action,
				ReportKind = l.ReportKind,
				ReportId = l.ReportId,
				FlightNo = l.FlightNo,
				Details = l.Details
			}).ToList(),
			Total = total,
			Page = page,
			PageSize = pageSize
		};
	}

	private static bool IsEmail(string email)
	{
		return MailAddress.TryCreate(email, out MailAddress? parsed) && parsed.Address == email;
	}

	private async Task<Role> ResolveRoleAsync(string? key)
	{
		string k = (key ?? "").Trim().ToLowerInvariant();
		Role role = await _db.Roles.FirstOrDefaultAsync(r => r.Key == k) ?? throw new AppException("الدور غير موجود: " + key);
		if (!role.IsActive)
		{
			throw new AppException("لا يمكن إسناد دور معطّل");
		}
		return role;
	}

	/// <summary>
	/// A user manager may only hand out (or manage holders of) roles whose permissions they hold themselves,
	/// so admin.users alone cannot be used to create or take over an administrator. Role managers are exempt:
	/// they can already give any role any permission.
	/// </summary>
	private async Task EnsureCanGrantAsync(int roleId)
	{
		if (_me.Has(Permissions.AdminRoles))
		{
			return;
		}
		List<string> rolePerms = await _roles.PermissionsForRoleAsync(roleId);
		if (rolePerms.Any(p => !_me.Has(p)))
		{
			throw new AppException("لا يمكنك إدارة مستخدم بدور يملك صلاحيات أعلى من صلاحياتك", 403);
		}
	}
}
