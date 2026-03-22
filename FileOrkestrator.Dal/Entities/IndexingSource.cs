namespace FileOrkestrator.Dal.Entities;

/// <summary>
/// Логический источник данных для индексации (каталог, share, bucket и т.д.).
/// </summary>
public sealed class IndexingSource
{
    /// <summary>Идентификатор источника в БД.</summary>
    public Guid Id { get; init; }

    /// <summary>Человекочитаемое имя (каталог, share, метка bucket и т.д.).</summary>
    public required string Name { get; init; }

    /// <summary>Время регистрации источника (UTC).</summary>
    public DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>Задачи индексации, привязанные к этому источнику.</summary>
    public ICollection<IndexingJob> Jobs { get; private set; } = new List<IndexingJob>();
}
