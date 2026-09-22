using System;
using System.Linq;
using System.Security.Claims;

namespace SGSForms.Api.Auth;

public static class ClaimsPrincipalExtensions
{
	public static bool HasPermission(this ClaimsPrincipal user, string permission)
	{
		return user.FindAll(JwtTokenService.PermClaim)
			.SelectMany(c => c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
			.Any(p => string.Equals(p, permission, StringComparison.Ordinal));
	}
}
