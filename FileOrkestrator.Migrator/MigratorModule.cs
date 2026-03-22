using Microsoft.Extensions.DependencyInjection;

namespace FileOrkestrator.Migrator;

public static class MigratorModule
{
    /// <summary>
    /// Регистрирует фоновый запуск применения миграций EF при старте хоста (после регистрации <see cref="FileOrkestrator.Dal.DalModule"/>).
    /// </summary>
    public static IServiceCollection AddDatabaseMigrator(this IServiceCollection services)
    {
        services.AddHostedService<DatabaseMigrationHostedService>();
        return services;
    }
}
