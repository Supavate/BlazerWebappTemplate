namespace MyWebApp.Navigation;

public static class NavigationLandingSelector
{
    public const string NoAccessRoute = "/error/403";

    public static string GetDestination(IReadOnlyList<NavItem> authorizedItems) =>
        authorizedItems.FirstOrDefault()?.Route ?? NoAccessRoute;
}
