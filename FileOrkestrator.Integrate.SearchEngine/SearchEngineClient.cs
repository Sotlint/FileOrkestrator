using FileOrkestrator.Integrate.SearchEngine.Generated;
using Microsoft.Extensions.Logging;

namespace FileOrkestrator.Integrate.SearchEngine;

/// <summary>
/// Обёртка над сгенерированным Refit-клиентом: единая точка для логирования и будущих политик (ретраи, заголовки).
/// </summary>
public sealed class SearchEngineClient : ISearchEngineClient
{
    private readonly ISearchEngineApi _api;
    private readonly ILogger<SearchEngineClient> _logger;

    /// <summary>Создаёт клиент с Refit API и логгером.</summary>
    public SearchEngineClient(ISearchEngineApi api, ILogger<SearchEngineClient> logger)
    {
        _api = api;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<StartIndexJobResponse> StartIndexJobAsync(StartIndexJobRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("SearchEngine: StartIndexJob sourceId={SourceId}", request.SourceId);
        return await _api.StartIndexJob(request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IndexJobStatus> GetIndexJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("SearchEngine: GetIndexJob jobId={JobId}", jobId);
        return await _api.GetIndexJob(jobId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task CancelIndexJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("SearchEngine: CancelIndexJob jobId={JobId}", jobId);
        await _api.CancelIndexJob(jobId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SearchResponse> SearchAsync(string query, int? skip = null, int? take = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        _logger.LogDebug("SearchEngine: Search q length={Length}", query.Length);
        return await _api.Search(query, skip, take, cancellationToken).ConfigureAwait(false);
    }
}
