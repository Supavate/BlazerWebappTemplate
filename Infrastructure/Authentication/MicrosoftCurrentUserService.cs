using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using MyWebApp.Application.Abstractions.Authentication;
using MyWebApp.Application.Models.Authentication;

namespace MyWebApp.Infrastructure.Authentication;

internal sealed class MicrosoftCurrentUserService(
    AuthenticationStateProvider authenticationStateProvider,
    GraphServiceClient graphClient,
    ILogger<MicrosoftCurrentUserService> logger) : ICurrentUserService
{
    private CurrentUser? cachedUser;

    public async Task<CurrentUser?> GetCurrentUserAsync(
        CancellationToken cancellationToken = default)
    {
        if (cachedUser is not null)
        {
            return cachedUser;
        }

        var principal = (await authenticationStateProvider
            .GetAuthenticationStateAsync()).User;

        if (principal.Identity?.IsAuthenticated is not true)
        {
            return null;
        }

        var tenantId = FindClaim(
            principal,
            "tid",
            "http://schemas.microsoft.com/identity/claims/tenantid");
        var objectId = FindClaim(
            principal,
            "oid",
            "http://schemas.microsoft.com/identity/claims/objectidentifier");

        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(objectId))
        {
            throw new InvalidOperationException(
                "The authenticated Microsoft account does not contain tenant and object identifiers.");
        }

        User? graphUser = null;
        try
        {
            graphUser = await graphClient.Me.GetAsync(
                request => request.QueryParameters.Select =
                [
                    "displayName",
                    "mail",
                    "userPrincipalName",
                    "jobTitle",
                    "officeLocation"
                ],
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Could not retrieve the current user's Microsoft Graph profile; using identity claims instead.");
        }

        var claimEmail = FindClaim(
            principal,
            "preferred_username",
            "upn",
            "email",
            ClaimTypes.Email);
        var displayName = graphUser?.DisplayName
            ?? FindClaim(principal, "name", ClaimTypes.Name)
            ?? claimEmail
            ?? objectId;
        var email = graphUser?.Mail
            ?? graphUser?.UserPrincipalName
            ?? claimEmail;
        var roles = principal.FindAll(ClaimTypes.Role)
            .Concat(principal.FindAll("roles"))
            .Select(claim => claim.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        cachedUser = new CurrentUser(
            tenantId,
            objectId,
            displayName,
            email,
            graphUser?.JobTitle,
            graphUser?.OfficeLocation,
            roles);

        return cachedUser;
    }

    private static string? FindClaim(
        ClaimsPrincipal principal,
        params string[] claimTypes)
    {
        foreach (var claimType in claimTypes)
        {
            var value = principal.FindFirst(claimType)?.Value;
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }
}
