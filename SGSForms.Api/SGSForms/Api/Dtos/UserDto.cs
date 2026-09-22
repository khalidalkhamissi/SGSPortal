namespace SGSForms.Api.Dtos;

public class UserDto
{
	public int Id { get; set; }

	public string Name { get; set; } = "";

	public string Email { get; set; } = "";

	public string Role { get; set; } = "";

	public string RoleNameAr { get; set; } = "";

	public string RoleNameEn { get; set; } = "";

	public int? StationId { get; set; }

	public string? StationCode { get; set; }

	public bool IsActive { get; set; }
}
