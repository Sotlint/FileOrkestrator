using FileOrkestrator.Integrate.SearchEngine.Generated;

namespace FileOrkestrator.Integrate.SearchEngine;

/// <summary>
/// Скоупный сервис над внешним Search Engine (Refit <see cref="ISearchEngineApi"/>).
/// </summary>
public interface ISearchEngineClient
{
    /// <summary>Ставит задачу индексации во внешнем API.</summary>
    Task<StartIndexJobResponse> StartIndexJobAsync(StartIndexJobRequest request, CancellationToken cancellationToken = default);

    /// <summary>Возвращает текущий статус задачи по её id у Search Engine.</summary>
    Task<IndexJobStatus> GetIndexJobAsync(string jobId, CancellationToken cancellationToken = default);

    /// <summary>Запрашивает отмену задачи на стороне Search Engine.</summary>
    Task CancelIndexJobAsync(string jobId, CancellationToken cancellationToken = default);

    /// <summary>Выполняет поиск по индексу с пагинацией.</summary>
    Task<SearchResponse> SearchAsync(string query, int? skip = null, int? take = null, CancellationToken cancellationToken = default);
}
