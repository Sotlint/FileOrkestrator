using FileOrkestrator.Cqrs.Sources;
using Microsoft.Extensions.DependencyInjection;

namespace FileOrkestrator.Cqrs;

/// <summary>Регистрация MediatR и обработчиков из сборки CQRS.</summary>
public static class CqrsModule
{
    /// <summary>Добавляет MediatR и сканирует текущую сборку на обработчики команд и запросов.</summary>
    public static IServiceCollection AddCqrs(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(RegisterSourceCommandHandler).Assembly));
        return services;
    }
}
