using System.Threading.Tasks;

namespace SGSForms.Api.Services;

public interface IExportService
{
	Task<(byte[] bytes, string fileName)> ReportPdfAsync(string kind, int id);

	Task<(byte[] bytes, string fileName)> CoordinationPdfAsync(int id);

	Task<(byte[] bytes, string fileName)> ListExcelAsync(string? status, string? kind, string? from, string? to, int? stationId, string? airline, string? q);

	Task<(byte[] bytes, string fileName)> CoordinationExcelAsync(string? status, string? handling, string? from, string? to, int? stationId, string? airline, string? q);

	Task<int> CoordinationExcelCountAsync(string? status, string? handling, string? from, string? to, int? stationId, string? airline, string? q);

	Task<(int Arrivals, int Departures)> ExcelCountAsync(string? status, string? kind, string? from, string? to, int? stationId, string? airline, string? q);
}
