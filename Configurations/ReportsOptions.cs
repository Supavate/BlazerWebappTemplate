namespace MyWebApp.Configurations;

public sealed class ReportsOptions
{
    public const string SectionName = "Reports";

    /// <summary>Sidebar route that receives the data-driven period items.</summary>
    public string ParentRoute { get; set; } = "/reports";

    /// <summary>Fallback icon when a period does not provide one.</summary>
    public string Icon { get; set; } = "calendar_month";
}
