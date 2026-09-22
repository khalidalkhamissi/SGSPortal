using System.Threading.Tasks;
using SGSForms.Api.Dtos;

namespace SGSForms.Api.Services;

public interface IApprovalService
{
	Task<object> ApproveAsync(ApprovalInput dto);

	Task ReturnAsync(string kind, int reportId, string? reason);

	Task<object?> GetForReportAsync(string kind, int reportId);
}
