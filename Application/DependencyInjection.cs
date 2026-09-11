using Microsoft.Extensions.Options;
using MyWebApp.Configurations;

namespace MyWebApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<ApplicationOptions>()
            .Bind(configuration.GetSection(ApplicationOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Name), "Application name is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Icon), "Application icon is required.")
            .ValidateOnStart();

        return services;
    }
}
