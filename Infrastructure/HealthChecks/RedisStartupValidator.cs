using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace MyWebApp.Infrastructure.HealthChecks;

public sealed class RedisStartupValidator(
    IOptions<RedisCacheOptions> redisCacheOptions,
    ILogger<RedisStartupValidator> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var configuration = redisCacheOptions.Value.Configuration;
            if (string.IsNullOrWhiteSpace(configuration))
            {
                logger.LogWarning("Redis connection string is not configured.");
                return;
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

                if (server is not null)
                {
                    await server.PingAsync();
                }
                else
                {
                    await database.PingAsync();
                }

                logger.LogInformation("Redis connection verified at startup.");
            }
            finally
            {
                await connection.CloseAsync();
                connection.Dispose();
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Redis is not reachable at startup. Authentication token caching will fail until Redis becomes available.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

}