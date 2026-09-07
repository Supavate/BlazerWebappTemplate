using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using MyWebApp.Navigation;

namespace MyWebApp.UnitTests.Navigation;

public sealed class NavigationAuthorizationServiceTests
{
    [Fact]
    public async Task FilteringPreservesSiblingOrderAndRemovesDeniedBranches()
    {
        var authorizationOptions = new AuthorizationOptions();
        authorizationOptions.AddPolicy(
            "Deny",
            policy => policy.Requirements.Add(new DenyRequirement()));
        var policyProvider = new DefaultAuthorizationPolicyProvider(
            Options.Create(authorizationOptions));
        var service = new NavigationAuthorizationService(
            new TestAuthenticationStateProvider(),
            policyProvider,
            new TestAuthorizationService());
        var items = new[]
        {
            CreateItem("First", "/first"),
            CreateItem(
                "Denied",
                "/denied",
                [new AuthorizeAttribute { Policy = "Deny" }],
                [CreateItem("Generated child", "/denied/generated")]),
            CreateItem("Third", "/third")
        };

        var authorized = await service.GetAuthorizedItemsAsync(items);

        Assert.Equal(["/first", "/third"], authorized.Select(item => item.Route));
    }

    private static NavItem CreateItem(
        string title,
        string route,
        IReadOnlyList<IAuthorizeData>? authorizationData = null,
        IReadOnlyList<NavItem>? children = null) =>
        new(title, "circle", route, 0, authorizationData ?? [], children ?? []);

    private sealed class TestAuthenticationStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
            Task.FromResult(new AuthenticationState(
                new ClaimsPrincipal(new ClaimsIdentity("Test"))));
    }

    private sealed class DenyRequirement : IAuthorizationRequirement;

    private sealed class TestAuthorizationService : IAuthorizationService
    {
        public Task<AuthorizationResult> AuthorizeAsync(
            ClaimsPrincipal user,
            object? resource,
            IEnumerable<IAuthorizationRequirement> requirements) =>
            Task.FromResult(requirements.Any(requirement => requirement is DenyRequirement)
                ? AuthorizationResult.Failed()
                : AuthorizationResult.Success());

        public Task<AuthorizationResult> AuthorizeAsync(
            ClaimsPrincipal user,
            object? resource,
            string policyName) =>
            Task.FromResult(AuthorizationResult.Success());
    }
}
