namespace FileOrkestrator.Dto.Search;

/// <summary>Страница результатов поиска и общее число совпадений.</summary>
public sealed class SearchDocumentsDto
{
    /// <summary>Элементы текущей страницы.</summary>
    public IReadOnlyList<SearchHitDto> Items { get; set; } = [];

    /// <summary>Общее количество документов, удовлетворяющих запросу.</summary>
    public long TotalCount { get; set; }
}
