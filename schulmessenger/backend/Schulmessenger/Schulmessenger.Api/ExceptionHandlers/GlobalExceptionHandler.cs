using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Schulmessenger.Api.ExceptionHandlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, $"Unbehandelter Fehler: {exception.Message}");

        var (statusCode, title) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Ungültige Eingabe."),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Aktion war nicht möglich."),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Ungültige Anfrage."),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Zugriff verweigert."),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Nicht gefunden."),
            _ => (StatusCodes.Status500InternalServerError, "Interner Serverfehler.")
        };

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode == StatusCodes.Status500InternalServerError
                ? "Ein unerwarteter Serverfehler ist aufgetreten."
                : exception.Message,
            Instance = httpContext.Request.Path
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}