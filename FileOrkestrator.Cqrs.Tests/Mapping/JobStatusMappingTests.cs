using FileOrkestrator.Cqrs.Mapping;
using FileOrkestrator.Domain.Indexing;
using FileOrkestrator.Integrate.SearchEngine.Generated;
using Xunit;

namespace FileOrkestrator.Cqrs.Tests.Mapping;

/// <summary>
/// Юнит-тесты <see cref="JobStatusMapping"/>: разбор строки из БД и имя enum из ответа Search Engine.
/// </summary>
public sealed class JobStatusMappingTests
{
    /// <summary>
    /// <see cref="JobStatusMapping.ParseStored"/>: пустые и неизвестные строки → Pending; известные имена enum парсятся.
    /// </summary>
    [Theory]
    [InlineData(null, OrchestrationJobStatus.Pending)]
    [InlineData("", OrchestrationJobStatus.Pending)]
    [InlineData("Running", OrchestrationJobStatus.Running)]
    [InlineData("UnknownEnum", OrchestrationJobStatus.Pending)]
    public void ParseStored_MapsOrFallsBackToPending(string? stored, OrchestrationJobStatus expected)
    {
        Assert.Equal(expected, JobStatusMapping.ParseStored(stored));
    }

    /// <summary>
    /// <see cref="JobStatusMapping.FromExternal"/> возвращает строковое имя значения <see cref="IndexJobState"/> (как в домене).
    /// </summary>
    [Fact]
    public void FromExternal_UsesEnumName()
    {
        Assert.Equal(nameof(IndexJobState.PartialSuccess), JobStatusMapping.FromExternal(IndexJobState.PartialSuccess));
    }
}
