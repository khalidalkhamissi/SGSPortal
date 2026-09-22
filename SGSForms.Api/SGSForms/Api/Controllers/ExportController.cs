using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGSForms.Api.Services;

namespace SGSForms.Api.Controllers;

[ApiController]
[Route("export")]
[Authorize]
public class ExportController : ControllerBase
{
	private readonly IExportService _svc;

	public ExportController(IExportService svc)
	{
		_svc = svc;
	}

	[HttpGet("report/{kind}/{id:int}/pdf")]
	[Authorize(Policy = "perm:export.pdf")]
	public async Task<IActionResult> ReportPdf(string kind, int id)
	{
		var (fileContents, fileDownloadName) = await _svc.ReportPdfAsync(kind, id);
		return File(fileContents, "application/pdf", fileDownloadName);
	}

	[HttpGet("coordination/{id:int}/pdf")]
	[Authorize(Policy = "perm:export.pdf")]
	public async Task<IActionResult> CoordinationPdf(int id)
	{
		var (fileContents, fileDownloadName) = await _svc.CoordinationPdfAsync(id);
		return File(fileContents, "application/pdf", fileDownloadName);
	}

	/// <summary>
	/// Excel of the reports chosen in the export dialog. status: comma separated list or "all";
	/// kind: all | arrival | departure; date is a one-day shortcut for from/to.
	/// </summary>
	[HttpGet("reports.xlsx")]
	[Authorize(Policy = "perm:export.excel")]
	public async Task<IActionResult> ReportsExcel([FromQuery] string status = "submitted", [FromQuery] string? kind = null, [FromQuery] string? from = null, [FromQuery] string? to = null, [FromQuery] int? stationId = null, [FromQuery] string? airline = null, [FromQuery] string? date = null, [FromQuery] string? q = null)
	{
		var (fileContents, fileDownloadName) = await _svc.ListExcelAsync(status, kind, from ?? date, to ?? date, stationId, airline, q);
		return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileDownloadName);
	}

	/// <summary>How many reports the same selection would export (shown live in the export dialog).</summary>
	[HttpGet("reports/count")]
	[Authorize(Policy = "perm:export.excel")]
	public async Task<IActionResult> ReportsCount([FromQuery] string status = "submitted", [FromQuery] string? kind = null, [FromQuery] string? from = null, [FromQuery] string? to = null, [FromQuery] int? stationId = null, [FromQuery] string? airline = null, [FromQuery] string? q = null)
	{
		var (arrivals, departures) = await _svc.ExcelCountAsync(status, kind, from, to, stationId, airline, q);
		return Ok(new { arrivals, departures, total = arrivals + departures });
	}

	/// <summary>Excel of coordination sheets. status: draft, submitted (comma separated) or "all"; handling: all | turnaround | transit | terminating | originating.</summary>
	[HttpGet("coordination.xlsx")]
	[Authorize(Policy = "perm:export.excel")]
	public async Task<IActionResult> CoordinationExcel([FromQuery] string? status = "all", [FromQuery] string? handling = null, [FromQuery] string? from = null, [FromQuery] string? to = null, [FromQuery] int? stationId = null, [FromQuery] string? airline = null, [FromQuery] string? q = null)
	{
		var (fileContents, fileDownloadName) = await _svc.CoordinationExcelAsync(status, handling, from, to, stationId, airline, q);
		return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileDownloadName);
	}

	[HttpGet("coordination/count")]
	[Authorize(Policy = "perm:export.excel")]
	public async Task<IActionResult> CoordinationCount([FromQuery] string? status = "all", [FromQuery] string? handling = null, [FromQuery] string? from = null, [FromQuery] string? to = null, [FromQuery] int? stationId = null, [FromQuery] string? airline = null, [FromQuery] string? q = null)
	{
		return Ok(new { total = await _svc.CoordinationExcelCountAsync(status, handling, from, to, stationId, airline, q) });
	}
}
