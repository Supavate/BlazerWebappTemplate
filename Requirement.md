# Project: Blazor Web App Template (POC)

## 1. Project Overview

Build a reusable **Blazor Server** web application template that serves as a starting point for future internal projects. The template should include authentication, a dynamic navigation sidebar, a consistent theming system, and a modern UI component library — all structured so that **adding new pages requires zero manual edits to layout, navigation, or CSS**.

---

## 2. Tech Stack

| Component | Choice |
| --- | --- |
| Framework | ASP.NET Core Blazor (.NET 10) |
| Render Mode | Interactive Server (`--interactivity Server`) |
| UI Component Library | MudBlazor |
| Authentication | Microsoft Entra ID (Azure AD) organizational SSO using `Microsoft.Identity.Web` |
| Styling | CSS custom properties (design tokens) + MudBlazor theme, mapped together |
| Target Platform | Desktop web only (no mobile responsiveness required for now) |
| IDE | Visual Studio / VS Code |

---

## 3. Functional Requirements

### 3.1 Authentication

- The fake authentication provider and dedicated fake login page must not be included.
- Every Blazor page and interactive circuit requires Microsoft Entra ID authentication using `Microsoft.Identity.Web` and organizational SSO.
- The health-check API remains anonymously accessible for monitoring.
- The sidebar account control must remain provider-neutral, render only for an authenticated user, and target the Microsoft Identity sign-out endpoint.
- Enforce authentication globally on the Razor component endpoint; use page-level `[Authorize(Roles = "...")]` only for stricter role requirements.

### 3.2 Dynamic Sidebar Navigation

- Sidebar must **automatically discover pages** via reflection — no manual editing of a nav menu file when adding new pages.
- Implement via a custom `[NavMenu]` attribute applied to page components, specifying:
  - `Title` (display text)
  - `Icon` (MudBlazor icon reference)
  - `Parent` (route string of parent page, null = top-level)
  - `Order` (sort order within its level)
- A `NavigationService` (singleton) scans the assembly at startup, builds a hierarchical tree of `NavItem` objects based on `Parent` relationships, and **caches** the result.
- `NavTreeItem.razor` recursively renders every `NavItem`, allowing any item to contain another level of children.
- A menu group must expand only when hovered and collapse when the pointer leaves, except that every ancestor of the currently selected route must remain expanded. Clicking a parent item navigates to its page rather than toggling the group, and items with children display a small down chevron.
- Hovered and selected navigation items must use the same highlight treatment.
- Sidebar CSS/styling should never need to change when new pages/sub-pages are added — all styling is driven by the shared navigation component and MudBlazor drawer/nav components.

### 3.3 Theming / Design Consistency

- Centralized **design token system** using CSS custom properties in `wwwroot/css/theme.css`, including (at minimum):
  - Brand colors: `--primary-theme`, `--secondary-theme`, `--accent-theme` (+ hover/light variants)
  - Status colors: `--success-theme`, `--warning-theme`, `--danger-theme`, `--info-theme`
  - Text colors: `--primary-text`, `--secondary-text`, `--disabled-text`, `--inverse-text`
  - Background colors: `--bg-body`, `--bg-surface`, `--bg-sidebar`
  - Border colors, spacing scale, border-radius scale, shadow scale, font family/sizes
- Use the SiS Master Data palette as the canonical color source:
  - `--primary-theme`: `#1a2848`
  - `--secondary-theme`: `#26375f`
  - `--accent-theme`: `#ff6b35`
  - `--accent-hover-theme`: `#cc4d18`
  - `--success-theme`: `#15803d`
  - `--warning-theme`: `#b45309`
  - `--danger-theme`: `#b91c1c`
  - `--info-theme`: `#26375f`
  - `--primary-text`: `#181c21`
  - `--secondary-text`: `#626978`
  - `--inverse-text`: `#ffffff`
  - `--bg-body`: `#f7f9ff`
  - `--bg-surface`: `#ffffff`
  - `--bg-surface-muted`: `#eef2f9`
  - `--bg-sidebar`: `#1a2848`
  - `--border-theme`: `rgba(26, 40, 72, 0.14)`
