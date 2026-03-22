using FileOrkestrator.Cqrs.Indexing;
using FileOrkestrator.Dto.Indexing;
using FileOrkestrator.Host.Routing;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FileOrkestrator.Host.Controllers;

/// <summary>HTTP API задач индексации: статус и отмена.</summary>
[ApiController]
[Route($"{ApiRoutePrefixes.Orkestrator}/v1/jobs")]
public sealed class JobsController(ISender sender) : ControllerBase
{
    /// <summary>Статус задачи индексации (оркестратор + актуальное состояние Search Engine).</summary>
    [HttpGet("{jobId:guid}")]
    public Task<JobStatusDto> GetStatus(Guid jobId, CancellationToken cancellationToken)
    {
        return sender.Send(new GetJobStatusQuery(jobId), cancellationToken);
    }

    /// <summary>Запрос отмены задачи индексации.</summary>
    [HttpDelete("{jobId:guid}")]
    public async Task<bool> Cancel(Guid jobId, CancellationToken cancellationToken)
    {
        await sender.Send(new CancelIndexingCommand(jobId), cancellationToken);
        return true;
    }
}