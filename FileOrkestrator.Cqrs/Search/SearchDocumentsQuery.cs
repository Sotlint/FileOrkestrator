using FileOrkestrator.Abstractions.Errors;
using FileOrkestrator.Dto.Search;
using FileOrkestrator.Integrate.SearchEngine;
using MediatR;

namespace FileOrkestrator.Cqrs.Search;

/// <summary>Поиск через внешний Search Engine.</summary>
public sealed record SearchDocumentsQuery(string Query, int? Skip, int? Take) : IRequest<SearchDocumentsDto>;

/// <summary>Проксирует запрос к <see cref="ISearchEngineClient"/> и маппит ответ в DTO.</summary>
public sealed class SearchDocumentsQueryHandler(ISearchEngineClient searchEngine)
    : IRequestHandler<SearchDocumentsQuery, SearchDocumentsDto>
{
    /// <inheritdoc />
    public async Task<SearchDocumentsDto> Handle(SearchDocumentsQuery request, CancellationToken cancellationToken)
    {
        var q = request.Query?.Trim() ?? string.Empty;
        if (q.Length == 0)
            throw new OrchestratorException(ErrorCode.ValidationFailed, "Search query 'q' is required.");

        var result = await searchEngine.SearchAsync(q, request.Skip, request.Take, cancellationToken).ConfigureAwait(false);

        var items = result.Items.Select(h => new SearchHitDto
        {
            DocumentId = h.DocumentId,
            Title = h.Title,
            Snippet = h.Snippet,
            Score = h.Score,
            SourcePath = h.SourcePath,
        }).ToList();

        return new SearchDocumentsDto
        {
            Items = items,
            TotalCount = result.TotalCount,
        };
    }
}
