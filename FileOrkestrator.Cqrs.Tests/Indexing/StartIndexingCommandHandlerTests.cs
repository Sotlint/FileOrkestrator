using FileOrkestrator.Abstractions.Errors;
using FileOrkestrator.Cqrs.Indexing;
using FileOrkestrator.Cqrs.Tests.TestInfrastructure;
using FileOrkestrator.Dal.Entities;
using FileOrkestrator.Domain.Indexing;
using FileOrkestrator.Integrate.SearchEngine;
using FileOrkestrator.Integrate.SearchEngine.Generated;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace FileOrkestrator.Cqrs.Tests.Indexing;

/// <summary>
/// Тесты <see cref="StartIndexingCommandHandler"/>: проверка источника, идемпотентность, успешный сценарий и ошибка Search Engine.
/// </summary>
public sealed class StartIndexingCommandHandlerTests
{
    /// <summary>Несуществующий <c>SourceId</c> → <see cref="ErrorCode.SourceNotFound"/>; внешний клиент не вызывается (strict mock).</summary>
    [Fact]
    public async Task Handle_SourceMissing_ThrowsSourceNotFound()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var search = new Mock<ISearchEngineClient>(MockBehavior.Strict);
        var handler = new StartIndexingCommandHandler(db, search.Object, NullLogger<StartIndexingCommandHandler>.Instance);

        var ex = await Assert.ThrowsAsync<OrchestratorException>(() =>
            handler.Handle(new StartIndexingCommand(Guid.NewGuid(), null, null, null), CancellationToken.None));

        Assert.Equal(ErrorCode.SourceNotFound, ex.Code);
    }

    /// <summary>
    /// Полный сценарий: источник в БД, мок принимает задачу и отдаёт статус; ответ DTO и строка в <c>IndexingJobs</c> согласованы.
    /// </summary>
    [Fact]
    public async Task Handle_HappyPath_PersistsJobAndReturnsDto()
    {
        var sourceId = Guid.NewGuid();
        await using var db = TestDbContextFactory.CreateInMemory();
        db.IndexingSources.Add(new IndexingSource
        {
            Id = sourceId,
            Name = "src",
            CreatedAtUtc = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();

        const string externalId = "se-job-1";
        var search = new Mock<ISearchEngineClient>();
        search
            .Setup(x => x.StartIndexJobAsync(It.IsAny<StartIndexJobRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StartIndexJobResponse
            {
                JobId = externalId,
                AcceptedAt = DateTimeOffset.Parse("2025-01-02T03:04:05Z"),
                IsDuplicate = false,
            });
        search
            .Setup(x => x.GetIndexJobAsync(externalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IndexJobStatus
            {
                JobId = externalId,
                Status = IndexJobState.Running,
                Progress = 0.25f,
                IndexedCount = 2,
                FailedCount = 0,
                ErrorMessage = null,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = null,
                PartialSuccess = false,
            });

        var handler = new StartIndexingCommandHandler(db, search.Object, NullLogger<StartIndexingCommandHandler>.Instance);
        var result = await handler.Handle(new StartIndexingCommand(sourceId, new[] { "/a" }, null, null), CancellationToken.None);

        Assert.Equal(externalId, result.ExternalJobId);
        Assert.Equal(nameof(IndexJobState.Running), result.Status);
        Assert.False(result.IsDuplicate);

        var stored = db.IndexingJobs.Single();
        Assert.Equal(externalId, stored.ExternalJobId);
        Assert.Equal(nameof(IndexJobState.Running), stored.Status);
    }

    /// <summary>
    /// Второй запрос с тем же ключом идемпотентности возвращает ту же задачу (<c>IsDuplicate</c>), без повторного <c>StartIndexJobAsync</c>.
    /// </summary>
    [Fact]
    public async Task Handle_SameIdempotencyKeyTwice_SecondCallIsDuplicateWithoutSecondStart()
    {
        var sourceId = Guid.NewGuid();
        await using var db = TestDbContextFactory.CreateInMemory();
        db.IndexingSources.Add(new IndexingSource
        {
            Id = sourceId,
            Name = "src",
            CreatedAtUtc = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();

        var search = new Mock<ISearchEngineClient>();
        search
            .Setup(x => x.StartIndexJobAsync(It.IsAny<StartIndexJobRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StartIndexJobResponse
            {
                JobId = "j1",
                AcceptedAt = DateTimeOffset.UtcNow,
                IsDuplicate = false,
            });
        search
            .Setup(x => x.GetIndexJobAsync("j1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IndexJobStatus
            {
                JobId = "j1",
                Status = IndexJobState.Pending,
                Progress = 0,
                IndexedCount = 0,
                FailedCount = 0,
                ErrorMessage = null,
                StartedAt = null,
                CompletedAt = null,
                PartialSuccess = false,
            });

        var handler = new StartIndexingCommandHandler(db, search.Object, NullLogger<StartIndexingCommandHandler>.Instance);
        var cmd = new StartIndexingCommand(sourceId, null, "idem-1", null);

        var first = await handler.Handle(cmd, CancellationToken.None);
        var second = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(first.IsDuplicate);
        Assert.True(second.IsDuplicate);
        Assert.Equal(first.JobId, second.JobId);
        search.Verify(x => x.StartIndexJobAsync(It.IsAny<StartIndexJobRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Исключение при старте у Search Engine → <see cref="ErrorCode.ExternalSearchEngineError"/> и задача в БД в статусе Failed с текстом ошибки.
    /// </summary>
    [Fact]
    public async Task Handle_SearchEngineStartFails_MarksJobFailedAndThrows()
    {
        var sourceId = Guid.NewGuid();
        await using var db = TestDbContextFactory.CreateInMemory();
        db.IndexingSources.Add(new IndexingSource
        {
            Id = sourceId,
            Name = "src",
            CreatedAtUtc = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();

        var search = new Mock<ISearchEngineClient>();
        search
            .Setup(x => x.StartIndexJobAsync(It.IsAny<StartIndexJobRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("upstream"));

        var handler = new StartIndexingCommandHandler(db, search.Object, NullLogger<StartIndexingCommandHandler>.Instance);

        var ex = await Assert.ThrowsAsync<OrchestratorException>(() =>
            handler.Handle(new StartIndexingCommand(sourceId, null, null, null), CancellationToken.None));

        Assert.Equal(ErrorCode.ExternalSearchEngineError, ex.Code);

        var job = db.IndexingJobs.Single();
        Assert.Equal(nameof(OrchestrationJobStatus.Failed), job.Status);
        Assert.Equal("upstream", job.LastError);
    }
}
