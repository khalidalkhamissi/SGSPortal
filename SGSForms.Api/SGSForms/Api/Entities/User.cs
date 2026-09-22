using System;

namespace SGSForms.Api.Entities;

public class User
{
	public int Id { get; set; }

	public string Name { get; set; } = "";

	public string Email { get; set; } = "";

	public string PasswordHash { get; set; } = "";

	public int RoleId { get; set; }

	public Role Role { get; set; } = null!;

	public int? StationId { get; set; }

	public Station? Station { get; set; }

	public bool IsActive { get; set; } = true;

	public DateTime? LastLogin { get; set; }

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
