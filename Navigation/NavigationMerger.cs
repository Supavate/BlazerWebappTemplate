using Microsoft.Extensions.Options;
using MyWebApp.Application.Abstractions.Reports;
using MyWebApp.Configurations;

namespace MyWebApp.Navigation;

/// <summary>
/// Combines the statically discovered NavItem tree with data-driven items
/// produced by all configured <see cref="ISubmenuProvider"/> instances.
/// </summary>
public sealed class NavigationMerger(
    NavigationService navigationService,
    SubmenuProviderFactory factory,
    IOptions<SubmenuCatalogsOptions> options)
{
    public async Task<IReadOnlyList<NavItem>> GetItemsAsync(
        CancellationToken cancellationToken = default)
    {
        var items = navigationService.Items;

        foreach (var (catalogKey, entry) in options.Value.Catalogs)
        {
            var provider = factory.GetProvider(catalogKey);
            var periods = await provider.GetItemsAsync(cancellationToken);
            if (periods.Count == 0)
            {
                continue;
            }

            var parentRoute = NormalizeRoute(entry.ParentRoute);
            var dynamicItems = periods
                .OrderBy(period => period.Order)
                .ThenBy(period => period.Title, StringComparer.OrdinalIgnoreCase)
                .Select(period => new NavItem(
                    period.Title.Trim(),
                    string.IsNullOrWhiteSpace(period.Icon) ? entry.Icon : period.Icon,
                    $"{parentRoute}/{Uri.EscapeDataString(period.Key)}",
                    period.Order,
                    [],
                    []))
                .ToArray();

            items = items
                .Select(item => item.Route == parentRoute
                    ? item with { Children = item.Children.Concat(dynamicItems).ToArray() }
                    : item)
                .ToArray();
        }

        return items;
    }

    private static string NormalizeRoute(string route)
    {
        var trimmed = route.Trim();
        if (trimmed.Length == 0 || trimmed == "/")
        {
            return "/";
        }

        return $"/{trimmed.Trim('/')}";
    }
}
