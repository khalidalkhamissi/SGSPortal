using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGSForms.Api.Dtos;
using SGSForms.Api.Services;

namespace SGSForms.Api.Controllers;

[ApiController]
[Route("arrival")]
[Authorize]
public class ArrivalController : ControllerBase
{
	private readonly IArrivalService _svc;

	public ArrivalController(IArrivalService svc)
	{
		_svc = svc;
	}

	[HttpPost]
	[Authorize(Policy = "perm:reports.create")]
	public Task<object> Create([FromBody] ArrivalInput dto)
	{
		return _svc.SaveAsync(null, dto);
	}

	[HttpPut("{id:int}")]
	[Authorize(Policy = "perm:reports.create")]
	public Task<object> Update(int id, [FromBody] ArrivalInput dto)
	{
		return _svc.SaveAsync(id, dto);
	}

	[HttpGet("{id:int}")]
	public Task<object> Get(int id)
	{
		return _svc.GetAsync(id);
	}

	[HttpDelete("{id:int}")]
	[Authorize(Policy = "perm:reports.delete")]
	public async Task<IActionResult> Delete(int id)
	{
		await _svc.DeleteAsync(id);
		return Ok(new
		{
			ok = true
		});
	}
}
