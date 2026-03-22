using FileOrkestrator.Cqrs;
using FileOrkestrator.Dal;
using FileOrkestrator.Host.Authentication;
using FileOrkestrator.Host.Configuration;
using FileOrkestrator.Host.Errors;
using FileOrkestrator.Host.Swagger;
using FileOrkestrator.Integrate.SearchEngine;
using FileOrkestrator.Migrator;
using Microsoft.OpenApi.Models;

// Конфигурация хоста: API, Swagger, глобальные ошибки, БД, миграции, CQRS, интеграция с Search Engine.
var builder = WebApplication.CreateBuilder(args);
// User secrets доступны при наличии UserSecretsId в csproj; appsettings*.json идут после и переопределяют секреты.
ConfigurationSourceOrdering.PrioritizeJsonFilesOverUserSecrets(builder.Configuration);

builder.Services.Configure<ApiAuthenticationOptions>(
    builder.Configuration.GetSection(ApiAuthenticationOptions.SectionName));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FileOrkestrator API",
        Version = "v1",
        Description = "Оркестратор индексации и поиска во внешнем Search Engine.",
    });
    options.AddApiKeySecurity();
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddDalModule(builder.Configuration);
builder.Services.AddDatabaseMigrator();
builder.Services.AddCqrs();
builder.Services.AddSearchEngineIntegration(builder.Configuration);
var app = builder.Build();
app.UseExceptionHandler();

app.UseMiddleware<ApiKeyMiddleware>();

// OpenAPI UI только в Development; в Production документация по умолчанию отключена.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "FileOrkestrator v1");
    });
}

app.MapControllers();
app.Run();
