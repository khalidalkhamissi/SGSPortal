using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGSForms.Api.Dtos;
using SGSForms.Api.Services;

namespace SGSForms.Api.Controllers;

[ApiController]
[Route("coordination")]
[Authorize]
public class CoordinationController : ControllerBase
{
	private readonly ICoordinationService _svc;

	public CoordinationController(ICoordinationService svc)
	{
		_svc = svc;
	}

	[HttpGet]
	public Task<PagedReports> List([FromQuery] string? status = null, [FromQuery] string? from = null, [FromQuery] string? to = null, [FromQuery] int? stationId = null, [FromQuery] string? airline = null, [FromQuery] string? q = null, [FromQuery] int page = 1, [FromQuery] int pageSize = Parse.MaxPageSize)
	{
		return _svc.ListAsync(status, from, to, stationId, airline, q, page, pageSize);
	}

	[HttpPost]
	[Authorize(Policy = "perm:coordination.create")]
	public Task<object> Create([FromBody] CoordinationInput dto)
	{
		return _svc.SaveAsync(null, dto);
	}

	[HttpPut("{id:int}")]
	[Authorize(Policy = "perm:coordination.create")]
	public Task<object> Update(int id, [FromBody] CoordinationInput dto)
	{
		return _svc.SaveAsync(id, dto);
	}

	[HttpGet("{id:int}")]
	public Task<object> Get(int id)
	{
		return _svc.GetAsync(id);
	}

	[HttpDelete("{id:int}")]
	[Authorize(Policy = "perm:coordination.delete")]
	public async Task<IActionResult> Delete(int id)
	{
		await _svc.DeleteAsync(id);
		return Ok(new
		{
			ok = true
		});
	}
}
