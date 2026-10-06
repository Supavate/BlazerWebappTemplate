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
}
