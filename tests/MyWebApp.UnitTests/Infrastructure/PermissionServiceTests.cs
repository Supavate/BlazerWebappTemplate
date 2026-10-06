using MyWebApp.Infrastructure.Authorization;

namespace MyWebApp.UnitTests.Infrastructure;

public sealed class PermissionServiceTests
{
    private readonly PermissionService service = new();

    [Fact]
    public async Task GetPermissionsForGroupsAsync_CombinesKnownGroups()
    {
        var permissions = await service.GetPermissionsForGroupsAsync(
            ["mock-report-readers", "MOCK-SETTINGS-ADMINS"]);

        Assert.Equal(3, permissions.Count);
        Assert.Contains("Reports.View", permissions);
        Assert.Contains("Settings.View", permissions);
        Assert.Contains("Users.Manage", permissions);
    }

    [Fact]
    public async Task GetPermissionsForGroupsAsync_IgnoresUnknownAndDuplicateGroups()
    {
        var permissions = await service.GetPermissionsForGroupsAsync(
            ["unknown", "mock-report-readers", "MOCK-REPORT-READERS"]);

        Assert.Single(permissions);
        Assert.Contains("Reports.View", permissions);
    }

    [Fact]
    public async Task GetPermissionsForGroupsAsync_WithNoGroups_ReturnsEmptySet()
    {
        var permissions = await service.GetPermissionsForGroupsAsync([]);

        Assert.Empty(permissions);
    }
}
