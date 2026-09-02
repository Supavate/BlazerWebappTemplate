using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace MyWebApp.Navigation;

public sealed class NavigationService
{
    public NavigationService(Assembly assembly)
    {
        Items = BuildTree(assembly);
    }

    public IReadOnlyList<NavItem> Items { get; }

    private static IReadOnlyList<NavItem> BuildTree(Assembly assembly)
    {
        var pages = assembly.DefinedTypes
            .Where(type => !type.IsAbstract && typeof(IComponent).IsAssignableFrom(type))
            .Select(type => new
            {
                Type = type.AsType(),
                Menu = type.GetCustomAttribute<NavMenuAttribute>(),
                Routes = type.GetCustomAttributes<RouteAttribute>().ToArray()
            })
            .Where(page => page.Menu is not null)
            .Select(page => CreateNode(page.Type, page.Menu!, page.Routes))
            .ToArray();

        var duplicateRoute = pages
            .GroupBy(page => page.Route, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateRoute is not null)
        {
            throw new InvalidOperationException(
                $"Navigation route '{duplicateRoute.Key}' is declared more than once.");
        }

        var byRoute = pages.ToDictionary(page => page.Route, StringComparer.OrdinalIgnoreCase);

        foreach (var page in pages.Where(page => page.Parent is not null))
        {
            if (!byRoute.TryGetValue(page.Parent!, out var parent))
            {
                throw new InvalidOperationException(
                    $"Navigation item '{page.Title}' references missing parent route '{page.Parent}'.");
            }

            parent.Children.Add(page);
        }

        ValidateNoCycles(pages);

        return pages
            .Where(page => page.Parent is null)
            .OrderBy(page => page.Order)
            .ThenBy(page => page.Title, StringComparer.OrdinalIgnoreCase)
            .Select(ToNavItem)
            .ToArray();
    }

    private static MutableNavNode CreateNode(
        Type componentType,
        NavMenuAttribute menu,
        RouteAttribute[] routes)
    {
        if (string.IsNullOrWhiteSpace(menu.Title))
        {
            throw new InvalidOperationException(
                $"Navigation title is required for '{componentType.FullName}'.");
        }

        if (string.IsNullOrWhiteSpace(menu.Icon))
        {
            throw new InvalidOperationException(
                $"Navigation icon is required for '{componentType.FullName}'.");
        }

        if (routes.Length != 1)
        {
            throw new InvalidOperationException(
                $"Navigation component '{componentType.FullName}' must declare exactly one @page route.");
        }

        var route = NormalizeRoute(routes[0].Template);
        if (route.Contains('{', StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Navigation route '{route}' cannot contain route parameters.");
        }

        return new MutableNavNode(
            menu.Title.Trim(),
            menu.Icon,
            route,
            NormalizeOptionalRoute(menu.Parent),
            menu.Order);
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

    private static string? NormalizeOptionalRoute(string? route) =>
        string.IsNullOrWhiteSpace(route) ? null : NormalizeRoute(route);

    private static void ValidateNoCycles(IEnumerable<MutableNavNode> pages)
    {
        var visitState = new Dictionary<MutableNavNode, VisitState>();

        foreach (var page in pages)
        {
            Visit(page, visitState);
        }
    }

    private static void Visit(
        MutableNavNode page,
        IDictionary<MutableNavNode, VisitState> visitState)
    {
        if (visitState.TryGetValue(page, out var state))
        {
            if (state == VisitState.Visiting)
            {
                throw new InvalidOperationException(
                    $"Circular navigation relationship detected at route '{page.Route}'.");
            }

            return;
        }

        visitState[page] = VisitState.Visiting;
        foreach (var child in page.Children)
        {
            Visit(child, visitState);
        }

        visitState[page] = VisitState.Visited;
    }

    private static NavItem ToNavItem(MutableNavNode page) =>
        new(
            page.Title,
            page.Icon,
            page.Route,
            page.Order,
            page.Children
                .OrderBy(child => child.Order)
                .ThenBy(child => child.Title, StringComparer.OrdinalIgnoreCase)
                .Select(ToNavItem)
                .ToArray());

    private sealed class MutableNavNode(
        string title,
        string icon,
        string route,
        string? parent,
        int order)
    {
        public string Title { get; } = title;
        public string Icon { get; } = icon;
        public string Route { get; } = route;
        public string? Parent { get; } = parent;
        public int Order { get; } = order;
        public List<MutableNavNode> Children { get; } = [];
    }

    private enum VisitState
    {
        Visiting,
        Visited
    }
}
