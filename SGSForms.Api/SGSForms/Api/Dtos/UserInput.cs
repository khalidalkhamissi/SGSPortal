namespace SGSForms.Api.Dtos;

public class UserInput
{
	public string Name { get; set; } = "";

	public string Email { get; set; } = "";

	public string? Password { get; set; }

	public string Role { get; set; } = "data_entry";

	public int? StationId { get; set; }

	public bool IsActive { get; set; } = true;
}
