using System.Collections.Generic;

namespace SGSForms.Api.Dtos;

public class RoleInput
{
	public string? Key { get; set; }

	public string NameAr { get; set; } = "";

	public string NameEn { get; set; } = "";

	public string? Color { get; set; }

	public bool IsActive { get; set; } = true;

	public List<string> Permissions { get; set; } = new List<string>();
}
