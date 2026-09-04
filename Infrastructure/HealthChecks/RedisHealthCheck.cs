using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace MyWebApp.Infrastructure.HealthChecks;

public sealed class RedisHealthCheck(
    RedisCacheOptions redisCacheOptions,
    ILogger<RedisHealthCheck> logger) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var configuration = redisCacheOptions.Configuration;
            if (string.IsNullOrWhiteSpace(configuration))
            {
                return HealthCheckResult.Unhealthy("Redis connection string is not configured.");
            }

            var connection = await ConnectionMultiplexer.ConnectAsync(
                configuration,
                TextWriter.Null);

            try
            {
                var database = connection.GetDatabase();
                var server = connection
                    .GetServers()
                    .FirstOrDefault();

                var started = DateTime.UtcNow;

                if (server is not null)
                {
                    await server.PingAsync();
                }
                else
                {
                    await database.PingAsync();
                }

                var duration = DateTime.UtcNow - started;

                return HealthCheckResult.Healthy(
                    "Redis is reachable.",
                    new Dictionary<string, object>
                    {
                        ["latencyMilliseconds"] = Math.Round(duration.TotalMilliseconds, 2)
                    });
            }
            finally
            {
                await connection.CloseAsync();
                connection.Dispose();
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Redis health check failed.");
            return HealthCheckResult.Unhealthy("Redis is not reachable.", exception);
        }
    }
}