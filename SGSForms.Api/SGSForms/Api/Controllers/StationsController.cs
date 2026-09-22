using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGSForms.Api.Auth;
using SGSForms.Api.Dtos;
using SGSForms.Api.Services;

namespace SGSForms.Api.Controllers;

[ApiController]
[Route("stations")]
[Authorize]
public class StationsController : ControllerBase
{
	private readonly IAdminService _svc;

	private readonly CurrentUser _me;

	public StationsController(IAdminService svc, CurrentUser me)
	{
		_svc = svc;
		_me = me;
	}

	[HttpGet]
	public Task<List<StationDto>> List([FromQuery] bool includeInactive = false)
	{
		bool all = includeInactive && _me.Has(Permissions.AdminStations);
		return _svc.StationsAsync(all);
	}

	[HttpPost]
	[Authorize(Policy = "perm:admin.stations")]
	public Task<StationDto> Create([FromBody] StationInput dto)
	{
		return _svc.SaveStationAsync(null, dto);
	}

	[HttpPut("{id:int}")]
	[Authorize(Policy = "perm:admin.stations")]
	public Task<StationDto> Update(int id, [FromBody] StationInput dto)
	{
		return _svc.SaveStationAsync(id, dto);
	}

	[HttpDelete("{id:int}")]
	[Authorize(Policy = "perm:admin.stations")]
	public async Task<IActionResult> Delete(int id)
	{
		await _svc.DeleteStationAsync(id);
		return Ok(new
		{
			ok = true
		});
	}
}
