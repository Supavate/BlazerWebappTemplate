using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;

namespace MyWebApp.Navigation;

public sealed class NavigationAuthorizationService(
    AuthenticationStateProvider authenticationStateProvider,
    IAuthorizationPolicyProvider policyProvider,
    IAuthorizationService authorizationService)
{
    public async Task<IReadOnlyList<NavItem>> GetAuthorizedItemsAsync(
        IEnumerable<NavItem> items)
    {
        var user = (await authenticationStateProvider
            .GetAuthenticationStateAsync()).User;
        var authorizedItems = new List<NavItem>();

        foreach (var item in items)
        {
            var authorizedItem = await FilterAsync(item, user);
            if (authorizedItem is not null)
            {
                authorizedItems.Add(authorizedItem);
            }
        }

        return authorizedItems;
    }

    private async Task<NavItem?> FilterAsync(
        NavItem item,
        System.Security.Claims.ClaimsPrincipal user)
    {
        if (!await IsAuthorizedAsync(item, user))
        {
            return null;
        }

        var authorizedChildren = new List<NavItem>();
        foreach (var child in item.Children)
        {
            var authorizedChild = await FilterAsync(child, user);
            if (authorizedChild is not null)
            {
                authorizedChildren.Add(authorizedChild);
            }
        }

        return item with { Children = authorizedChildren };
    }

    private async Task<bool> IsAuthorizedAsync(
        NavItem item,
        System.Security.Claims.ClaimsPrincipal user)
    {
        if (item.AuthorizationData.Count == 0)
        {
            return true;
        }

        var policy = await AuthorizationPolicy.CombineAsync(
            policyProvider,
            item.AuthorizationData);

        return policy is null ||
            (await authorizationService.AuthorizeAsync(user, resource: null, policy)).Succeeded;
    }
}
