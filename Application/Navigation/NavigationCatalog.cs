using MudBlazor;
namespace MyWebApp.Navigation;

public static class NavigationCatalog {
    public static readonly NavigationCatalogItem Home = new(
        typeof(Components.Pages.Home.Home),
        "Home",
        Icons.Material.Filled.Home
    );
    public static readonly NavigationCatalogItem Reports = new(
        typeof(Components.Pages.Reports.Reports),
        "Reports",
        Icons.Material.Filled.BarChart
    );
    public static readonly NavigationCatalogItem Settings = new(
        typeof(Components.Pages.Settings.Settings),
        "Settings",
        Icons.Material.Filled.Settings
    );
    public static IReadOnlyList<NavigationCatalogItem> Items { get; } = [
        Home,
        Reports,
        Settings,
    ];
}

public sealed record NavigationCatalogItem (
    Type ComponentType,
    string Title,
    string Icon,
    IReadOnlyList<NavigationCatalogItem>? Children = null
);
