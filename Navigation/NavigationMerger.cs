using Microsoft.Extensions.Options;
using MyWebApp.Application.Abstractions.Reports;
using MyWebApp.Configurations;

namespace MyWebApp.Navigation;

/// <summary>
/// Combines the statically discovered <see cref="NavItem"/> tree with
/// data-driven items produced by <see cref="ISalesPeriodCatalog"/>. Each
/// period becomes a sidebar entry under the configured parent route, and all
/// entries point at the shared report template page.
/// </summary>
public sealed class NavigationMerger(
    NavigationService navigationService,
    ISalesPeriodCatalog periodCatalog,
    IOptions<ReportsOptions> options)
{
    public async Task<IReadOnlyList<NavItem>> GetItemsAsync(
        CancellationToken cancellationToken = default)
    {
        var periods = await periodCatalog.GetPeriodsAsync(cancellationToken);
        if (periods.Count == 0)
        {
            return navigationService.Items;
        }

        var parentRoute = NormalizeRoute(options.Value.ParentRoute);
        var dynamicItems = periods
            .OrderBy(period => period.Order)
            .ThenBy(period => period.Title, StringComparer.OrdinalIgnoreCase)
            .Select(period => new NavItem(
                period.Title.Trim(),
                string.IsNullOrWhiteSpace(period.Icon) ? options.Value.Icon : period.Icon,
                $"{parentRoute}/{Uri.EscapeDataString(period.Key)}",
                period.Order,
                [],
                []))
            .ToArray();

        return navigationService.Items
            .Select(item => item.Route == parentRoute
                ? item with { Children = item.Children.Concat(dynamicItems).ToArray() }
                : item)
            .ToArray();
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