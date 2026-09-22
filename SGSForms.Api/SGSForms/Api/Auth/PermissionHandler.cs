using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace SGSForms.Api.Auth;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
	protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
	{
		if (context.User.HasPermission(requirement.Permission))
		{
			context.Succeed(requirement);
		}
		return Task.CompletedTask;
	}
}
