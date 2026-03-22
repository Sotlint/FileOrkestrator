namespace FileOrkestrator.Domain.Indexing;

/// <summary>
/// Задача индексации: локальный идентификатор и связь с внешним job Search Engine.
/// </summary>
public sealed class OrchestrationJob
{
    /// <summary>Идентификатор в оркестраторе.</summary>
    public Guid Id { get; init; }

    /// <summary>Связанный источник данных.</summary>
    public Guid SourceId { get; init; }

    /// <summary>Идентификатор задачи у Search Engine.</summary>
    public string ExternalJobId { get; init; } = string.Empty;

    /// <summary>Текущий статус.</summary>
    public OrchestrationJobStatus Status { get; init; }

    /// <summary>Время создания записи (UTC).</summary>
    public DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>Время последнего обновления (UTC).</summary>
    public DateTimeOffset? UpdatedAtUtc { get; init; }

    /// <summary>Текст последней ошибки.</summary>
    public string? LastError { get; init; }

    /// <summary>Ключ идемпотентности запуска.</summary>
    public string? IdempotencyKey { get; init; }

    /// <summary>Идентификатор трассировки.</summary>
    public string? CorrelationId { get; init; }
}
