using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using MyWebApp.Application.Abstractions.Authentication;
using MyWebApp.Infrastructure.Authentication;

namespace MyWebApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            services.AddAuthentication(FakeHostAuthenticationHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, FakeHostAuthenticationHandler>(
                    FakeHostAuthenticationHandler.SchemeName,
                    _ => { });
            services.AddScoped<FakeAuthStateProvider>();
            services.AddScoped<AuthenticationStateProvider>(provider =>
                provider.GetRequiredService<FakeAuthStateProvider>());
            services.AddScoped<ICurrentUserService>(provider =>
                provider.GetRequiredService<FakeAuthStateProvider>());
        }
        else
        {
            throw new InvalidOperationException(
                "A production authentication provider must be registered outside Development.");
        }

        return services;
    }
}
