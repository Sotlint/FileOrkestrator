namespace FileOrkestrator.Dto.Indexing;

/// <summary>Результат запуска индексации: локальный и внешний идентификаторы и признак идемпотентного повтора.</summary>
public sealed class StartIndexingDto
{
    /// <summary>Идентификатор задачи в оркестраторе.</summary>
    public Guid JobId { get; set; }

    /// <summary>Идентификатор задачи у Search Engine (пусто, если старт ещё не подтверждён внешней системой).</summary>
    public string ExternalJobId { get; set; } = string.Empty;

    /// <summary>Текущий строковый статус (как в БД).</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Время принятия запроса Search Engine (UTC).</summary>
    public DateTimeOffset AcceptedAtUtc { get; set; }

    /// <summary>True, если запрос с тем же ключом идемпотентности уже обрабатывался.</summary>
    public bool IsDuplicate { get; set; }
}
