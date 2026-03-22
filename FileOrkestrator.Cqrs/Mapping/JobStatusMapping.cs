using FileOrkestrator.Domain.Indexing;
using FileOrkestrator.Integrate.SearchEngine.Generated;

namespace FileOrkestrator.Cqrs.Mapping;

/// <summary>
/// Сопоставление строки статуса в БД с <see cref="OrchestrationJobStatus"/> и значения внешнего enum Search Engine.
/// </summary>
internal static class JobStatusMapping
{
    /// <summary>Разбирает сохранённую строку; неизвестное значение трактуется как <see cref="OrchestrationJobStatus.Pending"/>.</summary>
    public static OrchestrationJobStatus ParseStored(string? status)
    {
        if (string.IsNullOrEmpty(status))
            return OrchestrationJobStatus.Pending;
        return Enum.TryParse<OrchestrationJobStatus>(status, out var s) ? s : OrchestrationJobStatus.Pending;
    }

    /// <summary>Преобразует статус ответа Search Engine в строку для поля <c>IndexingJob.Status</c> (имена совпадают с доменом).</summary>
    public static string FromExternal(IndexJobState state) => state.ToString();
}
