using System.Threading.Tasks;
using SGSForms.Api.Dtos;

namespace SGSForms.Api.Services;

public interface IReportService
{
	Task<PagedReports> ListAsync(string? status, string? from, string? to, int? stationId, string? airline, string? q, int page, int pageSize);
}
