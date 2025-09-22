using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MF.Express.Bot.Api.Middleware;

/// <summary>
/// Обработчик исключений для Express Bot API
/// </summary>
public class ExpressBotExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ExpressBotExceptionHandler> _logger;

    public ExpressBotExceptionHandler(ILogger<ExpressBotExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Необработанное исключение в Express Bot API");

        var problemDetails = CreateProblemDetails(httpContext, exception);
        
        httpContext.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            ArgumentException => (HttpStatusCode.BadRequest, "Invalid argument", exception.Message),
            InvalidOperationException => (HttpStatusCode.BadRequest, "Invalid operation", exception.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized", "Access denied"),
            TimeoutException => (HttpStatusCode.RequestTimeout, "Request timeout", "Operation timed out"),
            HttpRequestException httpEx => (HttpStatusCode.BadGateway, "External service error", httpEx.Message),
            _ => (HttpStatusCode.InternalServerError, "Internal server error", "An unexpected error occurred")
        };

        return new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{(int)statusCode}"
        };
    }
}

