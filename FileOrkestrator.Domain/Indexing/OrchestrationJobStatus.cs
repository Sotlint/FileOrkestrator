namespace FileOrkestrator.Domain.Indexing;

/// <summary>
/// Статус задачи индексации в оркестраторе (хранится и синхронизируется с внешним Search Engine).
/// </summary>
public enum OrchestrationJobStatus
{
    /// <summary>Задача создана, ожидает или только что принята внешней системой.</summary>
    Pending = 0,

    /// <summary>Индексация выполняется.</summary>
    Running = 1,

    /// <summary>Успешное завершение.</summary>
    Succeeded = 2,

    /// <summary>Завершение с ошибкой.</summary>
    Failed = 3,

    /// <summary>Отменена пользователем или политикой.</summary>
    Cancelled = 4,

    /// <summary>Завершено с ошибками по части элементов.</summary>
    PartialSuccess = 5,
}
