using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using MyWebApp.Application.Abstractions.Authentication;
using MyWebApp.Configuration;

namespace MyWebApp.Infrastructure.Authentication;

public sealed class FakeAuthStateProvider(
    IOptions<AuthenticationOptions> options,
    IWebHostEnvironment environment)
    : AuthenticationStateProvider, ICurrentUserService
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());
    private readonly AuthenticationOptions _options = options.Value;
    private ClaimsPrincipal _currentUser = Anonymous;

    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        Task.FromResult(new AuthenticationState(_currentUser));

    public Task<ClaimsPrincipal> GetUserAsync() => Task.FromResult(_currentUser);

    public Task SignInAsync(string? role = null)
    {
        EnsureDevelopmentEnvironment();

        var selectedRole = string.IsNullOrWhiteSpace(role) ? _options.DefaultRole : role;
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, _options.FakeUserName),
            new Claim(ClaimTypes.Email, _options.FakeUserEmail),
            new Claim(ClaimTypes.Role, selectedRole)
        ], "FakeAuthentication");

        _currentUser = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return Task.CompletedTask;
    }

    public Task SignOutAsync()
    {
        EnsureDevelopmentEnvironment();
        _currentUser = Anonymous;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return Task.CompletedTask;
    }

    private void EnsureDevelopmentEnvironment()
    {
        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException("Fake authentication is available only in Development.");
        }
    }
}
