using System.Collections.Concurrent;
using FileOrkestrator.Integrate.SearchEngine.Generated;
using Microsoft.Extensions.Logging;

namespace FileOrkestrator.Integrate.SearchEngine.Mocks;

/// <summary>
/// Заглушка Search Engine для локальной отладки и тестов при <c>SearchEngine:IsTest = true</c>.
/// Состояние задач хранится в статической памяти процесса (общее для всех scope).
/// </summary>
public sealed class MockSearchEngineClient : ISearchEngineClient
{
    private static readonly ConcurrentDictionary<string, IndexJobStatus> Jobs = new(StringComparer.Ordinal);

    private readonly ILogger<MockSearchEngineClient> _logger;

    /// <summary>Создаёт mock с логгером.</summary>
    public MockSearchEngineClient(ILogger<MockSearchEngineClient> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<StartIndexJobResponse> StartIndexJobAsync(StartIndexJobRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("SearchEngine mock: StartIndexJob sourceId={SourceId}", request.SourceId);

        var jobId = $"mock-{Guid.NewGuid():N}";
        var now = DateTimeOffset.UtcNow;

        var status = new IndexJobStatus
        {
            JobId = jobId,
            Status = IndexJobState.Succeeded,
            Progress = 1f,
            IndexedCount = request.FilePaths?.Count ?? 0,
            FailedCount = 0,
            ErrorMessage = null,
            StartedAt = now,
            CompletedAt = now,
            PartialSuccess = false,
        };

        Jobs[jobId] = status;

        return Task.FromResult(new StartIndexJobResponse
        {
            JobId = jobId,
            AcceptedAt = now,
            IsDuplicate = false,
        });
    }

    /// <inheritdoc />
    public Task<IndexJobStatus> GetIndexJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("SearchEngine mock: GetIndexJob jobId={JobId}", jobId);

        if (Jobs.TryGetValue(jobId, out var status))
            return Task.FromResult(Clone(status));

        return Task.FromResult(new IndexJobStatus
        {
            JobId = jobId,
            Status = IndexJobState.Failed,
            Progress = 0,
            IndexedCount = 0,
            FailedCount = 0,
            ErrorMessage = "Unknown job (mock)",
            StartedAt = null,
            CompletedAt = DateTimeOffset.UtcNow,
            PartialSuccess = false,
        });
    }

    /// <inheritdoc />
    public Task CancelIndexJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("SearchEngine mock: CancelIndexJob jobId={JobId}", jobId);

        if (!Jobs.TryGetValue(jobId, out var current))
            return Task.CompletedTask;

        var now = DateTimeOffset.UtcNow;
        Jobs[jobId] = new IndexJobStatus
        {
            JobId = jobId,
            Status = IndexJobState.Cancelled,
            Progress = current.Progress,
            IndexedCount = current.IndexedCount,
            FailedCount = current.FailedCount,
            ErrorMessage = current.ErrorMessage,
            StartedAt = current.StartedAt,
            CompletedAt = now,
            PartialSuccess = current.PartialSuccess,
        };

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<SearchResponse> SearchAsync(string query, int? skip = null, int? take = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        _logger.LogDebug("SearchEngine mock: Search q={Query}", query);

        var hits = new List<SearchHit>
        {
            new()
            {
                DocumentId = "mock-doc-1",
                Title = "Mock document",
                Snippet = $"Match for «{query}» (SearchEngine mock).",
                Score = 1.0,
                SourcePath = "/mock/path.txt",
            },
            new()
            {
                DocumentId = "mock-doc-2",
                Title = "Another mock hit",
                Snippet = "Static mock search result.",
                Score = 0.5,
                SourcePath = "/mock/other.log",
            },
        };

        var s = skip.GetValueOrDefault(0);
        var t = take.GetValueOrDefault(20);
        var page = hits.Skip(s).Take(t).ToList();

        return Task.FromResult(new SearchResponse
        {
            Items = page,
            TotalCount = hits.Count,
        });
    }

    /// <summary>Копия статуса, чтобы не отдавать ссылку на объект в словаре.</summary>
    private static IndexJobStatus Clone(IndexJobStatus s) => new()
    {
        JobId = s.JobId,
        Status = s.Status,
        Progress = s.Progress,
        IndexedCount = s.IndexedCount,
        FailedCount = s.FailedCount,
        ErrorMessage = s.ErrorMessage,
        StartedAt = s.StartedAt,
        CompletedAt = s.CompletedAt,
        PartialSuccess = s.PartialSuccess,
    };
}
