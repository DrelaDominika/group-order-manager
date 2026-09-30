using GroupOrderManager.Application.Common.Exceptions;
using GroupOrderManager.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GroupOrderManager.Api;

/// <summary>
/// Turns exceptions into consistent HTTP error responses (RFC 9457 ProblemDetails).
/// Expected failures get a meaningful status code; anything else becomes a 500 without internal details.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            BadHttpRequestException badRequest => (badRequest.StatusCode, "Bad request"),   // e.g. malformed JSON
            NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            DomainException => (StatusCodes.Status409Conflict, "Business rule violated"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception");

        httpContext.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                // Only show messages we wrote ourselves; never leak internals on a 500.
                Detail = statusCode == StatusCodes.Status500InternalServerError ? null : exception.Message
            }
        });
    }
}
