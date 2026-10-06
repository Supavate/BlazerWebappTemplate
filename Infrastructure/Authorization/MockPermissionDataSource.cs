namespace MyWebApp.Infrastructure.Authorization;

/// <summary>
/// In-memory group-to-permission mappings used by the mock permission service.
/// Replace these mappings with data from the production authorization store
/// without changing the authorization handler or policies.
/// </summary>
public static class MockPermissionDataSource
{
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> GroupPermissions =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["mock-report-readers"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Reports.View"
            },
            ["mock-settings-admins"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Settings.View",
                "Users.Manage"
            }
        };

    public static HashSet<string> FindPermissions(IEnumerable<string> groupIds)
    {
        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var groupId in groupIds.Where(groupId => !string.IsNullOrWhiteSpace(groupId)))
        {
            if (GroupPermissions.TryGetValue(groupId, out var groupPermissions))
            {
                permissions.UnionWith(groupPermissions);
            }
        }

        return permissions;
    }
}
