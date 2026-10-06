using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace MyWebApp.Infrastructure.Authorization;

public sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider fallback = new(options);

    public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var configuredPolicy = await fallback.GetPolicyAsync(policyName);
        if (configuredPolicy is not null)
        {
            return configuredPolicy;
        }

        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        fallback.GetFallbackPolicyAsync();
}
