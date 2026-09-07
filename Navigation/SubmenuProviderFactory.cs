using Microsoft.Extensions.Options;
using MyWebApp.Application.Abstractions.Reports;
using MyWebApp.Configurations;

namespace MyWebApp.Navigation;

public sealed class SubmenuProviderFactory(
    IServiceProvider serviceProvider,
    IOptions<SubmenuCatalogsOptions> options)
{
    public ISubmenuProvider GetProvider(string catalogKey)
    {
        if (!options.Value.Catalogs.TryGetValue(catalogKey, out var entry))
        {
            throw new InvalidOperationException(
                $"Submenu catalog '{catalogKey}' is not configured in {SubmenuCatalogsOptions.SectionName}.");
        }

        var type = Type.GetType(entry.ProviderType, throwOnError: true)
            ?? throw new InvalidOperationException(
                $"Submenu provider type '{entry.ProviderType}' for catalog '{catalogKey}' could not be resolved.");
        var provider = serviceProvider.GetService(type) as ISubmenuProvider;
        if (provider is null)
        {
            throw new InvalidOperationException(
                $"Submenu provider type '{entry.ProviderType}' for catalog '{catalogKey}' " +
                $"is not registered as {nameof(ISubmenuProvider)} in the DI container.");
        }

        return provider;
    }
}
