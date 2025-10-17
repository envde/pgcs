namespace PgCs.Common.QueryGenerator.Models.Results;

/// <summary>
/// Статистика генерации методов запросов
/// </summary>
public sealed record QueryGenerationStatistics
{
    /// <summary>
    /// Общее количество сгенерированных файлов
    /// </summary>
    public int TotalFilesGenerated { get; init; }

    /// <summary>
    /// Общий размер сгенерированных файлов в байтах
    /// </summary>
    public long TotalSizeInBytes { get; init; }

    /// <summary>
    /// Количество сгенерированных строк кода
    /// </summary>
    public int TotalLinesOfCode { get; init; }

    /// <summary>
    /// Количество обработанных запросов
    /// </summary>
    public int QueriesProcessed { get; init; }

    /// <summary>
    /// Количество сгенерированных методов
    /// </summary>
    public int MethodsGenerated { get; init; }

    /// <summary>
    /// Количество сгенерированных моделей результатов
    /// </summary>
    public int ResultModelsGenerated { get; init; }

    /// <summary>
    /// Количество сгенерированных моделей параметров
    /// </summary>
    public int ParameterModelsGenerated { get; init; }

    /// <summary>
    /// Количество SELECT запросов
    /// </summary>
    public int SelectQueriesCount { get; init; }

    /// <summary>
    /// Количество INSERT запросов
    /// </summary>
    public int InsertQueriesCount { get; init; }

    /// <summary>
    /// Количество UPDATE запросов
    /// </summary>
    public int UpdateQueriesCount { get; init; }

    /// <summary>
    /// Количество DELETE запросов
    /// </summary>
    public int DeleteQueriesCount { get; init; }

    /// <summary>
    /// Количество ошибок валидации
    /// </summary>
    public int ErrorCount { get; init; }

    /// <summary>
    /// Количество предупреждений валидации
    /// </summary>
    public int WarningCount { get; init; }
}