- Hover, light, and disabled variants not explicitly listed above must be derived from these canonical colors rather than introducing unrelated palette values.
- A parallel **MudBlazor theme mapping** (`AppTheme.cs`) using `MudTheme`/`PaletteLight`, with color values **kept in sync** with `theme.css` so both the raw CSS and MudBlazor components render consistently.
- Optional utility CSS classes (`.btn-primary-theme`, `.text-secondary`, `.card-surface`, etc.) for any custom (non-MudBlazor) markup, so no page ever hardcodes a hex color.
- Both files (`theme.css` and `AppTheme.cs`) should be portable — copyable into future projects as a starter theme kit.

### 3.4 Layout

- Standard app shell: left sidebar (drawer) + main content area. No top app bar is required.
- The top of the sidebar must include a dedicated branding area where the application name and icon/logo can be configured and displayed.
- The sidebar application name and icon/logo should be defined in one centralized location so future projects can replace them without editing the navigation component markup.
- The sidebar must display the authenticated user greeting and logout control when an authentication provider supplies a user.
- No fake or local login page is required. Microsoft Entra ID provides the sign-in experience.
- Desktop-only — no need for responsive/collapsible mobile behavior at this stage (may be added later).
- Sidebar should remain collapsed as a narrow icon rail by default, automatically expand when the pointer hovers over it, and collapse again when the pointer leaves.
- In the collapsed state, navigation icons must remain visible while text labels, the application name, and secondary user details are hidden.

### 3.5 Sample Pages

- At least one top-level page (`Home`) and one parent/child page pair (`Settings` → `Settings/Users`) to validate the auto-discovery sidebar logic works for both flat and nested routes.

### 3.6 Backend Architecture

- Use a pragmatic layered structure within the web project so the POC remains easy to run while backend concerns stay separated from Razor components.
- `Components` is the presentation layer and must not directly access persistence implementations, external SDKs, or infrastructure-specific authentication classes.
- `Application` contains use cases, service interfaces, DTOs, validation, and application-level results. UI components depend on these abstractions.
- `Domain` contains business entities, value objects, enums, and rules that do not depend on Blazor, MudBlazor, persistence, or external services.
- `Infrastructure` implements application interfaces for authentication, persistence, and external integrations. Microsoft Entra ID registration belongs here.
- `Configuration` contains strongly typed options for application branding, authentication, and external services. Configuration must be bound and validated at startup.
- `Middleware` contains centralized exception handling, request correlation, and other HTTP pipeline behavior when required.
- `Common` contains narrowly scoped shared primitives such as result and error types; it must not become a catch-all folder for unrelated helpers.
- Register services through focused dependency-injection extension methods (for example, `AddApplicationServices` and `AddInfrastructureServices`) so `Program.cs` remains a composition root rather than accumulating implementation details.
- Use structured logging for backend failures and show users safe, non-sensitive error messages. Secrets, tokens, connection strings, and personally identifiable information must not be written to logs.
- Add unit tests for navigation-tree construction, validation, and application services. Add integration tests when real authentication, persistence, or HTTP endpoints are introduced.

---

## 4. Non-Functional Requirements

- **Reusability**: Navigation system, theme files, and layout components must be easily copyable into future unrelated projects with minimal modification.
- **Maintainability**: Adding a new page/sub-page must require **only**:
  1. Creating the `.razor` file with `@page` route
  2. Adding `[Authorize]` (if protected)
  3. Adding `[NavMenu(...)]` attribute
  — no other file should need editing.
- **Performance**: Navigation tree should be built once (cached in singleton service), not recomputed per request.
- **Security readiness**: Fake auth is clearly isolated (single file/class) so it can be deleted/replaced without side effects when real SSO is introduced.
- **Separation of concerns**: Razor components coordinate presentation and user interaction; business rules and infrastructure access remain outside component code.
- **Configuration safety**: Environment-specific values come from configuration providers and strongly typed options; secrets are never committed to source control.
- **Testability**: Application behavior and navigation discovery can be tested without rendering the full Blazor UI or connecting to production services.

---

## 5. Microsoft Entra ID Configuration

Microsoft SSO requires:

