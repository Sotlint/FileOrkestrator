using FileOrkestrator.Abstractions.Errors;
using FileOrkestrator.Cqrs.Sources;
using FileOrkestrator.Cqrs.Tests.TestInfrastructure;
using Xunit;

namespace FileOrkestrator.Cqrs.Tests.Sources;

/// <summary>
/// Тесты <see cref="RegisterSourceCommandHandler"/>: валидация имени и успешное создание <see cref="Dal.Entities.IndexingSource"/>.
/// </summary>
public sealed class RegisterSourceCommandHandlerTests
{
    /// <summary>Пустое имя после <c>Trim</c> → <see cref="ErrorCode.ValidationFailed"/>.</summary>
    [Fact]
    public async Task Handle_EmptyName_ThrowsValidationFailed()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var handler = new RegisterSourceCommandHandler(db);

        var ex = await Assert.ThrowsAsync<OrchestratorException>(() =>
            handler.Handle(new RegisterSourceCommand("   "), CancellationToken.None));

        Assert.Equal(ErrorCode.ValidationFailed, ex.Code);
    }

    /// <summary>Имя обрезается по краям, запись сохраняется, DTO совпадает с сущностью в БД.</summary>
    [Fact]
    public async Task Handle_ValidName_PersistsAndReturnsDto()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var handler = new RegisterSourceCommandHandler(db);

        var result = await handler.Handle(new RegisterSourceCommand("  My Source  "), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("My Source", result.Name);
        Assert.Single(db.IndexingSources);
        Assert.Equal(result.Name, db.IndexingSources.Single().Name);
    }
}
