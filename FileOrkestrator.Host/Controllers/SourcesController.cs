using FileOrkestrator.Cqrs.Indexing;
using FileOrkestrator.Cqrs.Sources;
using FileOrkestrator.Dto.Indexing;
using FileOrkestrator.Dto.Sources;
using FileOrkestrator.Host.Routing;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FileOrkestrator.Host.Controllers;

/// <summary>HTTP API источников данных: регистрация и запуск индексации.</summary>
[ApiController]
[Route($"{ApiRoutePrefixes.Orkestrator}/v1/sources")]
public sealed class SourcesController(ISender sender) : ControllerBase
{
    /// <summary>Регистрация логического источника данных для индексации.</summary>
    [HttpPost]
    public Task<RegisterSourceDto> Register([FromBody] RegisterSourceInput input,
        CancellationToken cancellationToken)
        => sender.Send(new RegisterSourceCommand(input.Name), cancellationToken);

    /// <summary>Запуск индексации источника (полная или по списку путей).</summary>
    [HttpPost("{sourceId:guid}/index")]
    public Task<StartIndexingDto> StartIndexing(
        Guid sourceId,
        [FromBody] StartIndexingInput? input,
        CancellationToken cancellationToken)
        => sender.Send(
            new StartIndexingCommand(sourceId, input?.FilePaths, input?.IdempotencyKey, input?.CorrelationId),
            cancellationToken);
}