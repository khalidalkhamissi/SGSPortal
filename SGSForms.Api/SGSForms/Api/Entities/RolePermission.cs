namespace SGSForms.Api.Entities;

public class RolePermission
{
	public int Id { get; set; }

	public int RoleId { get; set; }

	public Role Role { get; set; } = null!;

	public string Permission { get; set; } = "";
}
