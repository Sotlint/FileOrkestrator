namespace FileOrkestrator.Host.Authentication;

/// <summary>
/// Простая защита HTTP API по заголовку с ключом. Если <see cref="ApiKey"/> не задан, проверка отключена.
/// </summary>
public sealed class ApiAuthenticationOptions
{
    public const string SectionName = "ApiAuthentication";

    /// <summary>Секретный ключ. Пустой или пробелы — авторизация не применяется.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Имя заголовка (по умолчанию X-API-Key).</summary>
    public string HeaderName { get; set; } = "X-API-Key";
}
