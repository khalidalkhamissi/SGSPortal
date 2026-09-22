using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGSForms.Api.Dtos;
using SGSForms.Api.Services;

namespace SGSForms.Api.Controllers;

[ApiController]
[Route("logs")]
[Authorize(Policy = "perm:logs.view")]
public class LogsController : ControllerBase
{
	private readonly IAdminService _svc;

	public LogsController(IAdminService svc)
	{
		_svc = svc;
	}

	[HttpGet]
	public Task<PagedLogs> List([FromQuery] string? action, [FromQuery] string? kind, [FromQuery] string? date, [FromQuery] string? q = null, [FromQuery] int page = 1, [FromQuery] int pageSize = Parse.MaxPageSize)
	{
		return _svc.LogsAsync(action, kind, date, q, page, pageSize);
	}
}
