using System.Collections.Generic;
using System.Threading.Tasks;
using SGSForms.Api.Dtos;

namespace SGSForms.Api.Services;

public interface IRoleService
{
	Task<List<RoleDto>> ListAsync();

	List<PermissionDto> Catalog();

	Task<RoleDto> SaveAsync(int? id, RoleInput dto);

	Task DeleteAsync(int id);

	Task<List<string>> PermissionsForUserAsync(int userId);

	Task<List<string>> PermissionsForRoleAsync(int roleId);

	Task EnsureAdminReachableAsync();
}
