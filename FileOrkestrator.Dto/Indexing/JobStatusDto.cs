namespace FileOrkestrator.Dto.Indexing;

/// <summary>Снимок состояния задачи индексации для клиента API.</summary>
public sealed class JobStatusDto
{
    /// <summary>Идентификатор задачи в оркестраторе.</summary>
    public Guid JobId { get; set; }

    /// <summary>Источник данных, к которому относится задача.</summary>
    public Guid SourceId { get; set; }

    /// <summary>Идентификатор задачи у Search Engine.</summary>
    public string ExternalJobId { get; set; } = string.Empty;

    /// <summary>Строковый статус (согласован с доменным перечислением).</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Доля выполнения 0…1, если известна.</summary>
    public float? Progress { get; set; }

    /// <summary>Число успешно проиндексированных элементов.</summary>
    public int IndexedCount { get; set; }

    /// <summary>Число ошибок при индексации.</summary>
    public int FailedCount { get; set; }

    /// <summary>Последнее сообщение об ошибке.</summary>
    public string? LastError { get; set; }

    /// <summary>Время начала выполнения (UTC), если известно.</summary>
    public DateTimeOffset? StartedAtUtc { get; set; }

    /// <summary>Время завершения (UTC), если известно.</summary>
    public DateTimeOffset? CompletedAtUtc { get; set; }

    /// <summary>True, если завершено с частичным успехом.</summary>
    public bool PartialSuccess { get; set; } = false;
}
