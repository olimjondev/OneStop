using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using OneStop.Application.Exceptions;
using OneStop.Domain.Exceptions;
using OneStop.Presentation.Api.Contracts;

namespace OneStop.Presentation.Exceptions;

/// <summary>
/// Global exception handler using the modern IExceptionHandler approach.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, response) = MapException(exception);

        if (statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Request failed with {StatusCode}: {Message}", statusCode, exception.Message);
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }

    private static (int StatusCode, ErrorResponse Response) MapException(Exception exception)
    {
        return exception switch
        {
            // Handle JSON deserialization errors (missing required properties, invalid format, etc.)
            BadHttpRequestException badRequestEx => MapBadHttpRequestException(badRequestEx),

            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse
                {
                    Type = "ValidationError",
                    Message = "One or more validation errors occurred.",
                    Errors = validationEx.Errors
                }),

            NotFoundException notFoundEx => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse
                {
                    Type = "NotFoundError",
                    Message = notFoundEx.Message
                }),

            DomainException domainEx => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse
                {
                    Type = "DomainError",
                    Message = domainEx.Message
                }),

            ArgumentException argEx => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse
                {
                    Type = "ArgumentError",
                    Message = argEx.Message
                }),

            _ => (
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Type = "InternalError",
                    Message = "An unexpected error occurred. Please try again later."
                })
        };
    }

    private static (int StatusCode, ErrorResponse Response) MapBadHttpRequestException(BadHttpRequestException exception)
    {
        // Extract meaningful error message from the inner exception if available
        var errorMessage = exception.InnerException switch
        {
            JsonException jsonEx => ExtractJsonErrorMessage(jsonEx),
            _ => exception.Message
        };

        return (
            StatusCodes.Status400BadRequest,
            new ErrorResponse
            {
                Type = "BadRequestError",
                Message = errorMessage
            });
    }

    private static string ExtractJsonErrorMessage(JsonException jsonException)
    {
        // The JsonException message often contains useful details about what went wrong
        var message = jsonException.Message;
        
        // Clean up the message to be more user-friendly
        if (message.Contains("missing required properties"))
        {
            // Extract the property names from messages like:
            // "JSON deserialization for type '...' was missing required properties including: 'ProductId'."
            var startIndex = message.IndexOf("including:", StringComparison.OrdinalIgnoreCase);
            if (startIndex >= 0)
            {
                var propertiesPart = message[(startIndex + 10)..].Trim().TrimEnd('.');
                return $"Missing required field(s): {propertiesPart}";
            }
        }
        
        if (message.Contains("could not be converted"))
        {
            return "Invalid value type in request. Please check the data types of your fields.";
        }

        // Return a cleaned-up version of the original message
        return $"Invalid request format: {message}";
    }
}
