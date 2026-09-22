using System.Threading.Tasks;
using SGSForms.Api.Dtos;

namespace SGSForms.Api.Services;

public interface ICoordinationService
{
	Task<PagedReports> ListAsync(string? status, string? from, string? to, int? stationId, string? airline, string? q, int page, int pageSize);

	Task<object> SaveAsync(int? id, CoordinationInput dto);

	Task<object> GetAsync(int id);

	Task DeleteAsync(int id);
}
