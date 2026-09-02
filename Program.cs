using MyWebApp.Application;
using MyWebApp.Components;
using MyWebApp.Endpoints;
using MyWebApp.Infrastructure;
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
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();
builder.Services.AddMudServices();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Environment);
builder.Services.AddSingleton(_ => new NavigationService(typeof(Program).Assembly));

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDataProtection()
        .SetApplicationName("MyWebApp.Development")
        .PersistKeysToFileSystem(
            new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".data-protection-keys")));
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapHealthEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

public partial class Program;
