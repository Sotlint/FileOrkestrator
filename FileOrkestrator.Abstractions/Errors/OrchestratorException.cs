namespace FileOrkestrator.Abstractions.Errors;

/// <summary>
/// Прикладное исключение с кодом <see cref="ErrorCode"/> для маппинга в HTTP и Problem Details.
/// </summary>
public sealed class OrchestratorException : Exception
{
    /// <summary>Код для маппинга в HTTP и расширения Problem Details.</summary>
    public ErrorCode Code { get; }

    /// <param name="code">Доменный код ошибки.</param>
    /// <param name="message">Сообщение для клиента и логов.</param>
    /// <param name="innerException">Исходное исключение, если есть.</param>
    public OrchestratorException(ErrorCode code, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
    }
}
