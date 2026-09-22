using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SGSForms.Api.Dtos;

public class TokenResponse
{
	[JsonPropertyName("access_token")]
	public string AccessToken { get; set; } = "";


	[JsonPropertyName("role")]
	public string Role { get; set; } = "";

	[JsonPropertyName("airport_id")]
	public int? AirportId { get; set; }

	[JsonPropertyName("name")]
	public string Name { get; set; } = "";

	[JsonPropertyName("airport_name")]
	public string? AirportName { get; set; }

	[JsonPropertyName("permissions")]
	public List<string> Permissions { get; set; } = new List<string>();

	[JsonPropertyName("role_name_ar")]
	public string RoleNameAr { get; set; } = "";

	[JsonPropertyName("role_name_en")]
	public string RoleNameEn { get; set; } = "";

	[JsonPropertyName("role_color")]
	public string RoleColor { get; set; } = "";
}
