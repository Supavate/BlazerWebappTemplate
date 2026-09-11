using Microsoft.AspNetCore.Authorization;

namespace MyWebApp.Navigation;

public sealed record NavItem(
    string Title,
    string Icon,
    string Route,
    int Order,
    IReadOnlyList<IAuthorizeData> AuthorizationData,
    IReadOnlyList<NavItem> Children)
{
    public bool HasChildren => Children.Count > 0;
}
