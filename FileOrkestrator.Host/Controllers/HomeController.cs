using FileOrkestrator.Host.Swagger;
using Microsoft.AspNetCore.Mvc;

namespace FileOrkestrator.Host.Controllers;

/// <summary>Корень сайта: в Development перенаправляет на Swagger UI.</summary>
[SwaggerIgnore]
public sealed class HomeController : ControllerBase
{
    /// <summary>GET / — редирект на документацию OpenAPI (только Development); иначе краткий ответ без UI.</summary>
    [HttpGet("/")]
    public IActionResult Index([FromServices] IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
            return Redirect("/swagger");

        return Ok(new
        {
            service = "FileOrkestrator",
            api = "api/orkestrator/v1",
        });
    }
}
