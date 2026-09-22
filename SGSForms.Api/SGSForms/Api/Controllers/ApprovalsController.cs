using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGSForms.Api.Dtos;
using SGSForms.Api.Services;

namespace SGSForms.Api.Controllers;

[ApiController]
[Route("approvals")]
[Authorize]
public class ApprovalsController : ControllerBase
{
	private readonly IApprovalService _svc;

	public ApprovalsController(IApprovalService svc)
	{
		_svc = svc;
	}

	[HttpPost]
	[Authorize(Policy = "perm:reports.approve")]
	public Task<object> Approve([FromBody] ApprovalInput dto)
	{
		return _svc.ApproveAsync(dto);
	}

	[HttpGet("{kind}/{id:int}")]
	public Task<object?> Get(string kind, int id)
	{
		return _svc.GetForReportAsync(kind, id);
	}
}
