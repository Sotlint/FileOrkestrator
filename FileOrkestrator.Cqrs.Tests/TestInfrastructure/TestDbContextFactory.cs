using FileOrkestrator.Dal;
using Microsoft.EntityFrameworkCore;

namespace FileOrkestrator.Cqrs.Tests.TestInfrastructure;

/// <summary>
/// Фабрика <see cref="FileOrkestratorDbContext"/> на провайдере InMemory для изолированных тестов без PostgreSQL.
/// </summary>
internal static class TestDbContextFactory
{
    /// <summary>Создаёт контекст с уникальной именованной in-memory БД на каждый вызов (параллельные тесты не делят состояние).</summary>
    public static FileOrkestratorDbContext CreateInMemory()
    {
        var options = new DbContextOptionsBuilder<FileOrkestratorDbContext>()
            .UseInMemoryDatabase($"cqrs-tests-{Guid.NewGuid():N}")
            .Options;
        return new FileOrkestratorDbContext(options);
    }
}
