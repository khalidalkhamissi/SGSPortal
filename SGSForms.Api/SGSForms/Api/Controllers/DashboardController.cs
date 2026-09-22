using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGSForms.Api.Dtos;
using SGSForms.Api.Services;

namespace SGSForms.Api.Controllers;

[ApiController]
[Route("dashboard")]
[Authorize(Policy = "perm:dashboard.view")]
public class DashboardController : ControllerBase
{
	private readonly IDashboardService _svc;

	public DashboardController(IDashboardService svc)
	{
		_svc = svc;
	}

	[HttpGet("summary")]
	public Task<DashboardSummary> Summary([FromQuery] string? from, [FromQuery] string? to, [FromQuery] int? stationId, [FromQuery] string? airline)
	{
		return _svc.SummaryAsync(from, to, stationId, airline);
	}
}
