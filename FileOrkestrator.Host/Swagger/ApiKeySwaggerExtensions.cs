using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FileOrkestrator.Host.Swagger;

internal static class ApiKeySwaggerExtensions
{
    public static void AddApiKeySecurity(this SwaggerGenOptions options)
    {
        const string id = "ApiKey";
        options.AddSecurityDefinition(id, new OpenApiSecurityScheme
        {
            Description = "Ключ доступа к API (заголовок X-API-Key). Нужен только если в конфигурации задан ApiAuthentication:ApiKey.",
            Name = "X-API-Key",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = id },
                },
                Array.Empty<string>()
            },
        });
    }
}
