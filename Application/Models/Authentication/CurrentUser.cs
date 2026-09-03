namespace MyWebApp.Application.Models.Authentication;

public sealed record CurrentUser(
    string TenantId,
    string ObjectId,
    string DisplayName,
    string? Email,
    string? JobTitle,
    string? OfficeLocation,
    IReadOnlyCollection<string> Roles
);
