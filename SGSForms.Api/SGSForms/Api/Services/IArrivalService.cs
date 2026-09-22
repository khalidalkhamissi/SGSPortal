using System.Threading.Tasks;
using SGSForms.Api.Dtos;

namespace SGSForms.Api.Services;

public interface IArrivalService
{
	Task<object> SaveAsync(int? id, ArrivalInput dto);

	Task<object> GetAsync(int id);

	Task DeleteAsync(int id);
}
