using FileOrkestrator.Abstractions.Errors;
using FileOrkestrator.Dal;
using FileOrkestrator.Dal.Entities;
using FileOrkestrator.Dto.Sources;
using MediatR;

namespace FileOrkestrator.Cqrs.Sources;

/// <summary>Регистрация нового логического источника данных для последующей индексации.</summary>
public sealed record RegisterSourceCommand(string Name) : IRequest<RegisterSourceDto>;

/// <summary>Создаёт запись <see cref="IndexingSource"/> с уникальным идентификатором.</summary>
public sealed class RegisterSourceCommandHandler(FileOrkestratorDbContext dbContext) : IRequestHandler<RegisterSourceCommand, RegisterSourceDto>
{
    /// <inheritdoc />
    public async Task<RegisterSourceDto> Handle(RegisterSourceCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (name.Length == 0)
            throw new OrchestratorException(ErrorCode.ValidationFailed, "Source name is required.");

        var entity = new IndexingSource
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };

        dbContext.IndexingSources.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new RegisterSourceDto { Id = entity.Id, Name = entity.Name };
    }
}

