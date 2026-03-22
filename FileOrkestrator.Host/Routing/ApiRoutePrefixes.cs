namespace FileOrkestrator.Host.Routing;

/// <summary>Базовый префикс HTTP API оркестратора (без версии). Версия <c>v1</c> задаётся в атрибуте <see cref="Microsoft.AspNetCore.Mvc.RouteAttribute"/> каждого контроллера.</summary>
public static class ApiRoutePrefixes
{
    public const string Orkestrator = "api/orkestrator";
}
