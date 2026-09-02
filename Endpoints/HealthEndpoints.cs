using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MyWebApp.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/api/health", new HealthCheckOptions
        {
            ResponseWriter = WriteResponseAsync
        })
        .AllowAnonymous()
        .WithName("GetHealth")
        .WithTags("Health");

        return endpoints;
    }

    private static Task WriteResponseAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new HealthResponse(
            report.Status.ToString(),
            DateTimeOffset.UtcNow,
            report.TotalDuration.TotalMilliseconds,
            report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new HealthCheckResult(
                    entry.Value.Status.ToString(),
                    entry.Value.Description,
                    entry.Value.Duration.TotalMilliseconds)));

        return context.Response.WriteAsJsonAsync(response);
    }

    private sealed record HealthResponse(
        string Status,
        DateTimeOffset TimestampUtc,
        double DurationMilliseconds,
        IReadOnlyDictionary<string, HealthCheckResult> Checks);

    private sealed record HealthCheckResult(
        string Status,
        string? Description,
        double DurationMilliseconds);
}
