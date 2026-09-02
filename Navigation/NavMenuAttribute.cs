namespace MyWebApp.Navigation;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class NavMenuAttribute(string title, string icon) : Attribute
{
    public string Title { get; } = title;

    public string Icon { get; } = icon;

    public string? Parent { get; set; }

    public int Order { get; set; }
}
