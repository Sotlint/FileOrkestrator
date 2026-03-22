using FileOrkestrator.Abstractions.Errors;
using FileOrkestrator.Cqrs.Mapping;
using FileOrkestrator.Dal;
using FileOrkestrator.Dal.Entities;
using FileOrkestrator.Dto.Indexing;
using FileOrkestrator.Integrate.SearchEngine;
using FileOrkestrator.Integrate.SearchEngine.Generated;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FileOrkestrator.Cqrs.Indexing;

/// <summary>
/// Запуск индексации источника (полный проход или список путей). Создаёт локальную задачу и вызывает внешний Search Engine.
/// </summary>
public sealed record StartIndexingCommand(
    Guid SourceId,
    IReadOnlyList<string>? FilePaths,
    string? IdempotencyKey,
    string? CorrelationId) : IRequest<StartIndexingDto>;

/// <summary>Обработчик: проверка источника, идемпотентность, вызов API и синхронизация статуса.</summary>
public sealed class StartIndexingCommandHandler(
    FileOrkestratorDbContext dbContext,
    ISearchEngineClient searchEngine,
    ILogger<StartIndexingCommandHandler> logger) : IRequestHandler<StartIndexingCommand, StartIndexingDto>
{
    /// <inheritdoc />
    public async Task<StartIndexingDto> Handle(StartIndexingCommand request, CancellationToken cancellationToken)
    {
        // Убеждаемся, что источник существует до создания задачи.
        var sourceExists = await dbContext.IndexingSources
            .AsNoTracking()
            .AnyAsync(s => s.Id == request.SourceId, cancellationToken)
            .ConfigureAwait(false);
        
        if (!sourceExists)
            throw new OrchestratorException(ErrorCode.SourceNotFound, $"Source '{request.SourceId}' was not found.");

        var idempotency = string.IsNullOrWhiteSpace(request.IdempotencyKey) ? null : request.IdempotencyKey.Trim();
        var correlation = string.IsNullOrWhiteSpace(request.CorrelationId) ? null : request.CorrelationId.Trim();

        // Повтор с тем же ключом и источником — возвращаем ранее созданную задачу без второго вызова Search Engine.
        if (idempotency is not null)
        {
            var existing = await dbContext.IndexingJobs
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    j => j.SourceId == request.SourceId && j.IdempotencyKey == idempotency,
                    cancellationToken)
                .ConfigureAwait(false);

            if (existing is not null)
            {
                return new StartIndexingDto
                {
                    JobId = existing.Id,
                    ExternalJobId = existing.ExternalJobId,
                    Status = existing.Status,
                    AcceptedAtUtc = existing.CreatedAtUtc,
                    IsDuplicate = true,
                };
            }
        }

        var job = IndexingJob.CreatePending(request.SourceId, idempotency, correlation);

        dbContext.IndexingJobs.Add(job);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var seRequest = new StartIndexJobRequest
        {
            SourceId = request.SourceId,
            FilePaths = request.FilePaths is { Count: > 0 } ? request.FilePaths.ToList() : new List<string>(),
            IdempotencyKey = idempotency ?? string.Empty,
            CorrelationId = correlation ?? string.Empty,
        };

        try
        {
            var accepted = await searchEngine.StartIndexJobAsync(seRequest, cancellationToken).ConfigureAwait(false);

            job.SetExternalJobId(accepted.JobId);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            // Сразу подтягиваем актуальный статус с Search Engine и сохраняем в БД.
            var remote = await searchEngine.GetIndexJobAsync(accepted.JobId, cancellationToken).ConfigureAwait(false);
            job.ApplyStoredStatus(JobStatusMapping.FromExternal(remote.Status));
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new StartIndexingDto
            {
                JobId = job.Id,
                ExternalJobId = accepted.JobId,
                Status = job.Status,
                AcceptedAtUtc = accepted.AcceptedAt,
                IsDuplicate = accepted.IsDuplicate,
            };
        }
        catch (Exception ex)
        {
            // Фиксируем сбой на стороне Search Engine, чтобы задача не оставалась «вечно Pending».
            job.MarkFailed(ex.Message);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            logger.LogError(ex, "Search engine rejected indexing job {JobId}", job.Id);
            throw new OrchestratorException(ErrorCode.ExternalSearchEngineError, "Search engine failed to start indexing job.", ex);
        }
    }
}
