using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using MyWebApp.Configuration;
using MyWebApp.Infrastructure.Authentication;

namespace MyWebApp.UnitTests.Infrastructure;

public sealed class FakeAuthStateProviderTests
{
    [Fact]
    public async Task SignInAndSignOutUpdateClaimsPrincipal()
    {
        var options = Options.Create(new AuthenticationOptions
        {
            FakeUserName = "Test User",
            FakeUserEmail = "test.user@example.com",
            DefaultRole = "User"
        });
        var provider = new FakeAuthStateProvider(options, new DevelopmentEnvironment());

        await provider.SignInAsync("Admin");
        var signedIn = (await provider.GetAuthenticationStateAsync()).User;

        Assert.True(signedIn.Identity?.IsAuthenticated);
        Assert.Equal("Test User", signedIn.Identity?.Name);
        Assert.True(signedIn.IsInRole("Admin"));

        await provider.SignOutAsync();
        var signedOut = (await provider.GetAuthenticationStateAsync()).User;

        Assert.False(signedOut.Identity?.IsAuthenticated);
    }

    private sealed class DevelopmentEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "MyWebApp.UnitTests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
