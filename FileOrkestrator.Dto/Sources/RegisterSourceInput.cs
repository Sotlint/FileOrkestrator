namespace FileOrkestrator.Dto.Sources;

/// <summary>Тело запроса на регистрацию источника данных.</summary>
public sealed class RegisterSourceInput
{
    /// <summary>Отображаемое имя источника (обязательно непустое после обрезки пробелов).</summary>
    public string Name { get; set; } = string.Empty;
}
