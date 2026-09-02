namespace MyWebApp.Configuration;

public sealed class ApplicationOptions
{
    public const string SectionName = "Application";

    public string Name { get; set; } = "My Web App";

    public string Icon { get; set; } = "Business";
}
