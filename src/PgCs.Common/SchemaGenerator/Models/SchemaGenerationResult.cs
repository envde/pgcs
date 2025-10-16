namespace PgCs.Common.SchemaGenerator.Models;

/// <summary>
/// Результат генерации моделей схемы
/// </summary>
public sealed record SchemaGenerationResult
{
    /// <summary>
    /// Сгенерированные модели
    /// </summary>
    public required IReadOnlyList<GeneratedModel> Models { get; init; }

    /// <summary>
    /// Путь к выходной директории
    /// </summary>
    public required string OutputDirectory { get; init; }

    /// <summary>
    /// Успешность генерации
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Сообщения об ошибках (если есть)
    /// </summary>
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Предупреждения
    /// </summary>
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Время генерации
    /// </summary>
    public TimeSpan GenerationTime { get; init; }

    /// <summary>
    /// Дата и время генерации
    /// </summary>
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Статистика генерации
    /// </summary>
    public GenerationStatistics? Statistics { get; init; }
}

/// <summary>
/// Статистика генерации
/// </summary>
public sealed record GenerationStatistics
{
    /// <summary>
    /// Количество сгенерированных таблиц
    /// </summary>
    public int TablesGenerated { get; init; }

    /// <summary>
    /// Количество сгенерированных представлений
    /// </summary>
    public int ViewsGenerated { get; init; }

    /// <summary>
    /// Количество сгенерированных типов
    /// </summary>
    public int TypesGenerated { get; init; }

    /// <summary>
    /// Количество сгенерированных перечислений
    /// </summary>
    public int EnumsGenerated { get; init; }

    /// <summary>
    /// Общее количество файлов
    /// </summary>
    public int TotalFilesGenerated { get; init; }

    /// <summary>
    /// Общий размер сгенерированного кода в байтах
    /// </summary>
    public long TotalBytesGenerated { get; init; }
}
