using Microsoft.Extensions.Options;
using MyWebApp.Application.Abstractions.Reports;
using MyWebApp.Application.Models.Reports;
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
            ValidateEntries(catalogKey, periods);

            var dynamicItems = periods
                .Select(period => new NavItem(
                    period.Title.Trim(),
                    string.IsNullOrWhiteSpace(period.Icon) ? entry.Icon : period.Icon,
                    $"{parentRoute.TrimEnd('/')}/{Uri.EscapeDataString(period.Key.Trim())}",
                    0,
                    [],
                    []))
                .ToArray();

            items = items
                .Select(item => AppendToParent(item, parentRoute, dynamicItems))
                .ToArray();
        }

        return items;
    }

    private static NavItem AppendToParent(
        NavItem item,
        string parentRoute,
        IReadOnlyList<NavItem> dynamicItems)
    {
        if (string.Equals(item.Route, parentRoute, StringComparison.OrdinalIgnoreCase))
        {
            return item with { Children = item.Children.Concat(dynamicItems).ToArray() };
        }

        if (!item.HasChildren)
        {
            return item;
        }

        return item with
        {
            Children = item.Children
                .Select(child => AppendToParent(child, parentRoute, dynamicItems))
                .ToArray()
        };
    }

    private static void ValidateEntries(
        string catalogKey,
        IReadOnlyList<ISubmenuEntry> entries)
    {
        for (var index = 0; index < entries.Count; index++)
        {
            var entry = entries[index];
            if (string.IsNullOrWhiteSpace(entry.Key))
            {
                throw new InvalidOperationException(
                    $"Submenu catalog '{catalogKey}' contains an entry with a blank key at index {index}.");
            }

            if (string.IsNullOrWhiteSpace(entry.Title))
            {
                throw new InvalidOperationException(
                    $"Submenu catalog '{catalogKey}' contains an entry with a blank title at index {index}.");
            }
        }

        var duplicateKey = entries
            .GroupBy(entry => entry.Key.Trim(), StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateKey is not null)
        {
            throw new InvalidOperationException(
                $"Submenu catalog '{catalogKey}' contains duplicate key '{duplicateKey.Key}'.");
        }
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
