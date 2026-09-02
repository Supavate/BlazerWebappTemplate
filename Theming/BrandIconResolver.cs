using MudBlazor;

namespace MyWebApp.Theming;

public static class BrandIconResolver
{
    public static string Resolve(string iconName) => iconName.Trim().ToUpperInvariant() switch
    {
        "DATABASE" => Icons.Material.Filled.Storage,
        "HUB" => Icons.Material.Filled.Hub,
        "INVENTORY" => Icons.Material.Filled.Inventory2,
        _ => Icons.Material.Filled.Business
    };
}
