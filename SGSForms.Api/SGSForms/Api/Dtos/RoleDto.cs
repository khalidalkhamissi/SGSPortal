using System.Collections.Generic;

namespace SGSForms.Api.Dtos;

public class RoleDto
{
	public int Id { get; set; }

	public string Key { get; set; } = "";

	public string NameAr { get; set; } = "";

	public string NameEn { get; set; } = "";

	public string Color { get; set; } = "";

	public bool IsSystem { get; set; }

	public bool IsLocked { get; set; }

	public bool IsActive { get; set; }

	public int UserCount { get; set; }

	public List<string> Permissions { get; set; } = new List<string>();
}
