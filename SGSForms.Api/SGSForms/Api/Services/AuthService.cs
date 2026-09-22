using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SGSForms.Api.Auth;
using SGSForms.Api.Data;
using SGSForms.Api.Dtos;
using SGSForms.Api.Entities;

namespace SGSForms.Api.Services;

public class AuthService : IAuthService
{
	/// <summary>Verified when the account does not exist so both paths take the same time (no user enumeration by timing).</summary>
	private static readonly string DummyHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());

	private readonly AppDbContext _db;

	private readonly JwtTokenService _jwt;

	private readonly IRoleService _roles;

	private readonly ILogger<AuthService> _log;

	public AuthService(AppDbContext db, JwtTokenService jwt, IRoleService roles, ILogger<AuthService> log)
	{
		_db = db;
		_jwt = jwt;
		_roles = roles;
		_log = log;
	}

	public async Task<TokenResponse> LoginAsync(string? email, string? password)
	{
		email = (email ?? "").Trim();
		if (email.Length == 0 || email.Length > 255 || string.IsNullOrEmpty(password) || password.Length > 200)
		{
			throw new AppException("البريد أو كلمة المرور غير صحيحة", 401);
		}
		User? user = await _db.Users.Include(u => u.Station).Include(u => u.Role)
			.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
		bool valid = BCrypt.Net.BCrypt.Verify(password, user?.PasswordHash ?? DummyHash);
		if (user == null || !valid)
		{
			_log.LogWarning("Failed sign-in for {Email}", email);
			throw new AppException("البريد أو كلمة المرور غير صحيحة", 401);
		}
		if (!user.Role.IsActive)
		{
			throw new AppException("دور المستخدم معطّل — راجع مدير النظام", 403);
		}
		user.LastLogin = DateTime.UtcNow;
		await _db.SaveChangesAsync();
		List<string> perms = await _roles.PermissionsForUserAsync(user.Id);
		return new TokenResponse
		{
			AccessToken = _jwt.Create(user, perms),
			Role = user.Role.Key,
			AirportId = user.StationId,
			Name = user.Name,
			AirportName = user.Station?.NameAr,
			Permissions = perms,
			RoleNameAr = user.Role.NameAr,
			RoleNameEn = user.Role.NameEn,
			RoleColor = user.Role.Color
		};
	}
}
