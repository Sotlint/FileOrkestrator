namespace FileOrkestrator.Dto.Indexing;

/// <summary>Тело запроса на запуск индексации источника.</summary>
public sealed class StartIndexingInput
{
    /// <summary>Конкретные пути; пусто — полная переиндексация источника.</summary>
    public IReadOnlyList<string>? FilePaths { get; set; }

    /// <summary>Ключ идемпотентности: повтор с тем же значением возвращает ту же задачу.</summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>Идентификатор для трассировки между сервисами.</summary>
    public string? CorrelationId { get; set; }
}
