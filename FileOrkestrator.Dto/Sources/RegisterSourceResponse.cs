namespace FileOrkestrator.Dto.Sources;

/// <summary>Ответ после успешной регистрации источника.</summary>
public sealed class RegisterSourceDto
{
    /// <summary>Идентификатор созданного источника.</summary>
    public Guid Id { get; set; }

    /// <summary>Сохранённое имя источника.</summary>
    public string Name { get; set; } = string.Empty;
}
