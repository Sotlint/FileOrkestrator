using System.Net;
using FileOrkestrator.Abstractions.Errors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MvcProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace FileOrkestrator.Host.Errors;

/// <summary>
/// Единая точка: доменные исключения, Refit и прочие ошибки → Problem Details + код <see cref="ErrorCode"/>.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IHostEnvironment _environment;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IHostEnvironment environment, ILogger<GlobalExceptionHandler> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // Отмена запроса клиентом — отдаём стандартный конвейер ASP.NET Core, не подменяем ответ.
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
            return false;

        var (statusCode, errorCode, title, detail) = Map(exception);

        _logger.LogError(exception, "Unhandled exception mapped to {ErrorCode}, HTTP {Status}", errorCode, statusCode);

        if (httpContext.Response.HasStarted)
            return false;

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var problem = new MvcProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path.Value,
            Type = statusCode.ToString(),
        };

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        problem.Extensions["code"] = errorCode.ToString();

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken: cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <summary>Сопоставляет исключение с HTTP-статусом и полями Problem Details.</summary>
    private (int StatusCode, ErrorCode ErrorCode, string Title, string Detail) Map(Exception exception)
    {
        switch (exception)
        {
            case OrchestratorException ox:
                return (
                    ErrorCodeMapper.ToStatusCode(ox.Code),
                    ox.Code,
                    TitleFor(ox.Code),
                    ox.Message);

            case Refit.ApiException api:
                return MapRefit(api);

            case OperationCanceledException:
                return (
                    StatusCodes.Status504GatewayTimeout,
                    ErrorCode.ExternalSearchEngineTimeout,
                    "Gateway timeout",
                    "The operation was canceled or timed out.");

            default:
                var detail = _environment.IsDevelopment()
                    ? exception.ToString()
                    : "An unexpected error occurred.";
                return (
                    StatusCodes.Status500InternalServerError,
                    ErrorCode.InternalError,
                    "Internal server error",
                    detail);
        }
    }

    /// <summary>Ошибки HTTP от Refit при вызове Search Engine.</summary>
    private static (int, ErrorCode, string, string) MapRefit(Refit.ApiException api)
    {
        var status = (int)api.StatusCode;
        var code = status switch
        {
            (int)HttpStatusCode.RequestTimeout => ErrorCode.ExternalSearchEngineTimeout,
            (int)HttpStatusCode.ServiceUnavailable => ErrorCode.ExternalSearchEngineUnavailable,
            _ => ErrorCode.ExternalSearchEngineError,
        };

        var httpStatus = status switch
        {
            (int)HttpStatusCode.RequestTimeout => StatusCodes.Status504GatewayTimeout,
            (int)HttpStatusCode.ServiceUnavailable => StatusCodes.Status503ServiceUnavailable,
            (int)HttpStatusCode.NotFound => StatusCodes.Status502BadGateway,
            _ => StatusCodes.Status502BadGateway,
        };

        var detail = string.IsNullOrWhiteSpace(api.Content)
            ? $"Search engine HTTP {status}"
            : api.Content!;

        return (
            httpStatus,
            code,
            "Search engine request failed",
            detail);
    }

    /// <summary>Краткий заголовок Problem Details по коду ошибки.</summary>
    private static string TitleFor(ErrorCode code) => code switch
    {
        ErrorCode.ValidationFailed => "Validation failed",
        ErrorCode.BadRequest => "Bad request",
        ErrorCode.NotFound or ErrorCode.IndexJobNotFound or ErrorCode.SourceNotFound => "Not found",
        ErrorCode.Conflict => "Conflict",
        ErrorCode.Unauthorized => "Unauthorized",
        ErrorCode.Forbidden => "Forbidden",
        _ => "Error",
    };
}
