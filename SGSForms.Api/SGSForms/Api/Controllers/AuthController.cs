using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SGSForms.Api.Services;

namespace SGSForms.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
	private readonly IAuthService _auth;

	public AuthController(IAuthService auth)
	{
		_auth = auth;
	}

	[AllowAnonymous]
	[EnableRateLimiting("login")]
	[HttpPost("login")]
	public async Task<IActionResult> Login([FromForm] string? username, [FromForm] string? password)
	{
		return Ok(await _auth.LoginAsync(username, password));
	}
}
