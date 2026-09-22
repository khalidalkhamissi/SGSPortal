using System.Collections.Generic;
using System.Threading.Tasks;
using SGSForms.Api.Dtos;

namespace SGSForms.Api.Services;

public interface IAdminService
{
	Task<List<StationDto>> StationsAsync(bool includeInactive);

	Task<StationDto> SaveStationAsync(int? id, StationInput dto);

	Task DeleteStationAsync(int id);

	Task<List<UserDto>> UsersAsync();

	Task<UserDto> SaveUserAsync(int? id, UserInput dto);

	Task DeleteUserAsync(int id);

	Task<PagedLogs> LogsAsync(string? action, string? kind, string? date, string? q, int page, int pageSize);
}
