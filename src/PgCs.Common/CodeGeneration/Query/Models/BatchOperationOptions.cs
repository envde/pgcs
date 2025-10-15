namespace PgCs.Common.CodeGeneration.Query.Models;

/// <summary>
/// Настройки для батч операций
/// </summary>
public sealed record BatchOperationOptions
{
    /// <summary>
    /// Генерировать методы для батч вставки
    /// </summary>
    public bool GenerateBatchInsert { get; init; } = true;

    /// <summary>
    /// Генерировать методы для батч обновления
    /// </summary>
    public bool GenerateBatchUpdate { get; init; } = true;

    /// <summary>
    /// Использовать COPY для батч вставки (быстрее для больших объемов)
    /// </summary>
    public bool UseCopyForBatchInsert { get; init; } = true;

    /// <summary>
    /// Максимальный размер батча по умолчанию
    /// </summary>
    public int DefaultBatchSize { get; init; } = 1000;

    /// <summary>
    /// Генерировать перегрузки с IAsyncEnumerable для стриминга
    /// </summary>
    public bool GenerateStreamingOverloads { get; init; } = true;

    /// <summary>
    /// Использовать ValueTask для батч операций
    /// </summary>
    public bool UseValueTaskForBatch { get; init; } = true;
}