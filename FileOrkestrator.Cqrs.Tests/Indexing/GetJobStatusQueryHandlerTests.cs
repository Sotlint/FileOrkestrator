using FileOrkestrator.Abstractions.Errors;
using FileOrkestrator.Cqrs.Indexing;
using FileOrkestrator.Cqrs.Tests.TestInfrastructure;
using FileOrkestrator.Dal.Entities;
using FileOrkestrator.Domain.Indexing;
using FileOrkestrator.Integrate.SearchEngine;
using FileOrkestrator.Integrate.SearchEngine.Generated;
using Moq;
using Xunit;

namespace FileOrkestrator.Cqrs.Tests.Indexing;

/// <summary>
/// Тесты <see cref="GetJobStatusQueryHandler"/>: отсутствие задачи, ответ только из БД без внешнего id, синхронизация с Search Engine.
/// </summary>
public sealed class GetJobStatusQueryHandlerTests
{
    /// <summary>Несуществующий <c>JobId</c> → <see cref="ErrorCode.IndexJobNotFound"/>.</summary>
    [Fact]
    public async Task Handle_JobNotFound_Throws()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var search = new Mock<ISearchEngineClient>(MockBehavior.Strict);
        var handler = new GetJobStatusQueryHandler(db, search.Object);

        var ex = await Assert.ThrowsAsync<OrchestratorException>(() =>
            handler.Handle(new GetJobStatusQuery(Guid.NewGuid()), CancellationToken.None));

        Assert.Equal(ErrorCode.IndexJobNotFound, ex.Code);
    }

    /// <summary>
    /// Пустой <c>ExternalJobId</c>: DTO строится из локальной строки, <c>GetIndexJobAsync</c> не вызывается.
    /// </summary>
    [Fact]
    public async Task Handle_NoExternalJobId_ReturnsLocalSnapshotWithoutCallingSearchEngine()
    {
        var sourceId = Guid.NewGuid();
        await using var db = TestDbContextFactory.CreateInMemory();
        db.IndexingSources.Add(new IndexingSource { Id = sourceId, Name = "s", CreatedAtUtc = DateTimeOffset.UtcNow });
        var job = IndexingJob.CreatePending(sourceId, null, null);
        db.IndexingJobs.Add(job);
        await db.SaveChangesAsync();

        var search = new Mock<ISearchEngineClient>();
        var handler = new GetJobStatusQueryHandler(db, search.Object);
        var dto = await handler.Handle(new GetJobStatusQuery(job.Id), CancellationToken.None);

        Assert.Equal(job.Id, dto.JobId);
        Assert.Equal(nameof(OrchestrationJobStatus.Pending), dto.Status);
        Assert.Empty(dto.ExternalJobId);
        search.Verify(x => x.GetIndexJobAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// При наличии внешнего id — опрос Search Engine, маппинг полей в DTO и обновление строки задачи в БД.
    /// </summary>
    [Fact]
    public async Task Handle_WithExternalId_SyncsFromSearchEngine()
    {
        var sourceId = Guid.NewGuid();
        await using var db = TestDbContextFactory.CreateInMemory();
        db.IndexingSources.Add(new IndexingSource { Id = sourceId, Name = "s", CreatedAtUtc = DateTimeOffset.UtcNow });
        var job = IndexingJob.CreatePending(sourceId, null, null);
        db.IndexingJobs.Add(job);
        await db.SaveChangesAsync();

        job.SetExternalJobId("ext-1");
        await db.SaveChangesAsync();

        var remote = new IndexJobStatus
        {
            JobId = "ext-1",
            Status = IndexJobState.Succeeded,
            Progress = 1f,
            IndexedCount = 10,
            FailedCount = 1,
            ErrorMessage = "partial",
            StartedAt = DateTimeOffset.Parse("2025-01-01T00:00:00Z"),
            CompletedAt = DateTimeOffset.Parse("2025-01-01T01:00:00Z"),
            PartialSuccess = true,
        };

        var search = new Mock<ISearchEngineClient>();
        search
            .Setup(x => x.GetIndexJobAsync("ext-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(remote);

        var handler = new GetJobStatusQueryHandler(db, search.Object);
        var dto = await handler.Handle(new GetJobStatusQuery(job.Id), CancellationToken.None);

        Assert.Equal(nameof(IndexJobState.Succeeded), dto.Status);
        Assert.Equal(10, dto.IndexedCount);
        Assert.Equal(1, dto.FailedCount);
        Assert.Equal(1f, dto.Progress);
        Assert.True(dto.PartialSuccess);
        Assert.Equal("partial", dto.LastError);
        Assert.Equal(remote.StartedAt, dto.StartedAtUtc);
        Assert.Equal(remote.CompletedAt, dto.CompletedAtUtc);

        Assert.Equal(nameof(IndexJobState.Succeeded), db.IndexingJobs.Single().Status);
    }
}
