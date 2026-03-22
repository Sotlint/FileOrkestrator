using FileOrkestrator.Abstractions.Errors;
using FileOrkestrator.Cqrs.Mapping;
using FileOrkestrator.Dal;
using FileOrkestrator.Dal.Entities;
using FileOrkestrator.Dto.Indexing;
using FileOrkestrator.Integrate.SearchEngine;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FileOrkestrator.Cqrs.Indexing;

/// <summary>Актуальный статус задачи: при наличии внешнего id — опрос Search Engine и обновление локальной записи.</summary>
public sealed record GetJobStatusQuery(Guid JobId) : IRequest<JobStatusDto>;

/// <summary>Возвращает DTO из БД и при необходимости синхронизирует с удалённым API.</summary>
public sealed class GetJobStatusQueryHandler(
    FileOrkestratorDbContext dbContext,
    ISearchEngineClient searchEngine) : IRequestHandler<GetJobStatusQuery, JobStatusDto>
{
    /// <inheritdoc />
    public async Task<JobStatusDto> Handle(GetJobStatusQuery request, CancellationToken cancellationToken)
    {
        var job = await dbContext.IndexingJobs.FirstOrDefaultAsync(j => j.Id == request.JobId, cancellationToken).ConfigureAwait(false);
        if (job is null)
            throw new OrchestratorException(ErrorCode.IndexJobNotFound, $"Job '{request.JobId}' was not found.");

        // Пока Search Engine не выдал job id — только локальные поля (например сразу после сбоя старта).
        if (string.IsNullOrEmpty(job.ExternalJobId))
            return MapFromLocal(job);

        var remote = await searchEngine.GetIndexJobAsync(job.ExternalJobId, cancellationToken).ConfigureAwait(false);

        job.SyncFromRemotePoll(JobStatusMapping.FromExternal(remote.Status), remote.ErrorMessage);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new JobStatusDto
        {
            JobId = job.Id,
            SourceId = job.SourceId,
            ExternalJobId = job.ExternalJobId,
            Status = job.Status,
            Progress = remote.Progress,
            IndexedCount = remote.IndexedCount,
            FailedCount = remote.FailedCount,
            LastError = job.LastError,
            StartedAtUtc = remote.StartedAt,
            CompletedAtUtc = remote.CompletedAt,
            PartialSuccess = remote.PartialSuccess,
        };
    }

    /// <summary>Собирает ответ без обращения к Search Engine (нет внешнего идентификатора).</summary>
    private static JobStatusDto MapFromLocal(IndexingJob job)
    {
        return new JobStatusDto
        {
            JobId = job.Id,
            SourceId = job.SourceId,
            ExternalJobId = job.ExternalJobId,
            Status = job.Status,
            LastError = job.LastError,
        };
    }
}
