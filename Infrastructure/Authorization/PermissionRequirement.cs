using Microsoft.AspNetCore.Authorization;

namespace MyWebApp.Infrastructure.Authorization;

public sealed record PermissionRequirement(string PermissionCode)
    : IAuthorizationRequirement;
