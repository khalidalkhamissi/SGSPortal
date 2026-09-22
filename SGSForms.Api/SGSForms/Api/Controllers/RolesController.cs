using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGSForms.Api.Auth;
using SGSForms.Api.Dtos;
using SGSForms.Api.Services;

namespace SGSForms.Api.Controllers;

[ApiController]
[Route("roles")]
[Authorize]
public class RolesController : ControllerBase
{
	private readonly IRoleService _svc;

	public RolesController(IRoleService svc)
	{
		_svc = svc;
	}

	[HttpGet]
	public async Task<IActionResult> List()
	{
		if (!User.HasPermission(Permissions.AdminRoles) && !User.HasPermission(Permissions.AdminUsers))
		{
			return Forbid();
		}
		return Ok(await _svc.ListAsync());
	}

	[HttpGet("permissions")]
	[Authorize(Policy = "perm:admin.roles")]
	public ActionResult<List<PermissionDto>> Catalog()
	{
		return _svc.Catalog();
	}

	[HttpPost]
	[Authorize(Policy = "perm:admin.roles")]
	public Task<RoleDto> Create([FromBody] RoleInput dto)
	{
		return _svc.SaveAsync(null, dto);
	}

	[HttpPut("{id:int}")]
	[Authorize(Policy = "perm:admin.roles")]
	public Task<RoleDto> Update(int id, [FromBody] RoleInput dto)
	{
		return _svc.SaveAsync(id, dto);
	}

	[HttpDelete("{id:int}")]
	[Authorize(Policy = "perm:admin.roles")]
	public async Task<IActionResult> Delete(int id)
	{
		await _svc.DeleteAsync(id);
		return Ok(new
		{
			ok = true
		});
	}
}
