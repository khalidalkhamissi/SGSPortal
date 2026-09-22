using System.Threading.Tasks;
using SGSForms.Api.Dtos;

namespace SGSForms.Api.Services;

public interface IAuthService
{
	Task<TokenResponse> LoginAsync(string? email, string? password);
}
