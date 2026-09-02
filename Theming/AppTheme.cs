using MudBlazor;

namespace MyWebApp.Theming;

public static class AppTheme
{
    public const string Primary = "#1a2848";
    public const string PrimarySoft = "#26375f";
    public const string Accent = "#ff6b35";
    public const string AccentDark = "#cc4d18";
    public const string Surface = "#f7f9ff";
    public const string SurfaceMuted = "#eef2f9";
    public const string Card = "#ffffff";
    public const string Text = "#181c21";
    public const string TextMuted = "#626978";
    public const string Success = "#15803d";
    public const string Warning = "#b45309";
    public const string Danger = "#b91c1c";

    public static MudTheme Theme { get; } = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = Primary,
            Secondary = PrimarySoft,
            Tertiary = Accent,
            Background = Surface,
            Surface = Card,
            DrawerBackground = Primary,
            DrawerText = Card,
            TextPrimary = Text,
            TextSecondary = TextMuted,
            Success = Success,
            Warning = Warning,
            Error = Danger,
            Info = PrimarySoft
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "12px"
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = ["Noto Sans Thai", "Noto Sans", "Segoe UI", "sans-serif"]
            }
        }
    };
}
