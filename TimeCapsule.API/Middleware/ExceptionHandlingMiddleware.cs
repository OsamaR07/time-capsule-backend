using System.Net;
using FluentValidation;
using TimeCapsule.Domain.Exceptions;

namespace TimeCapsule.API.Middleware;

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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (status, code, message, errors) = exception switch
        {
            AppException e => (e.StatusCode, e.ErrorCode, e.Message, (object?)null),
            UnauthorizedAccessException e => (401, "unauthorized", e.Message, (object?)null),
            ValidationException e => (422, "validation_error", "Validation failed.", (object)e.Errors),
            _ => (500, "internal_error", "An unexpected error occurred.", (object?)null)
        };

        if (status == 500)
        {
            _logger.LogError(exception, "Unhandled exception occurred.");
        }

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            error = code,
            message,
            errors
        });
    }
}
