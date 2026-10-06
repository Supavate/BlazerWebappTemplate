namespace MyWebApp.Infrastructure.Authorization;

/// <summary>
/// Mock permission service backed by <see cref="MockPermissionDataSource"/>.
/// Replace this implementation with a repository-backed service when a real
/// authorization data source is available.
/// </summary>
public sealed class PermissionService
{
    public Task<HashSet<string>> GetPermissionsForGroupsAsync(
        IEnumerable<string> azureGroupIds,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(MockPermissionDataSource.FindPermissions(azureGroupIds));
    }
}
