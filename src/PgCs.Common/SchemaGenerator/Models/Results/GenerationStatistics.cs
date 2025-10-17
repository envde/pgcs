namespace PgCs.Common.SchemaGenerator.Models.Results;

/// <summary>
/// Статистика генерации кода
/// </summary>
public sealed record GenerationStatistics
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
    /// Количество обработанных таблиц
    /// </summary>
    public int TablesProcessed { get; init; }

    /// <summary>
    /// Количество обработанных представлений
    /// </summary>
    public int ViewsProcessed { get; init; }

    /// <summary>
    /// Количество обработанных пользовательских типов
    /// </summary>
    public int TypesProcessed { get; init; }

    /// <summary>
    /// Количество обработанных функций
    /// </summary>
    public int FunctionsProcessed { get; init; }

    /// <summary>
    /// Количество ошибок валидации
    /// </summary>
    public int ErrorCount { get; init; }

    /// <summary>
    /// Количество предупреждений валидации
    /// </summary>
    public int WarningCount { get; init; }
}
