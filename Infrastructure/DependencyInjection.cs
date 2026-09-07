using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.Distributed;
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
        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException(
                "Connection string 'Redis' is required for the Microsoft identity token cache.");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = configuration["Redis:InstanceName"] ?? "MyWebApp:";
        });

        services.Configure<MsalDistributedTokenCacheAdapterOptions>(options =>
        {
            options.Encrypt = true;
        });

        services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(configuration.GetSection("AzureAd"))
            .EnableTokenAcquisitionToCallDownstreamApi(["User.Read"])
            .AddMicrosoftGraph(configuration.GetSection("MicrosoftGraph"))
            .AddDistributedTokenCaches();

        services.AddRazorPages()
            .AddMicrosoftIdentityUI();

        services.AddScoped<ICurrentUserService, MicrosoftCurrentUserService>();

        RegisterReportApi(services, configuration);
        RegisterSubmenuProviders(services, configuration);

        return services;
    }

    private static void RegisterReportApi(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<ReportsOptions>()
            .Bind(configuration.GetSection(ReportsOptions.SectionName))
            .ValidateOnStart();

        services.AddScoped<ReportMockService>();
        services.AddScoped<ISalesPeriodCatalog>(
            serviceProvider => serviceProvider.GetRequiredService<ReportMockService>());
        services.AddScoped<ISalesReportService>(
            serviceProvider => serviceProvider.GetRequiredService<ReportMockService>());
    }

    private static void RegisterSubmenuProviders(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<SubmenuCatalogsOptions>()
            .Bind(configuration.GetSection(SubmenuCatalogsOptions.SectionName))
            .ValidateOnStart();

        services.AddScoped<SubmenuProviderFactory>();
    }
}
