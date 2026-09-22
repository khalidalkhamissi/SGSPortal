using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGSForms.Api.Dtos;
using SGSForms.Api.Services;

namespace SGSForms.Api.Controllers;

[ApiController]
[Route("reports")]
[Authorize]
public class ReportsController : ControllerBase
{
	private readonly IReportService _reports;

	private readonly IApprovalService _approval;

	public ReportsController(IReportService reports, IApprovalService approval)
	{
		_reports = reports;
		_approval = approval;
	}

	[HttpGet]
	public Task<PagedReports> List([FromQuery] string? status = "submitted", [FromQuery] string? from = null, [FromQuery] string? to = null, [FromQuery] int? stationId = null, [FromQuery] string? airline = null, [FromQuery] string? q = null, [FromQuery] int page = 1, [FromQuery] int pageSize = Parse.MaxPageSize)
	{
		return _reports.ListAsync(status, from, to, stationId, airline, q, page, pageSize);
	}

	[HttpPost("{kind}/{id:int}/return")]
	[Authorize(Policy = "perm:reports.approve")]
	public async Task<IActionResult> Return(string kind, int id, [FromBody] ReturnInput dto)
	{
		await _approval.ReturnAsync(kind, id, dto.Reason);
		return Ok(new
		{
			ok = true
		});
	}
}
