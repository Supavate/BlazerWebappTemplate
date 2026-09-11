using Microsoft.AspNetCore.Components;
using MudBlazor;
using MyWebApp.Navigation;

namespace MyWebApp.UnitTests.Navigation;

public sealed class NavigationServiceTests
{
    [Fact]
    public void BuildsOrderedHierarchyFromPageMetadata()
    {
        var service = new NavigationService(typeof(NavigationServiceTests).Assembly);

        Assert.Collection(
            service.Items,
            home =>
            {
                Assert.Equal("Test Home", home.Title);
                Assert.Equal("/test-home", home.Route);
                Assert.Empty(home.Children);
            },
            settings =>
            {
                Assert.Equal("Test Settings", settings.Title);
                var users = Assert.Single(settings.Children);
                Assert.Equal("Test Users", users.Title);
                Assert.Equal("/test-settings/users", users.Route);
                var permissions = Assert.Single(users.Children);
                Assert.Equal("Test Permissions", permissions.Title);
                Assert.Equal("/test-settings/users/permissions", permissions.Route);
            });
    }

    [Route("/test-home")]
    [NavMenu("Test Home", Icons.Material.Filled.Home, 0)]
    private sealed class TestHomeComponent : ComponentBase;

    [Route("/test-settings")]
    [NavMenu("Test Settings", Icons.Material.Filled.Settings, 10)]
    private sealed class TestSettingsComponent : ComponentBase;

    [Route("/test-settings/users")]
    [NavMenu("Test Users", Icons.Material.Filled.People, 5, Parent = "/test-settings")]
    private sealed class TestUsersComponent : ComponentBase;

    [Route("/test-settings/users/permissions")]
    [NavMenu(
        "Test Permissions",
        Icons.Material.Filled.Security,
        5,
        Parent = "/test-settings/users")]
    private sealed class TestPermissionsComponent : ComponentBase;
}
