using FileOrkestrator.Dal.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileOrkestrator.Dal;

/// <summary>
/// Контекст EF Core: источники данных и задачи индексации. Миграции лежат в сборке <c>FileOrkestrator.Migrator</c>.
/// </summary>
public sealed class FileOrkestratorDbContext : DbContext
{
    /// <summary>Создаёт контекст с опциями, зарегистрированными в DI.</summary>
    public FileOrkestratorDbContext(DbContextOptions<FileOrkestratorDbContext> options)
        : base(options)
    {
    }

    /// <summary>Зарегистрированные логические источники для индексации.</summary>
    public DbSet<IndexingSource> IndexingSources => Set<IndexingSource>();

    /// <summary>Задачи индексации и связь с внешним Search Engine.</summary>
    public DbSet<IndexingJob> IndexingJobs => Set<IndexingJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Таблица источников: уникальность имени на уровне индекса (бизнес-правило можно усилить отдельно).
        modelBuilder.Entity<IndexingSource>(entity =>
        {
            entity.ToTable("indexing_sources");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(512).IsRequired();
            entity.HasIndex(e => e.Name);
        });

        // Задачи: каскадное удаление при удалении источника; частичный уникальный индекс по идемпотентности (PostgreSQL).
        modelBuilder.Entity<IndexingJob>(entity =>
        {
            entity.ToTable("indexing_jobs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExternalJobId).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(64).IsRequired();
            entity.Property(e => e.LastError).HasMaxLength(4000);
            entity.Property(e => e.IdempotencyKey).HasMaxLength(256);
            entity.Property(e => e.CorrelationId).HasMaxLength(256);
            entity.HasIndex(e => e.ExternalJobId);
            entity.HasIndex(e => new { e.SourceId, e.CreatedAtUtc });
            entity.HasIndex(e => new { e.SourceId, e.IdempotencyKey })
                .IsUnique()
                .HasFilter("\"IdempotencyKey\" IS NOT NULL");

            entity
                .HasOne(e => e.Source)
                .WithMany(s => s.Jobs)
                .HasForeignKey(e => e.SourceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
