using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.Distributed;
using Microsoft.Identity.Web.UI;
using MyWebApp.Application.Abstractions.Authentication;
using MyWebApp.Infrastructure.Authentication;

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

        return services;
    }
}
