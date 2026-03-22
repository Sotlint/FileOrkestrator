namespace FileOrkestrator.Domain.Sources;

/// <summary>
/// Зарегистрированный источник данных для индексации.
/// </summary>
public sealed class DataSource
{
    /// <summary>Идентификатор источника.</summary>
    public Guid Id { get; init; }

    /// <summary>Имя источника.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Время регистрации (UTC).</summary>
    public DateTimeOffset CreatedAtUtc { get; init; }
}
