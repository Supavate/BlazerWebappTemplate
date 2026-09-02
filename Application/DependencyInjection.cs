using Microsoft.Extensions.Options;
using MyWebApp.Configuration;

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

        services.AddOptions<AuthenticationOptions>()
            .Bind(configuration.GetSection(AuthenticationOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.FakeUserName), "Fake user name is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.FakeUserEmail), "Fake user email is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.DefaultRole), "Default fake role is required.")
            .ValidateOnStart();

        return services;
    }
}
