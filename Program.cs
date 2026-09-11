using MyWebApp.Application;
using MyWebApp.Components;
using MyWebApp.Configurations;
using MyWebApp.Endpoints;
using MyWebApp.Infrastructure;
using MyWebApp.Infrastructure.ErrorHandling;
using MyWebApp.Navigation;
using MudBlazor.Services;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHealthChecks();
builder.Services.AddMudServices();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddSingleton(_ => new NavigationService(typeof(Program).Assembly));
builder.Services.AddScoped<NavigationAuthorizationService>();
builder.Services.AddScoped<NavigationMerger>();

var applicationName = builder.Configuration[
    $"{ApplicationOptions.SectionName}:{nameof(ApplicationOptions.Name)}"]
    ?? throw new InvalidOperationException("Application name is required.");
var dataProtection = builder.Services.AddDataProtection()
    .SetApplicationName(applicationName);
var dataProtectionKeysPath = builder.Configuration["DataProtection:KeysPath"];

if (string.IsNullOrWhiteSpace(dataProtectionKeysPath) &&
    builder.Environment.IsDevelopment())
{
    dataProtectionKeysPath = Path.Combine(
        builder.Environment.ContentRootPath,
        ".data-protection-keys");
}

if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    dataProtection.PersistKeysToFileSystem(
        new DirectoryInfo(dataProtectionKeysPath));
}

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler("/error/500", createScopeForErrors: true);

if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute(
    "/error/{0}",
    createScopeForStatusCodePages: true);
app.UseMiddleware<ErrorLoggingMiddleware>();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapHealthEndpoints();
app.MapRazorPages();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .RequireAuthorization();

app.Run();

public partial class Program;
