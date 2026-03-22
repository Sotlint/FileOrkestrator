using FileOrkestrator.Abstractions.Errors;

namespace FileOrkestrator.Host.Errors;

/// <summary>Соответствие доменного <see cref="ErrorCode"/> HTTP-статусу ответа.</summary>
internal static class ErrorCodeMapper
{
    /// <summary>Возвращает код HTTP для Problem Details.</summary>
    public static int ToStatusCode(ErrorCode code) => code switch
    {
        ErrorCode.ValidationFailed or ErrorCode.BadRequest => StatusCodes.Status400BadRequest,
        ErrorCode.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorCode.Forbidden => StatusCodes.Status403Forbidden,
        ErrorCode.NotFound or ErrorCode.IndexJobNotFound or ErrorCode.SourceNotFound => StatusCodes.Status404NotFound,
        ErrorCode.Conflict => StatusCodes.Status409Conflict,
        ErrorCode.ExternalSearchEngineTimeout => StatusCodes.Status504GatewayTimeout,
        ErrorCode.ExternalSearchEngineUnavailable => StatusCodes.Status503ServiceUnavailable,
        ErrorCode.ExternalSearchEngineError => StatusCodes.Status502BadGateway,
        ErrorCode.InternalError or ErrorCode.None => StatusCodes.Status500InternalServerError,
        _ => StatusCodes.Status500InternalServerError,
    };
}
