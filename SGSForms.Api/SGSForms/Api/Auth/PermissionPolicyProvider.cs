using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace SGSForms.Api.Auth;

/// <summary>Turns policy names of the form "perm:&lt;permission&gt;" into a permission requirement.</summary>
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
	private readonly DefaultAuthorizationPolicyProvider _fallback;

	public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
	{
		_fallback = new DefaultAuthorizationPolicyProvider(options);
	}

	public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();

	public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();

	public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
	{
		if (policyName.StartsWith(Permissions.PolicyPrefix, StringComparison.Ordinal))
		{
			string permission = policyName.Substring(Permissions.PolicyPrefix.Length);
			AuthorizationPolicy policy = new AuthorizationPolicyBuilder()
				.RequireAuthenticatedUser()
				.AddRequirements(new PermissionRequirement(permission))
				.Build();
			return Task.FromResult<AuthorizationPolicy?>(policy);
		}
		return _fallback.GetPolicyAsync(policyName);
	}
}
