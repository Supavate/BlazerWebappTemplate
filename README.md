# MyWebApp Developer Guide

MyWebApp is a .NET 10 Blazor Web App using interactive server rendering and MudBlazor. It includes automatic sidebar navigation, a centralized theme, a health-check API, and a backend structure ready for application and infrastructure services.

## Quick start

Requirements:

- .NET 10 SDK
- A trusted ASP.NET Core development certificate for local HTTPS
- Redis 6 or later

Start Redis locally and make sure it is reachable at `localhost:6379`.

From the project directory:

```powershell
dotnet restore
dotnet user-secrets set "AzureAd:ClientSecret" "YOUR-CLIENT-SECRET"
dotnet run --launch-profile https
```

Open `https://localhost:7174`. Local authentication uses HTTPS so its generated redirect URI matches the Entra app registration.

The default Redis connection is `localhost:6379`. Override it without editing committed settings when necessary:

```powershell
$env:ConnectionStrings__Redis = "redis-host:6379,password=YOUR_PASSWORD,ssl=true,abortConnect=false"
dotnet run --launch-profile https
```

Microsoft Entra ID authentication is registered through Microsoft Identity Web. Every Blazor page and interactive circuit requires an authenticated organizational user; the health-check API remains public.

The application requests the delegated Microsoft Graph `User.Read` permission. Add this permission under **App registrations > API permissions > Microsoft Graph > Delegated permissions** in Microsoft Entra. The current-user service calls `/me` for display name, email, job title, and office location while retaining the tenant ID, object ID, and roles from authentication claims.

Inject `ICurrentUserService` wherever the application needs the signed-in profile:

```csharp
var currentUser = await currentUserService.GetCurrentUserAsync(cancellationToken);
```

`CurrentUser` uses the stable tenant ID and object ID as identity values. Email is resolved from Graph `mail`, Graph `userPrincipalName`, then the available username/email claim. If Graph is unavailable, the service logs a warning and returns the claim-based profile without Graph-only fields such as job title.

Microsoft Identity Web stores user tokens in Redis instead of process memory. This keeps the MSAL account available when the application restarts and prevents an existing authentication cookie from producing `MsalUiRequiredException` with `ErrorCode: user_null`. Token-cache entries are encrypted with ASP.NET Core Data Protection before being written to Redis.

## TODO: production Redis and stale-session recovery

- [ ] Provision a production Redis service and set `ConnectionStrings__Redis` in the hosting platform's secret store. Require TLS and authentication; don't commit its password or access key.
- [ ] Give each environment a distinct `Redis__InstanceName`, such as `MyWebApp:Production:`, so development, staging, and production token caches never overlap.
- [ ] Persist ASP.NET Core Data Protection keys in a durable key store shared by every application instance.
- [ ] Configure Redis persistence, availability, backups, network restrictions, and monitoring according to the hosting environment's recovery requirements.
- [ ] Add a cookie-validation or HTTP challenge recovery path for the remaining cache-loss case. When Microsoft Identity Web reports `MicrosoftIdentityWebChallengeUserException`/`user_null`, reject the stale authentication cookie and start a fresh OpenID Connect sign-in. Don't attempt the challenge from an active Blazor SignalR circuit, where the HTTP response may already have started.
- [ ] Test the recovery flow: sign in, load the Graph profile, restart the web application while Redis stays running, and confirm the profile still loads without another sign-in. Then deliberately flush only the test token-cache database and confirm the next full HTTP request reauthenticates instead of repeatedly logging `user_null`.

For an already-stale local session, clear the localhost authentication cookies (or sign out), keep Redis running, restart the application, and sign in once. The new authorization-code exchange will populate Redis.

The Tenant ID and Client ID belong in `appsettings.json`. Keep the client secret out of source control; use user secrets locally and the hosting platform's secure credential store in production. Register `https://localhost:7174/signin-oidc` and `https://localhost:7174/signout-callback-oidc` as Web redirect URIs in the Entra app registration.

Run the tests with:

```powershell
dotnet test tests/MyWebApp.UnitTests/MyWebApp.UnitTests.csproj
```

