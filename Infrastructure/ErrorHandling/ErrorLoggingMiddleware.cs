using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;

namespace MyWebApp.Infrastructure.ErrorHandling;

public sealed class ErrorLoggingMiddleware(
    RequestDelegate next,
    ILogger<ErrorLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next(context);

            // A re-executed error request is the same failure being rendered by the
            // centralized error page, so only log the original request.
            if (context.Response.StatusCode >= StatusCodes.Status400BadRequest &&
                context.Features.Get<IStatusCodeReExecuteFeature>() is null &&
                context.Features.Get<IExceptionHandlerFeature>() is null)
            {
                LogFailedRequest(context, stopwatch.ElapsedMilliseconds);
            }
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Unhandled exception while processing {HttpMethod} {RequestPath}. " +
                "TraceId: {TraceId}; ElapsedMs: {ElapsedMs}",
                context.Request.Method,
                context.Request.Path.Value,
                context.TraceIdentifier,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    private void LogFailedRequest(HttpContext context, long elapsedMilliseconds)
    {
        var statusCode = context.Response.StatusCode;
        var logLevel = statusCode >= StatusCodes.Status500InternalServerError
            ? LogLevel.Error
            : LogLevel.Warning;

        logger.Log(
            logLevel,
            "HTTP request failed with {StatusCode}: {HttpMethod} {RequestPath}. " +
            "TraceId: {TraceId}; ElapsedMs: {ElapsedMs}",
            statusCode,
            context.Request.Method,
            context.Request.Path.Value,
            context.TraceIdentifier,
            elapsedMilliseconds);
    }
}
