using System.Threading.Tasks;
using SGSForms.Api.Dtos;

namespace SGSForms.Api.Services;

public interface IDashboardService
{
	Task<DashboardSummary> SummaryAsync(string? from, string? to, int? stationId, string? airline);
}