## Where to edit what

| Goal | File or folder |
| --- | --- |
| Change application name or brand icon | `appsettings.json`, under `Application` |
| Add another supported brand icon name | `Theming/BrandIconResolver.cs` |
| Change colors, spacing, typography, radii, or shadows | `wwwroot/css/theme.css` |
| Change reusable page utilities, cards, tables, and status styles | `wwwroot/css/utilities.css` |
| Change global HTML/body behavior | `wwwroot/app.css` |
| Change sidebar structure, responsive behavior, or navigation styling | `Components/Layout/AppSidebar.razor` and `.razor.css` |
| Change the page-content gutter, width, or error presentation | `Components/Layout/AppDocument.razor` and `.razor.css` |
| Change sidebar account footer and sign-out styling | `Components/Layout/AuthenticationControls.razor` and `.razor.css` |
| Change a specific page | Its folder under `Components/Pages/<PageName>/` |
| Change automatic menu discovery rules | `Navigation/NavigationService.cs` and `NavMenuAttribute.cs` |
| Change application configuration validation | `Application/DependencyInjection.cs` |
| Register databases, external clients, or provider implementations | `Infrastructure/DependencyInjection.cs` |
| Change the current Microsoft user model or Graph profile mapping | `Application/Models/Authentication/CurrentUser.cs` and `Infrastructure/Authentication/MicrosoftCurrentUserService.cs` |
| Add HTTP API endpoints | `Endpoints/` and the mapping call in `Program.cs` |
| Change middleware or application startup | `Program.cs` |

Keep page-specific CSS beside its Razor component using the same base filename, for example `Reports.razor` and `Reports.razor.css`. Blazor scopes that CSS to the component.

## Project boundaries

- `Components/` contains Razor pages, layouts, and UI behavior.
- `Navigation/` discovers page metadata and builds the recursive sidebar tree.
- `Application/` contains use cases, interfaces, and business rules. It should not depend on infrastructure implementations.
- `Infrastructure/` contains authentication, database, file, and external-service implementations.
- `Configuration/` contains strongly typed settings classes.
- `Endpoints/` contains HTTP endpoint modules. Endpoint handlers should delegate non-trivial work to Application services.
- `Theming/` maps application branding to MudBlazor.
- `wwwroot/` contains global CSS and public static assets.
- `tests/` contains automated tests.

## Add a new page

Every page has its own folder. For a Reports page, create:

```text
Components/Pages/Reports/
├── Reports.razor
└── Reports.razor.css
```

Use this starting component:

```razor
@page "/reports"
@attribute [NavMenu("Reports", Icons.Material.Filled.Assessment, 10)]

<PageTitle>Reports · SiS Workspace</PageTitle>

<section class="portal-page">
    <header class="portal-page-header">
        <div class="portal-page-header-copy">
            <p class="portal-eyebrow">Analytics</p>
            <h1 class="portal-page-title">Reports</h1>
            <p class="portal-page-subtitle">Review application reporting data.</p>
        </div>
    </header>
</section>
```

Navigation is generated automatically when the application starts. No sidebar markup needs to be edited.

The navigation rules are:

- Declare exactly one `@page` route on a component using `NavMenu`.
- Provide a non-empty menu title and MudBlazor icon.
- Use `Order` to control placement; lower values appear first.
- Route parameters are not supported for sidebar entries.
- Omit `NavMenu` when a routable page should not appear in the sidebar.
- All Blazor pages are protected globally by the authorized Razor component endpoint. Add `[Authorize(Roles = "...")]` only when a page needs a stricter role requirement.
- Sidebar items are filtered using the destination page's authorization metadata. A denied parent removes its complete navigation branch.

To protect a feature folder and its nested pages with one policy, add an `_Imports.razor` file to that folder. For example, `Components/Pages/Settings/_Imports.razor` contains:

```razor
@using Microsoft.AspNetCore.Authorization
@attribute [Authorize(Policy = "ViewSettings")]
```

