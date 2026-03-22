namespace FileOrkestrator.Integrate.SearchEngine.Options;

/// <summary>
/// Настройки HTTP-клиента внешнего Search Engine.
/// </summary>
public sealed class SearchEngineOptions
{
    public const string SectionName = "SearchEngine";

    /// <summary>
    /// Имя HTTP-заголовка с ключом API (общепринятое: X-API-Key).
    /// </summary>
    public const string ApiKeyHeaderName = "X-API-Key";

    /// <summary>
    /// Базовый URL API (например https://search-engine.internal:8080).
    /// </summary>
    public string BaseUrl { get; set; } = "https://localhost:5001";

    /// <summary>
    /// Таймаут HTTP-запросов к Search Engine.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(100);

    /// <summary>
    /// Секрет для авторизации в Search Engine. Если null или пустая строка, заголовок не отправляется.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Если true — HTTP к Search Engine не выполняется, используется in-memory mock (<see cref="T:FileOrkestrator.Integrate.SearchEngine.Mocks.MockSearchEngineClient"/>).
    /// </summary>
    public bool IsTest { get; set; }
}
