# MyWebApp Developer Guide

MyWebApp is a .NET 10 Blazor Web App using interactive server rendering and MudBlazor. It includes automatic sidebar navigation, a centralized theme, development-only fake authentication, and a backend structure ready for application and infrastructure services.

## Quick start

Requirements:

- .NET 10 SDK
- A trusted ASP.NET Core development certificate for local HTTPS

From the project directory:

```powershell
dotnet restore
dotnet run --launch-profile https
```

Open `https://localhost:7174`. The HTTP profile is available at `http://localhost:5095`.

The login page offers development-only User and Admin identities. Fake authentication is intentionally blocked outside the Development environment.

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
| Change sidebar structure, width, or page layout | `Components/Layout/MainLayout.razor` |
| Change sidebar brand and navigation styling | `Components/Layout/MainLayout.razor.css` |
| Change sidebar account footer and sign-out styling | `Components/Layout/AuthenticationControls.razor` and `.razor.css` |
| Change the login layout | `Components/Layout/LoginLayout.razor` and `.razor.css` |
| Change a specific page | Its folder under `Components/Pages/<PageName>/` |
| Change automatic menu discovery rules | `Navigation/NavigationService.cs` and `NavMenuAttribute.cs` |
| Change application configuration validation | `Application/DependencyInjection.cs` |
| Register databases, external clients, or provider implementations | `Infrastructure/DependencyInjection.cs` |
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
@attribute [Authorize]
@attribute [NavMenu("Reports", Icons.Material.Filled.Assessment, Order = 10)]

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
- Add `[AllowAnonymous]` only when the page must be accessible without login; otherwise use `[Authorize]`.

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
@attribute [Authorize]
@attribute [NavMenu(
    "Audit log",
    Icons.Material.Filled.History,
    Parent = "/settings",
    Order = 20)]

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

Use `.RequireAuthorization()` for protected endpoint groups. Use `.AllowAnonymous()` only for endpoints that deliberately need public access, such as health probes.

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

## Production and Docker

The Dockerfile uses .NET 10 images and publishes the application on port 8080. However, production startup intentionally fails until a real authentication provider is registered in `Infrastructure/DependencyInjection.cs`. Replace the development fake provider with the chosen production authentication integration before building a deployable container.

Build the image after production authentication is configured:

```powershell
docker build -f DOCKERFILE -t mywebapp .
docker run --rm -p 8080:8080 mywebapp
```