1. Register app in Azure AD (Entra ID) — using a **personal/free Azure tenant** for POC purposes (not company tenant), via [azure.microsoft.com/free](https://azure.microsoft.com/free)
2. Obtain: Tenant ID, Client ID, Client Secret, Primary Domain (all from the App Registration Overview page in Entra ID)
3. Configure the `AzureAd` section with `Instance`, `TenantId`, and `ClientId`
4. Register the local and deployed `/signin-oidc` and `/signout-callback-oidc` redirect URIs
5. Store `ClientSecret` via `dotnet user-secrets` for local development and a secure credential store in production

---

## 6. Out of Scope (For Now)

- Mobile/responsive layout
- Dark mode toggle
- Role-based menu filtering (structure should support it later, but not required initially)
- Database/EF Core integration
- Deployment/hosting setup
- Multi-tenant or external user support

---

## 7. File/Folder Structure Reference

```
MyWebApp/
├── Application/
│   ├── Abstractions/
│   │   ├── Authentication/
│   │   │   └── ICurrentUserService.cs
│   │   └── Services/
│   ├── DTOs/
│   ├── Features/
│   │   └── ExampleFeature/
│   │       ├── Models/
│   │       ├── Services/
│   │       └── Validators/
│   └── DependencyInjection.cs
├── Common/
│   ├── Errors/
│   └── Results/
├── Configuration/
│   └── ApplicationOptions.cs
├── Navigation/
│   ├── NavMenuAttribute.cs
│   ├── NavItem.cs
│   └── NavigationService.cs
├── Domain/
│   ├── Entities/
│   ├── Enums/
│   ├── ValueObjects/
│   └── Rules/
├── Infrastructure/
│   ├── Persistence/
│   ├── ExternalServices/
│   └── DependencyInjection.cs
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs
├── Theming/
│   └── AppTheme.cs
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   └── NavTreeItem.razor
│   ├── Shared/
│   ├── Pages/
│   │   ├── Home/
│   │   │   └── Home.razor
│   │   ├── Error/
│   │   │   └── Error.razor
│   │   ├── NotFound/
│   │   │   └── NotFound.razor
│   │   ├── Counter/
│   │   │   └── Counter.razor
│   │   ├── Weather/
│   │   │   └── Weather.razor
│   │   └── Settings/
│   │       ├── Settings.razor
│   │       └── UserSetting/
│   │           └── UserSetting.razor
│   ├── App.razor
│   └── Routes.razor
├── wwwroot/
│   └── css/
│       ├── theme.css
│       └── utilities.css
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
└── MyWebApp.csproj

tests/
├── MyWebApp.UnitTests/
│   ├── Application/
│   └── Navigation/
└── MyWebApp.IntegrationTests/
    └── Infrastructure/
```

Folders should be created when they have a concrete responsibility; the reference structure defines where backend code belongs as features are added and does not require empty placeholder files.

---

## 8. Acceptance Criteria

- [ ] App runs locally with `dotnet run`, desktop browser only
- [ ] No fake authentication provider or fake login page is included
- [ ] Every Blazor page redirects anonymous users to Microsoft Entra ID for sign-in
- [ ] Sidebar shows "Home" and expandable "Settings" (with "Users" nested child) with zero manual sidebar code
- [ ] Menu groups expand only on hover, active branches remain expanded, parent items show a small down chevron, hover and selected highlights match, and navigation supports multiple nested submenu levels
- [ ] Sidebar displays a configurable application name and icon/logo in its branding area
- [ ] Sidebar expands on hover and automatically returns to its collapsed icon-only state when the pointer leaves
- [ ] Adding a brand-new page with `[NavMenu(...)]` immediately appears in sidebar on next app restart, no other code touched
- [ ] All colors/fonts in the UI trace back to `theme.css` variables — no hardcoded hex values in components
- [ ] MudBlazor components (Drawer, NavMenu, Buttons) render using the custom theme colors, not MudBlazor defaults
- [ ] Razor components do not directly depend on persistence implementations or external-service SDKs
- [ ] Backend services are registered through layer-specific dependency-injection extension methods
- [ ] Navigation-tree behavior and application services have automated unit-test coverage

---

*Save this document and paste it at the start of a new session to continue development with full context.*
