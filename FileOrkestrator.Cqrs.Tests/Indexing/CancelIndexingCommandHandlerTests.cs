using FileOrkestrator.Abstractions.Errors;
using FileOrkestrator.Cqrs.Indexing;
using FileOrkestrator.Cqrs.Tests.TestInfrastructure;
using FileOrkestrator.Dal.Entities;
using FileOrkestrator.Domain.Indexing;
using FileOrkestrator.Integrate.SearchEngine;
using Moq;
using Xunit;

namespace FileOrkestrator.Cqrs.Tests.Indexing;

/// <summary>
/// Тесты <see cref="CancelIndexingCommandHandler"/>: отсутствие задачи, терминальные статусы, вызов отмены у Search Engine.
/// </summary>
public sealed class CancelIndexingCommandHandlerTests
{
    /// <summary>Неизвестный <c>JobId</c> → <see cref="ErrorCode.IndexJobNotFound"/>.</summary>
    [Fact]
    public async Task Handle_JobNotFound_Throws()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var search = new Mock<ISearchEngineClient>(MockBehavior.Strict);
        var handler = new CancelIndexingCommandHandler(db, search.Object);

        var ex = await Assert.ThrowsAsync<OrchestratorException>(() =>
            handler.Handle(new CancelIndexingCommand(Guid.NewGuid()), CancellationToken.None));

        Assert.Equal(ErrorCode.IndexJobNotFound, ex.Code);
    }

    /// <summary>Задача уже в Succeeded → <see cref="ErrorCode.Conflict"/>, без вызова Search Engine.</summary>
    [Fact]
    public async Task Handle_AlreadySucceeded_ThrowsConflict()
    {
        var sourceId = Guid.NewGuid();
        await using var db = TestDbContextFactory.CreateInMemory();
        db.IndexingSources.Add(new IndexingSource { Id = sourceId, Name = "s", CreatedAtUtc = DateTimeOffset.UtcNow });
        var job = IndexingJob.CreatePending(sourceId, null, null);
        db.IndexingJobs.Add(job);
        await db.SaveChangesAsync();

        job.SetExternalJobId("ext");
        job.ApplyStoredStatus(nameof(OrchestrationJobStatus.Succeeded));
        await db.SaveChangesAsync();

        var search = new Mock<ISearchEngineClient>(MockBehavior.Strict);
        var handler = new CancelIndexingCommandHandler(db, search.Object);

        var ex = await Assert.ThrowsAsync<OrchestratorException>(() =>
            handler.Handle(new CancelIndexingCommand(job.Id), CancellationToken.None));

        Assert.Equal(ErrorCode.Conflict, ex.Code);
    }

    /// <summary>
    /// При непустом <c>ExternalJobId</c> вызывается <c>CancelIndexJobAsync</c>, локальный статус — Cancelled.
    /// </summary>
    [Fact]
    public async Task Handle_ActiveJobWithExternalId_CallsCancelAndMarksCancelled()
    {
        var sourceId = Guid.NewGuid();
        await using var db = TestDbContextFactory.CreateInMemory();
        db.IndexingSources.Add(new IndexingSource { Id = sourceId, Name = "s", CreatedAtUtc = DateTimeOffset.UtcNow });
        var job = IndexingJob.CreatePending(sourceId, null, null);
        db.IndexingJobs.Add(job);
        await db.SaveChangesAsync();

        job.SetExternalJobId("ext-99");
        await db.SaveChangesAsync();

        var search = new Mock<ISearchEngineClient>();
        search
            .Setup(x => x.CancelIndexJobAsync("ext-99", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CancelIndexingCommandHandler(db, search.Object);
        await handler.Handle(new CancelIndexingCommand(job.Id), CancellationToken.None);

        search.Verify(x => x.CancelIndexJobAsync("ext-99", It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(nameof(OrchestrationJobStatus.Cancelled), db.IndexingJobs.Single().Status);
    }

    /// <summary>
    /// Без внешнего id отмена только локальная: <c>CancelIndexJobAsync</c> не вызывается, статус всё равно Cancelled.
    /// </summary>
    [Fact]
    public async Task Handle_NoExternalId_DoesNotCallSearchEngineCancel()
    {
        var sourceId = Guid.NewGuid();
        await using var db = TestDbContextFactory.CreateInMemory();
        db.IndexingSources.Add(new IndexingSource { Id = sourceId, Name = "s", CreatedAtUtc = DateTimeOffset.UtcNow });
        var job = IndexingJob.CreatePending(sourceId, null, null);
        db.IndexingJobs.Add(job);
        await db.SaveChangesAsync();

        var search = new Mock<ISearchEngineClient>();
        var handler = new CancelIndexingCommandHandler(db, search.Object);
        await handler.Handle(new CancelIndexingCommand(job.Id), CancellationToken.None);

        search.Verify(x => x.CancelIndexJobAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Equal(nameof(OrchestrationJobStatus.Cancelled), db.IndexingJobs.Single().Status);
    }
}