The policy applies recursively to routed components in the folder, and the sidebar uses the same compiled metadata to hide the corresponding items from unauthorized users. Route authorization remains the security boundary; menu filtering only prevents users from seeing links they cannot open.

Restart the application after adding or changing navigation metadata because the menu is discovered at startup.

## Add a submenu page

The sidebar supports nested child levels. Put each child page in its own folder under the parent feature:

```text
Components/Pages/Settings/AuditLog/
├── AuditLog.razor
└── AuditLog.razor.css
```

```razor
@page "/settings/audit-log"
@attribute [NavMenu(
    "Audit log",
    Icons.Material.Filled.History,
    20,
    Parent = "/settings")]

<PageTitle>Audit log · SiS Workspace</PageTitle>

<section class="portal-page">
    <h1 class="portal-page-title">Audit log</h1>
</section>
```

`Parent` must exactly match the route of another component that has `NavMenu`. To add another level, point the new page's `Parent` to the route of an existing child. The navigation component renders the resulting tree recursively.

## Add an API

Create one endpoint module per feature. For example, create `Endpoints/WidgetEndpoints.cs`:

```csharp
namespace MyWebApp.Endpoints;

public static class WidgetEndpoints
{
    public static IEndpointRouteBuilder MapWidgetEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/widgets")
            .WithTags("Widgets")
            .RequireAuthorization();

        group.MapGet("/{id:int}", (int id) =>
            Results.Ok(new WidgetResponse(id, $"Widget {id}")))
            .WithName("GetWidget");

        return endpoints;
    }

    private sealed record WidgetResponse(int Id, string Name);
}
```

Then import the endpoint namespace and map it in `Program.cs`:

```csharp
using MyWebApp.Endpoints;

// After app.MapStaticAssets()
app.MapWidgetEndpoints();
```

For real features:

1. Define the use case or service interface in `Application/`.
2. Implement database or external-system access in `Infrastructure/`.
3. Register those services in the matching `DependencyInjection.cs` file.
4. Inject the Application service into the endpoint handler.
5. Keep request/response HTTP models close to the endpoint or in a feature-specific contracts folder.
6. Return intentional status codes such as `Ok`, `Created`, `NoContent`, `NotFound`, and `ValidationProblem`.
7. Pass `CancellationToken` through asynchronous endpoint and service calls.

API endpoints are separate from the globally protected Blazor endpoint. Use `.RequireAuthorization()` for protected API groups and keep `.AllowAnonymous()` only on endpoints that deliberately need public access, such as health probes.

## Health-check API

The project includes a public health endpoint:

```http
GET /api/health
```

Test it locally:

```powershell
Invoke-RestMethod http://localhost:5095/api/health
```

Example response:

```json
{
  "status": "Healthy",
  "timestampUtc": "2026-09-02T00:00:00+00:00",
  "durationMilliseconds": 0.12,
  "checks": {}
}
```

The registration is in `Program.cs`; the response and route mapping are in `Endpoints/HealthEndpoints.cs`.

When adding infrastructure, register named checks with the existing health-check builder, for example:

```csharp
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");
```

Implement `DatabaseHealthCheck` using `IHealthCheck`. Its result will then appear inside the `checks` object.

## Configuration and branding

Default settings are in `appsettings.json`; development overrides belong in `appsettings.Development.json`. Environment variables override nested configuration with double underscores, for example:

```powershell
$env:Application__Name = "Operations Portal"
$env:Application__Icon = "Inventory"
dotnet run --launch-profile http
```

Supported icon names are `Database`, `Hub`, and `Inventory`; unknown values fall back to the Business icon. Add mappings in `Theming/BrandIconResolver.cs`.

Do not commit credentials or connection strings. Use .NET user secrets for local development and the deployment platform's secret store in hosted environments.

## Styling guidance

Use existing CSS variables instead of hard-coded colors or spacing:

```css
.report-panel {
    background: var(--bg-surface);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
    padding: var(--space-5);
}
```

Edit the variable values in `wwwroot/css/theme.css` to change the application consistently. Reusable structural classes belong in `wwwroot/css/utilities.css`; page-only styles belong in that page's scoped `.razor.css` file.
