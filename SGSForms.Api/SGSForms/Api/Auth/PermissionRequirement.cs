using Microsoft.AspNetCore.Authorization;

namespace SGSForms.Api.Auth;

public class PermissionRequirement : IAuthorizationRequirement
{
	public string Permission { get; }

	public PermissionRequirement(string permission)
	{
		Permission = permission;
	}
}
