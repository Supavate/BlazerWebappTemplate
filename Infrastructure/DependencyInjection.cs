using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using MyWebApp.Application.Abstractions.Authentication;
using MyWebApp.Application.Abstractions.Reports;
using MyWebApp.Configurations;
using MyWebApp.Infrastructure.Authentication;
using MyWebApp.Infrastructure.Reports;
using MyWebApp.Navigation;

namespace MyWebApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(configuration.GetSection("AzureAd"))
            .EnableTokenAcquisitionToCallDownstreamApi(["User.Read"])
            .AddMicrosoftGraph(configuration.GetSection("MicrosoftGraph"))
            .AddInMemoryTokenCaches();

        services.AddRazorPages()
            .AddMicrosoftIdentityUI();

        services.AddScoped<ICurrentUserService, MicrosoftCurrentUserService>();

        services.AddOptions<SubmenuCatalogsOptions>()
            .Bind(configuration.GetSection(SubmenuCatalogsOptions.SectionName))
            .ValidateOnStart();

        services.AddScoped<ReportMockService>();
        services.AddScoped<ISalesPeriodCatalog>(serviceProvider =>
            serviceProvider.GetRequiredService<ReportMockService>());
        services.AddScoped<ISalesReportService>(serviceProvider =>
            serviceProvider.GetRequiredService<ReportMockService>());

        services.AddScoped<SubmenuProviderFactory>();
        services.AddScoped<NavigationMerger>();
        services.AddScoped<NavigationAuthorizationService>();
        services.AddScoped<NavigationRefreshService>();

        return services;
    }
}
