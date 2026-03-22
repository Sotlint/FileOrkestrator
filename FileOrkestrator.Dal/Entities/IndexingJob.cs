using FileOrkestrator.Domain.Indexing;

namespace FileOrkestrator.Dal.Entities;

/// <summary>
/// Задача индексации на стороне оркестратора (связь с внешним job id Search Engine).
/// </summary>
public sealed class IndexingJob
{
    /// <summary>Локальный идентификатор задачи в БД оркестратора.</summary>
    public Guid Id { get; init; }

    /// <summary>Ссылка на <see cref="IndexingSource"/>.</summary>
    public Guid SourceId { get; init; }

    /// <summary>Идентификатор задачи у внешнего Search Engine (пусто до успешного <c>StartIndex</c>).</summary>
    public string ExternalJobId { get; private set; } = string.Empty;

    /// <summary>Текущий статус: строковое имя значения <see cref="OrchestrationJobStatus"/> (как в БД и API).</summary>
    public string Status { get; private set; } = string.Empty;

    /// <summary>Время создания записи (UTC).</summary>
    public DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>Время последнего изменения состояния задачи (UTC).</summary>
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    /// <summary>Текст последней ошибки (локальная или синхронизированная с внешним API).</summary>
    public string? LastError { get; private set; }

    /// <summary>Ключ идемпотентности: повторный запрос с тем же ключом и источником возвращает ту же задачу.</summary>
    public string? IdempotencyKey { get; private set; }

    /// <summary>Идентификатор для сквозной трассировки (логи, метрики).</summary>
    public string? CorrelationId { get; private set; }

    /// <summary>Навигация к источнику (EF Core).</summary>
    public IndexingSource? Source { get; private set; }

    /// <summary>Создаёт новую задачу в статусе <see cref="OrchestrationJobStatus.Pending"/>.</summary>
    public static IndexingJob CreatePending(Guid sourceId, string? idempotencyKey, string? correlationId)
    {
        return new IndexingJob
        {
            Id = Guid.NewGuid(),
            SourceId = sourceId,
            ExternalJobId = string.Empty,
            Status = nameof(OrchestrationJobStatus.Pending),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IdempotencyKey = idempotencyKey,
            CorrelationId = correlationId,
        };
    }

    /// <summary>Обновляет метку времени последнего изменения.</summary>
    private void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;

    /// <summary>Сохраняет идентификатор задачи у Search Engine после принятия запроса на индексацию.</summary>
    public void SetExternalJobId(string externalJobId)
    {
        ExternalJobId = externalJobId;
        Touch();
    }

    /// <summary>Устанавливает статус из уже сопоставленной строки (например после <c>GetIndexJob</c>).</summary>
    public void ApplyStoredStatus(string storedStatus)
    {
        Status = storedStatus;
        Touch();
    }

    /// <summary>Синхронизация после опроса внешнего API (статус + опционально текст ошибки).</summary>
    public void SyncFromRemotePoll(string storedStatus, string? lastError)
    {
        Status = storedStatus;
        Touch();
        if (!string.IsNullOrEmpty(lastError))
            LastError = lastError;
    }

    /// <summary>Переводит задачу в терминальный статус «отменена».</summary>
    public void MarkCancelled()
    {
        Status = nameof(OrchestrationJobStatus.Cancelled);
        Touch();
    }

    /// <summary>Переводит задачу в терминальный статус «ошибка» и сохраняет сообщение.</summary>
    public void MarkFailed(string error)
    {
        Status = nameof(OrchestrationJobStatus.Failed);
        LastError = error;
        Touch();
    }
}
