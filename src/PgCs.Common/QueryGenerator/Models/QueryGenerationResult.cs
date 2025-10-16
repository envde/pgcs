namespace PgCs.Common.QueryGenerator.Models;

/// <summary>
/// Результат генерации методов запросов
/// </summary>
public sealed record QueryGenerationResult
{
    /// <summary>
    /// Сгенерированные классы с методами
    /// </summary>
    public required IReadOnlyList<GeneratedClass> Classes { get; init; }

    /// <summary>
    /// Сгенерированные модели результатов
    /// </summary>
    public IReadOnlyList<GeneratedModel> ResultModels { get; init; } = Array.Empty<GeneratedModel>();

    /// <summary>
    /// Сгенерированные модели параметров
    /// </summary>
    public IReadOnlyList<GeneratedModel> ParameterModels { get; init; } = Array.Empty<GeneratedModel>();

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
    public QueryGenerationStatistics? Statistics { get; init; }
}

/// <summary>
/// Статистика генерации запросов
/// </summary>
public sealed record QueryGenerationStatistics
{
    /// <summary>
    /// Количество сгенерированных методов
    /// </summary>
    public int MethodsGenerated { get; init; }

    /// <summary>
    /// Количество методов SELECT
    /// </summary>
    public int SelectMethodsGenerated { get; init; }

    /// <summary>
    /// Количество методов INSERT/UPDATE/DELETE
    /// </summary>
    public int MutationMethodsGenerated { get; init; }

    /// <summary>
    /// Количество сгенерированных моделей результатов
    /// </summary>
    public int ResultModelsGenerated { get; init; }

    /// <summary>
    /// Количество сгенерированных моделей параметров
    /// </summary>
    public int ParameterModelsGenerated { get; init; }

    /// <summary>
    /// Общее количество файлов
    /// </summary>
    public int TotalFilesGenerated { get; init; }

    /// <summary>
    /// Общий размер сгенерированного кода в байтах
    /// </summary>
    public long TotalBytesGenerated { get; init; }
}
