using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace FileOrkestrator.Host.Authentication;

/// <summary>
/// Для запросов к <c>/api</c> проверяет заголовок с API-ключом, если ключ задан в конфигурации.
/// </summary>
public sealed class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IOptionsMonitor<ApiAuthenticationOptions> _options;

    public ApiKeyMiddleware(RequestDelegate next, IOptionsMonitor<ApiAuthenticationOptions> options)
    {
        _next = next;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var opt = _options.CurrentValue;
        if (string.IsNullOrWhiteSpace(opt.ApiKey))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        var headerName = string.IsNullOrWhiteSpace(opt.HeaderName) ? "X-API-Key" : opt.HeaderName.Trim();
        if (!context.Request.Headers.TryGetValue(headerName, out var provided) ||
            !SecureEquals(provided.ToString(), opt.ApiKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await _next(context).ConfigureAwait(false);
    }

    private static bool SecureEquals(string provided, string expected)
    {
        var a = Encoding.UTF8.GetBytes(provided);
        var b = Encoding.UTF8.GetBytes(expected);
        if (a.Length != b.Length)
            return false;
        return CryptographicOperations.FixedTimeEquals(a, b);
    }
}
