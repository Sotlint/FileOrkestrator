using FileOrkestrator.Dal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FileOrkestrator.Migrator.Design;

/// <summary>
/// Design-time: <c>dotnet ef</c> из каталога Migrator. Строка — <c>FILEORKESTRATOR_CONNECTION_STRING</c> или локальный Postgres.
/// </summary>
public sealed class FileOrkestratorDbContextFactory : IDesignTimeDbContextFactory<FileOrkestratorDbContext>
{
    /// <inheritdoc />
    public FileOrkestratorDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("FILEORKESTRATOR_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=fileorkestrator;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<FileOrkestratorDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsAssembly(MigratorProject.AssemblyName));

        return new FileOrkestratorDbContext(optionsBuilder.Options);
    }
}
