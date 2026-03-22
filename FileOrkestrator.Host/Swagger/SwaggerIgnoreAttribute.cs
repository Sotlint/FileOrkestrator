using Microsoft.AspNetCore.Mvc;

namespace FileOrkestrator.Host.Swagger;

/// <summary>
/// Исключает контроллер или действие из OpenAPI (Swagger). Обертка над <see cref="ApiExplorerSettingsAttribute.IgnoreApi"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SwaggerIgnoreAttribute : ApiExplorerSettingsAttribute
{
    public SwaggerIgnoreAttribute()
    {
        IgnoreApi = true;
    }
}
