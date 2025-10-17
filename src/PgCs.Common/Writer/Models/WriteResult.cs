using PgCs.Common.Writer.Models;

namespace PgCs.Common.Writer;

/// <summary>
/// Результат записи файлов
/// </summary>
public sealed record WriteResult
{
    /// <summary>
    /// Успешность записи
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// Список успешно записанных файлов (абсолютные пути)
    /// </summary>
    public required IReadOnlyList<string> WrittenFiles { get; init; }

    /// <summary>
    /// Список файлов, которые не удалось записать
    /// </summary>
    public IReadOnlyList<WriteError> Errors { get; init; } = [];

    /// <summary>
    /// Список созданных резервных копий (если включено)
    /// </summary>
    public IReadOnlyList<string> BackupFiles { get; init; } = [];

    /// <summary>
    /// Время выполнения операции записи
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Общее количество записанных байт
    /// </summary>
    public long TotalBytesWritten { get; init; }
}
