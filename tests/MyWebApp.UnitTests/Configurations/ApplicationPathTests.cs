using Microsoft.Extensions.Configuration;
using MyWebApp.Configurations;

namespace MyWebApp.UnitTests.Configurations;

public sealed class ApplicationPathTests
{
    [Theory]
    [InlineData(null, "", "/")]
    [InlineData("/", "", "/")]
    [InlineData("portal", "/portal", "/portal/")]
    [InlineData(" /portal/ ", "/portal", "/portal/")]
    public void BasePath_IsNormalized(string? configured, string expectedPath, string expectedHref)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Application:BasePath"] = configured
            })
            .Build();

        Assert.Equal(expectedPath, ApplicationPath.GetBasePath(configuration).Value ?? "");
        Assert.Equal(expectedHref, ApplicationPath.GetBaseHref(configuration));
    }

    [Theory]
    [InlineData("/portal?mode=test")]
    [InlineData("/portal#section")]
    public void BasePath_RejectsQueryStringsAndFragments(string configured)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Application:BasePath"] = configured
            })
            .Build();

        Assert.Throws<InvalidOperationException>(() => ApplicationPath.GetBasePath(configuration));
    }
}
