using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace MyWebApp.Infrastructure.Authorization;

public sealed class PermissionAuthorizationHandler(PermissionService permissionService)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated is not true)
        {
            return;
        }

        var groupIds = context.User
            .FindAll("groups")
            .Concat(context.User.FindAll(ClaimTypes.GroupSid))
            .Select(claim => claim.Value);

        var permissions = await permissionService.GetPermissionsForGroupsAsync(groupIds);

        if (permissions.Contains(requirement.PermissionCode))
        {
            context.Succeed(requirement);
        }
    }
}
