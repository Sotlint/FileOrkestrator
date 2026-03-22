using FileOrkestrator.Abstractions.Errors;
using FileOrkestrator.Cqrs.Mapping;
using FileOrkestrator.Dal;
using FileOrkestrator.Domain.Indexing;
using FileOrkestrator.Integrate.SearchEngine;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FileOrkestrator.Cqrs.Indexing;

/// <summary>Отмена задачи индексации (локально и при наличии — на стороне Search Engine).</summary>
public sealed record CancelIndexingCommand(Guid JobId) : IRequest<Unit>;

/// <summary>Проверяет терминальные состояния, вызывает отмену у внешнего API и помечает задачу отменённой.</summary>
public sealed class CancelIndexingCommandHandler(
    FileOrkestratorDbContext dbContext,
    ISearchEngineClient searchEngine) : IRequestHandler<CancelIndexingCommand, Unit>
{
    /// <inheritdoc />
    public async Task<Unit> Handle(CancelIndexingCommand request, CancellationToken cancellationToken)
    {
        var job = await dbContext.IndexingJobs.FirstOrDefaultAsync(j => j.Id == request.JobId, cancellationToken).ConfigureAwait(false);
        if (job is null)
            throw new OrchestratorException(ErrorCode.IndexJobNotFound, $"Job '{request.JobId}' was not found.");

        var local = JobStatusMapping.ParseStored(job.Status);
        if (local is OrchestrationJobStatus.Succeeded or OrchestrationJobStatus.Failed or OrchestrationJobStatus.Cancelled)
            throw new OrchestratorException(ErrorCode.Conflict, $"Job is already in terminal state '{job.Status}'.");

        // Без внешнего job id отменять у Search Engine нечего — только локальный статус.
        if (!string.IsNullOrEmpty(job.ExternalJobId))
            await searchEngine.CancelIndexJobAsync(job.ExternalJobId, cancellationToken).ConfigureAwait(false);

        job.MarkCancelled();
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
