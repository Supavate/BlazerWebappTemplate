namespace MyWebApp.Configuration;

public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    public string FakeUserName { get; set; } = "Demo User";

    public string FakeUserEmail { get; set; } = "demo@example.com";

    public string DefaultRole { get; set; } = "User";
}
