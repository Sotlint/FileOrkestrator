using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileOrkestrator.Dal;

public static class DalModule
{
    public const string PostgreSqlConnectionName = "PostgreSQL";

    /// <summary>
    /// Регистрирует <see cref="FileOrkestratorDbContext"/> с провайдером PostgreSQL (Npgsql).
    /// Ожидается строка подключения <c>ConnectionStrings:{PostgreSqlConnectionName}</c>.
    /// </summary>
    public static IServiceCollection AddDalModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(PostgreSqlConnectionName);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                $"Connection string '{PostgreSqlConnectionName}' is missing. Add ConnectionStrings:{PostgreSqlConnectionName} to configuration.");

        services.AddDbContext<FileOrkestratorDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(MigratorProject.AssemblyName)));

        return services;
    }
}
