namespace MyWebApp.Configurations;

public sealed class SubmenuCatalogsOptions
{
    public const string SectionName = "SubmenuCatalogs";

    public Dictionary<string, SubmenuCatalogEntry> Catalogs { get; set; } = new();
}

public sealed class SubmenuCatalogEntry
{
    public string ProviderType { get; set; } = string.Empty;
    public string ParentRoute { get; set; } = "/";
    public string Icon { get; set; } = "circle";
}
