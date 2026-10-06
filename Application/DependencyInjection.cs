using Microsoft.Extensions.Options;
using MyWebApp.Configurations;
using MyWebApp.Infrastructure.Authorization;

namespace MyWebApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authorizationSettings = configuration
            .GetRequiredSection(AppAuthorizationOptions.SectionName)
            .Get<AppAuthorizationOptions>()
            ?? throw new InvalidOperationException(
                "Authorization settings are not configured properly."
            );

        services.AddAuthorization(options =>
        {
            foreach (var (policyName, policySettings) in authorizationSettings.Policies)
            {
                if (string.IsNullOrWhiteSpace(policySettings.Permission))
                {
                    throw new InvalidOperationException(
                        $"Policy '{policyName}' must have a permission defined."
                    );
                }

                options.AddPolicy(policyName, policy =>
                    policy.RequireAuthenticatedUser()
                        .AddRequirements(new PermissionRequirement(policySettings.Permission))
                );
            }
        });

        services.AddOptions<ApplicationOptions>()
            .Bind(configuration.GetSection(ApplicationOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Name), "Application name is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Icon), "Application icon is required.")
            .ValidateOnStart();

        return services;
    }
}
