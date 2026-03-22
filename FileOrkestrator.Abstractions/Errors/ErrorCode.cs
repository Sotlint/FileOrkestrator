namespace FileOrkestrator.Abstractions.Errors;

/// <summary>
/// Коды ошибок API оркестратора (в теле Problem Details и для логов/метрик).
/// </summary>
public enum ErrorCode
{
    /// <summary>Не задано / успех.</summary>
    None,

    // Клиент и валидация
    /// <summary>Нарушение правил валидации входных данных.</summary>
    ValidationFailed,

    /// <summary>Некорректный запрос (общий случай).</summary>
    BadRequest,

    /// <summary>Ресурс не найден (общий случай).</summary>
    NotFound,

    /// <summary>Конфликт с текущим состоянием (например повторная операция).</summary>
    Conflict,

    /// <summary>Требуется аутентификация.</summary>
    Unauthorized,

    /// <summary>Доступ запрещён.</summary>
    Forbidden,

    // Домен оркестратора
    /// <summary>Задача индексации с указанным id не найдена.</summary>
    IndexJobNotFound,

    /// <summary>Источник данных не найден.</summary>
    SourceNotFound,

    // Внутренние
    /// <summary>Необработанная ошибка приложения.</summary>
    InternalError,

    // Внешний Search Engine
    /// <summary>Ошибка ответа или вызова Search Engine.</summary>
    ExternalSearchEngineError,

    /// <summary>Таймаут при обращении к Search Engine.</summary>
    ExternalSearchEngineTimeout,

    /// <summary>Search Engine недоступен (503 и аналоги).</summary>
    ExternalSearchEngineUnavailable,
}
