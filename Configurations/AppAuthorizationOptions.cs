namespace MyWebApp.Configurations;

public sealed class AppAuthorizationOptions
{
    public const string SectionName = "Authorization";
    public Dictionary<string, PolicyOptions> Policies { get; init; } = [];
}

public sealed class PolicyOptions
{
    public string[] Roles { get; init; } = [];
}