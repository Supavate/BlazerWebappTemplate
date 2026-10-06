namespace MyWebApp.Configurations;

public sealed class ApplicationOptions
{
    public const string SectionName = "Application";

    public string Name { get; set; } = "My Web App";

    public string Icon { get; set; } = "Business";

    public string BasePath { get; set; } = string.Empty;
}
