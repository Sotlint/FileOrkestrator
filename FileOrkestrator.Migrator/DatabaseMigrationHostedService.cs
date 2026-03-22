using FileOrkestrator.Dal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FileOrkestrator.Migrator;

/// <summary>
/// При старте приложения применяет к БД только ожидающие миграции (в таблице <c>__EFMigrationsHistory</c> уже есть записанные; повторно они не выполняются).
/// </summary>
public sealed class DatabaseMigrationHostedService(IServiceScopeFactory scopeFactory, ILogger<DatabaseMigrationHostedService> logger) : IHostedService
{
    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<FileOrkestratorDbContext>();

        var pending = (await db.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).ToList();
        if (pending.Count == 0)
        {
            logger.LogInformation("Database: no pending EF Core migrations; schema is up to date.");
            return;
        }

        logger.LogInformation(
            "Database: applying {Count} pending migration(s): {Migrations}",
            pending.Count,
            string.Join(", ", pending));

        await db.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Database: migrations applied successfully.");
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
