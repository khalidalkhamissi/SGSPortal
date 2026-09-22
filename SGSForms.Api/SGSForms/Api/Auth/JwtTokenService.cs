using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SGSForms.Api.Entities;

namespace SGSForms.Api.Auth;

public class JwtTokenService
{
	public const string NameClaim = "name";

	public const string StationClaim = "stationId";

	public const string PermClaim = "perm";

	/// <summary>Changes whenever the password changes, so older tokens stop working.</summary>
	public const string StampClaim = "sv";

	/// <summary>HS256 needs a key of at least 256 bits.</summary>
	public const int MinKeyBytes = 32;

	private readonly JwtSettings _s;

	public JwtTokenService(IOptions<JwtSettings> s)
	{
		_s = s.Value;
	}

	public static SymmetricSecurityKey SigningKey(string? key)
	{
		if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < MinKeyBytes)
		{
			throw new InvalidOperationException($"Jwt:Key must be set and at least {MinKeyBytes} bytes long (use the Jwt__Key environment variable).");
		}
		return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
	}

	public static string SecurityStamp(string passwordHash)
	{
		byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(passwordHash));
		return Convert.ToHexString(hash, 0, 8);
	}

	public static List<Claim> ClaimsFor(int userId, string name, string roleKey, int? stationId, IEnumerable<string> permissions)
	{
		string id = userId.ToString();
		return new List<Claim>
		{
			new Claim(JwtRegisteredClaimNames.Sub, id),
			new Claim(ClaimTypes.NameIdentifier, id),
			new Claim(NameClaim, name),
			new Claim(ClaimTypes.Role, roleKey),
			new Claim(StationClaim, stationId?.ToString() ?? ""),
			new Claim(PermClaim, string.Join(' ', permissions))
		};
	}

	public string Create(User user, IEnumerable<string> permissions)
	{
		List<Claim> claims = ClaimsFor(user.Id, user.Name, user.Role.Key, user.StationId, permissions);
		claims.Add(new Claim(StampClaim, SecurityStamp(user.PasswordHash)));
		JwtSecurityToken token = new JwtSecurityToken(
			issuer: _s.Issuer,
			audience: _s.Audience,
			claims: claims,
			expires: DateTime.UtcNow.AddMinutes(_s.ExpireMinutes),
			signingCredentials: new SigningCredentials(SigningKey(_s.Key), SecurityAlgorithms.HmacSha256));
		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}
