namespace MyWebApp.Configurations;

public static class ApplicationPath
{
    public static PathString GetBasePath(IConfiguration configuration)
    {
        var configuredPath = configuration[$"{ApplicationOptions.SectionName}:BasePath"]?.Trim();

        if (string.IsNullOrEmpty(configuredPath) || configuredPath == "/")
        {
            return PathString.Empty;
        }

        if (configuredPath.Contains('?', StringComparison.Ordinal) ||
            configuredPath.Contains('#', StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"{ApplicationOptions.SectionName}:BasePath cannot contain a query string or fragment.");
        }

        if (!configuredPath.StartsWith('/'))
        {
            configuredPath = $"/{configuredPath}";
        }

        return new PathString(configuredPath.TrimEnd('/'));
    }

    public static string GetBaseHref(IConfiguration configuration)
    {
        var basePath = GetBasePath(configuration);
        return basePath.HasValue ? $"{basePath.Value}/" : "/";
    }

    public static string ToBaseRelativeUrl(string path) => path.TrimStart('/');
}
