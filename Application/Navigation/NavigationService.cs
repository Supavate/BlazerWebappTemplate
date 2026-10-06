using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MyWebApp.Navigation;

public sealed class NavigationService(IEnumerable<NavigationCatalogItem> catalogItems)
{

    public IReadOnlyList<NavItem> Items { get; } = BuildTree(catalogItems);

    private static IReadOnlyList<NavItem> BuildTree(IEnumerable<NavigationCatalogItem> catalogItems)
    {
        ArgumentNullException.ThrowIfNull(catalogItems);

        var routes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var componentTypes = new HashSet<Type>();
        var ancestors = new HashSet<NavigationCatalogItem>(ReferenceEqualityComparer.Instance);

        return catalogItems
            .Select(items => BuildItem(items, routes, componentTypes, ancestors))
            .ToArray();
    }

    private static NavItem BuildItem(
        NavigationCatalogItem item,
        ISet<string> routes,
        ISet<Type> componentTypes,
        ISet<NavigationCatalogItem> ancestors
    )
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!ancestors.Add(item))
        {
            throw new InvalidOperationException(
                $"Circular navigation relationship detected for '{item.ComponentType.FullName}'.");
        }
        try
        {
            ValidateMetadata(item);

            if (!typeof(IComponent).IsAssignableFrom(item.ComponentType))
            {
                throw new InvalidOperationException(
                    $"Navigation component '{item.ComponentType.FullName}' must implement IComponent.");
            }

            if (!componentTypes.Add(item.ComponentType))
            {
                throw new InvalidOperationException(
                    $"Navigation component '{item.ComponentType.FullName}' is declared more than once.");
            }

            var declaredRoutes = item.ComponentType.GetCustomAttributes<RouteAttribute>().ToArray();
            if (declaredRoutes.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Navigation component '{item.ComponentType.FullName}' must declare exactly one @page route.");
            }

            var route = NavigationRouteHelper.Normalize(declaredRoutes[0].Template);
            if (route.Contains('{', StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Navigation route '{route}' cannot contain route parameters.");
            }

            if (!routes.Add(route))
            {
                throw new InvalidOperationException(
                    $"Navigation route '{route}' is declared more than once.");
            }

            var authorizationData = item.ComponentType
                .GetCustomAttributes<AuthorizeAttribute>(inherit: true)
                .Cast<IAuthorizeData>()
                .ToArray();

            var children = (item.Children ?? [])
                .Select(child => BuildItem(child, routes, componentTypes, ancestors))
                .ToArray();
                
            return new NavItem(item.Title.Trim(), item.Icon, route, authorizationData, children);
        }
        finally
        {
            ancestors.Remove(item);
        }
    }

    private static void ValidateMetadata(NavigationCatalogItem item)
    {
        if (item.ComponentType is null)
        {
            throw new InvalidOperationException("A navigation component type is required.");
        }

        if (string.IsNullOrWhiteSpace(item.Title))
        {
            throw new InvalidOperationException(
                $"Navigation title is required for '{item.ComponentType.FullName}'.");
        }

        if (string.IsNullOrWhiteSpace(item.Icon))
        {
            throw new InvalidOperationException(
                $"Navigation icon is required for '{item.ComponentType.FullName}'.");
        }
    }
}
