using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SGSForms.Api.Auth;

public class CurrentUser
{
	/// <summary>Station id used in queries when the user has no station and cannot see all stations: matches nothing.</summary>
	private const int NoStation = -1;

	public bool IsAuthenticated { get; }

	public int Id { get; }

	public string Name { get; } = "";

	public string Role { get; } = "";

	public int? StationId { get; }

	public IReadOnlySet<string> Perms { get; } = new HashSet<string>();

	/// <summary>Holds stations.viewAll and is not pinned to a station.</summary>
	public bool SeesAllStations => Has(Permissions.StationsViewAll) && !StationId.HasValue;

	public CurrentUser(IHttpContextAccessor accessor)
	{
		ClaimsPrincipal? principal = accessor.HttpContext?.User;
		if (principal?.Identity?.IsAuthenticated != true)
		{
			return;
		}
		IsAuthenticated = true;
		Id = int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : 0;
		Name = principal.FindFirstValue(JwtTokenService.NameClaim) ?? "";
		Role = principal.FindFirstValue(ClaimTypes.Role) ?? "";
		StationId = int.TryParse(principal.FindFirstValue(JwtTokenService.StationClaim), out int sid) ? sid : null;
		Perms = principal.FindAll(JwtTokenService.PermClaim)
			.SelectMany(c => c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
			.ToHashSet(StringComparer.Ordinal);
	}

	public bool Has(string permission) => Perms.Contains(permission);

	public bool HasAny(params string[] permissions) => permissions.Any(Has);

	public bool CanAccessStation(int stationId) => SeesAllStations || StationId == stationId;

	/// <summary>
	/// Station filter for list queries: null means "all stations".
	/// Users who cannot see all stations are always pinned to their own station,
	/// and a user with neither a station nor stations.viewAll sees nothing.
	/// </summary>
	public int? StationScope(int? requested) => SeesAllStations ? requested : (StationId ?? NoStation);
}
