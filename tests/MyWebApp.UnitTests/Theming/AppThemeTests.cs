using MyWebApp.Theming;

namespace MyWebApp.UnitTests.Theming;

public sealed class AppThemeTests
{
    [Fact]
    public void MudThemeColorsMatchCssTokens()
    {
        var projectDirectory = FindProjectDirectory();
        var css = File.ReadAllText(Path.Combine(projectDirectory, "wwwroot", "css", "theme.css"));

        AssertToken(css, "--primary-theme", AppTheme.Primary);
        AssertToken(css, "--primary-soft-theme", AppTheme.PrimarySoft);
        AssertToken(css, "--accent-theme", AppTheme.Accent);
        AssertToken(css, "--accent-hover-theme", AppTheme.AccentDark);
        AssertToken(css, "--bg-body", AppTheme.Surface);
        AssertToken(css, "--bg-surface-muted", AppTheme.SurfaceMuted);
        AssertToken(css, "--bg-surface", AppTheme.Card);
        AssertToken(css, "--primary-text", AppTheme.Text);
        AssertToken(css, "--secondary-text", AppTheme.TextMuted);
        AssertToken(css, "--success-theme", AppTheme.Success);
        AssertToken(css, "--warning-theme", AppTheme.Warning);
        AssertToken(css, "--danger-theme", AppTheme.Danger);
        AssertToken(css, "--chart-orange", AppTheme.ChartOrange);
        AssertToken(css, "--chart-blue", AppTheme.ChartBlue);
        AssertToken(css, "--chart-teal", AppTheme.ChartTeal);
        AssertToken(css, "--chart-gold", AppTheme.ChartGold);
        AssertToken(css, "--chart-remainder-theme", AppTheme.ChartRemainder);
    }

    private static void AssertToken(string css, string token, string value) =>
        Assert.Contains($"{token}: {value.ToLowerInvariant()};", css, StringComparison.OrdinalIgnoreCase);

    private static string FindProjectDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "MyWebApp.csproj")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the MyWebApp project directory.");
    }
}
