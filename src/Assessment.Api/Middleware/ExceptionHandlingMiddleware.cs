using System.Net;
using System.Text.Json;
using Assessment.Domain.Exceptions;

namespace Assessment.Api.Middleware;

/// <summary>
/// TASK 9: Global Exception Handling (5 Points)
/// This middleware is poorly implemented and incomplete:
/// - Does not conform to RFC 7807 ProblemDetails specification.
/// - Does not properly map specific domain exceptions (ValidationException, NotFoundException, BusinessException).
/// - Lacks trace ID correlation.
/// - In several controllers, repetitive try-catches are used instead of relying on this global middleware.
/// Candidates should refactor this to return standard ProblemDetails.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred during request execution.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // ANTI-PATTERN [Task 9]:
        // Incomplete error mapping. Everything defaults to generic 500 or arbitrary shape
        // rather than standard ProblemDetails (RFC 7807).
        var statusCode = exception switch
        {
            NotFoundException => (int)HttpStatusCode.NotFound,
            ValidationException => (int)HttpStatusCode.BadRequest,
            UnauthorizedException => (int)HttpStatusCode.Unauthorized,
            BusinessException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = statusCode;

        // Non-standard response format (Missing ProblemDetails spec)
        var response = new
        {
            error = exception.Message,
            statusCode = statusCode,
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}
