using System;
using System.Collections.Generic;

namespace SGSForms.Api.Entities;

public class Role
{
	public int Id { get; set; }

	public string Key { get; set; } = "";

	public string NameAr { get; set; } = "";

	public string NameEn { get; set; } = "";

	public string Color { get; set; } = "#006C4E";

	public bool IsSystem { get; set; }

	public bool IsLocked { get; set; }

	public bool IsActive { get; set; } = true;

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	public ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();

	public ICollection<User> Users { get; set; } = new List<User>();
}
