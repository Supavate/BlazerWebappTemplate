namespace MyWebApp.Navigation;

internal static class NavigationRouteHelper
{
    public static string Normalize(string route)
    {
        var trimmed = route.Trim();
        if (trimmed.Length == 0 || trimmed == "/")
        {
            return "/";
        }

        return $"/{trimmed.Trim('/')}";
    }

    public static string ToBaseRelativeHref(string route)
    {
        var trimmed = route.Trim();
        return trimmed.Length == 0 || trimmed == "/" 
            ? "./" 
            : $"./{trimmed.TrimStart('/')}";
    }
}
