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

        var type = ResolveProviderType(catalogKey, entry.ProviderType);
        var provider = serviceProvider.GetService(type) as ISubmenuProvider;
        if (provider is null)
        {
            throw new InvalidOperationException(
                $"Submenu provider type '{entry.ProviderType}' for catalog '{catalogKey}' " +
                $"is not registered as {nameof(ISubmenuProvider)} in the DI container.");
        }

        return provider;
    }

    private static Type ResolveProviderType(string catalogKey, string providerType)
    {
        var resolvedType = Type.GetType(providerType, throwOnError: false);
        if (resolvedType is not null)
        {
            return resolvedType;
        }

        var separatorIndex = providerType.IndexOf(',');
        if (separatorIndex < 0)
        {
            throw new InvalidOperationException(
                $"Submenu provider type '{providerType}' for catalog '{catalogKey}' could not be resolved.");
        }

        var typeName = providerType[..separatorIndex].Trim();
        var configuredAssemblyName = providerType[(separatorIndex + 1)..].Trim();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var assemblyName = assembly.GetName().Name;
            if (!string.Equals(assemblyName, configuredAssemblyName, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(assembly.FullName?.Split(',')[0], configuredAssemblyName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var fallbackType = assembly.GetType(typeName, throwOnError: false);
            if (fallbackType is not null)
            {
                return fallbackType;
            }
        }

        throw new InvalidOperationException(
            $"Submenu provider type '{providerType}' for catalog '{catalogKey}' could not be resolved.");
    }
}
