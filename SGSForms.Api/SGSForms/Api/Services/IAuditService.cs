using System.Threading.Tasks;

namespace SGSForms.Api.Services;

public interface IAuditService
{
	Task LogAsync(string action, string kind, int? reportId = null, string? flightNo = null, string? details = null, int? stationId = null);
}
