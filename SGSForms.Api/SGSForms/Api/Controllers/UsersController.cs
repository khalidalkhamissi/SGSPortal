using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGSForms.Api.Dtos;
using SGSForms.Api.Services;

namespace SGSForms.Api.Controllers;

[ApiController]
[Route("users")]
[Authorize(Policy = "perm:admin.users")]
public class UsersController : ControllerBase
{
	private readonly IAdminService _svc;

	public UsersController(IAdminService svc)
	{
		_svc = svc;
	}

	[HttpGet]
	public Task<List<UserDto>> List()
	{
		return _svc.UsersAsync();
	}

	[HttpPost]
	public Task<UserDto> Create([FromBody] UserInput dto)
	{
		return _svc.SaveUserAsync(null, dto);
	}

	[HttpPut("{id:int}")]
	public Task<UserDto> Update(int id, [FromBody] UserInput dto)
	{
		return _svc.SaveUserAsync(id, dto);
	}

	[HttpDelete("{id:int}")]
	public async Task<IActionResult> Delete(int id)
	{
		await _svc.DeleteUserAsync(id);
		return Ok(new
		{
			ok = true
		});
	}
}
