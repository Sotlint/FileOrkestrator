namespace FileOrkestrator.Dto.Search;

/// <summary>Один документ в выдаче поиска.</summary>
public sealed class SearchHitDto
{
    /// <summary>Идентификатор документа в индексе.</summary>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>Заголовок или имя файла.</summary>
    public string? Title { get; set; }

    /// <summary>Фрагмент текста с подсветкой совпадения.</summary>
    public string? Snippet { get; set; }

    /// <summary>Релевантность (чем выше, тем ближе к запросу).</summary>
    public double? Score { get; set; }

    /// <summary>Путь или URI источника в хранилище.</summary>
    public string? SourcePath { get; set; }
}
