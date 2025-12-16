using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace OneStop.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs request handling.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public partial class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        LogRequestStarted(_logger, requestName);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var response = await next();
            
            stopwatch.Stop();
            LogRequestCompleted(_logger, requestName, stopwatch.ElapsedMilliseconds);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogRequestFailed(_logger, requestName, stopwatch.ElapsedMilliseconds, ex);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Handling {RequestName}")]
    private static partial void LogRequestStarted(ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {RequestName} in {ElapsedMs}ms")]
    private static partial void LogRequestCompleted(ILogger logger, string requestName, long elapsedMs);

    [LoggerMessage(Level = LogLevel.Error, Message = "Request {RequestName} failed after {ElapsedMs}ms")]
    private static partial void LogRequestFailed(ILogger logger, string requestName, long elapsedMs, Exception ex);
}
