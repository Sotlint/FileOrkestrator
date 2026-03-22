using FileOrkestrator.Cqrs.Search;
using FileOrkestrator.Dto.Search;
using FileOrkestrator.Host.Routing;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FileOrkestrator.Host.Controllers;

/// <summary>HTTP API поиска по индексу (прокси к Search Engine).</summary>
[ApiController]
[Route($"{ApiRoutePrefixes.Orkestrator}/v1/search")]
public sealed class SearchController(ISender sender) : ControllerBase
{
    /// <summary>Поиск по строке через внешний Search Engine.</summary>
    [HttpGet]
    public Task<SearchDocumentsDto> Search(
        [FromQuery(Name = "q")] string q,
        [FromQuery] int? skip,
        [FromQuery] int? take,
        CancellationToken cancellationToken)
        => sender.Send(new SearchDocumentsQuery(q, skip, take), cancellationToken);
}