using FileOrkestrator.Abstractions.Errors;
using FileOrkestrator.Cqrs.Search;
using FileOrkestrator.Integrate.SearchEngine;
using FileOrkestrator.Integrate.SearchEngine.Generated;
using Moq;
using Xunit;

namespace FileOrkestrator.Cqrs.Tests.Search;

/// <summary>
/// Тесты <see cref="SearchDocumentsQueryHandler"/>: валидация строки запроса и маппинг ответа Search Engine в DTO.
/// </summary>
public sealed class SearchDocumentsQueryHandlerTests
{
    /// <summary>Пустой запрос после обрезки → <see cref="ErrorCode.ValidationFailed"/>; клиент не дергается (strict mock).</summary>
    [Fact]
    public async Task Handle_EmptyQuery_ThrowsValidationFailed()
    {
        var search = new Mock<ISearchEngineClient>(MockBehavior.Strict);
        var handler = new SearchDocumentsQueryHandler(search.Object);

        var ex = await Assert.ThrowsAsync<OrchestratorException>(() =>
            handler.Handle(new SearchDocumentsQuery("  ", null, null), CancellationToken.None));

        Assert.Equal(ErrorCode.ValidationFailed, ex.Code);
    }

    /// <summary>
    /// Успешный поиск: проброс <c>skip</c>/<c>take</c> в <c>SearchAsync</c>, перенос полей в <see cref="Dto.Search.SearchHitDto"/>.
    /// </summary>
    [Fact]
    public async Task Handle_ValidQuery_MapsHitsAndTotal()
    {
        var search = new Mock<ISearchEngineClient>();
        search
            .Setup(x => x.SearchAsync("hello", 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchResponse
            {
                TotalCount = 2,
                Items = new List<SearchHit>
                {
                    new()
                    {
                        DocumentId = "d1",
                        Title = "T",
                        Snippet = "S",
                        Score = 0.9,
                        SourcePath = "/a",
                    },
                },
            });

        var handler = new SearchDocumentsQueryHandler(search.Object);
        var result = await handler.Handle(new SearchDocumentsQuery("hello", 1, 10), CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("d1", result.Items[0].DocumentId);
        Assert.Equal("T", result.Items[0].Title);
        search.Verify(x => x.SearchAsync("hello", 1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }
}
