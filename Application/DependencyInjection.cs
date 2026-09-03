using Microsoft.Extensions.Options;
using MyWebApp.Configurations;

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
                if (policySettings.Roles.Length == 0)
                {
                    throw new InvalidOperationException(
                        $"Policy '{policyName}' must have at least one role defined."
                    );
                }

                options.AddPolicy(policyName, policy =>
                    policy.RequireRole(policySettings.Roles)
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
