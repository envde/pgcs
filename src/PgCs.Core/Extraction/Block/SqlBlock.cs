namespace PgCs.Core.Extraction.Block;

/// <summary>
/// Представляет отдельный блок SQL кода - одну команду PostgreSQL с контекстом.
/// Блок содержит как чистый SQL код, так и связанные комментарии.
/// </summary>
public sealed record SqlBlock
{
    /// <summary>
    /// Чистый SQL код команды без комментариев.
    /// Используется для парсинга и анализа структуры.
    /// </summary>
    /// <example>CREATE TABLE users (id INT PRIMARY KEY);</example>
    public required string Content { get; init; }

    /// <summary>
    /// Полный исходный текст блока, включая все комментарии и форматирование.
    /// Используется для сохранения оригинального представления кода.
    /// </summary>
    public required string RawContent { get; init; }

    /// <summary>
    /// Комментарий, расположенный непосредственно перед блоком.
    /// Может содержать описание объекта или метаданные для генерации кода.
    /// </summary>
    /// <example>-- Users table for authentication</example>
    public string? HeaderComment { get; init; }

    /// <summary>
    /// Inline-комментарии внутри блока.
    /// Ключ - позиция символа в Content, значение - текст комментария.
    /// </summary>
    public IReadOnlyList<InlineComment>? InlineComments { get; init; }

    /// <summary>
    /// Номер строки начала блока в исходном файле (нумерация с 1).
    /// </summary>
    public required int StartLine { get; init; }

    /// <summary>
    /// Номер строки окончания блока в исходном файле (нумерация с 1).
    /// </summary>
    public required int EndLine { get; init; }

    /// <summary>
    /// Полный путь к файлу-источнику, если блок извлечен из файла.
    /// </summary>
    public string? SourcePath { get; init; }

    /// <summary>
    /// Количество строк в блоке.
    /// </summary>
    public int LineCount => EndLine - StartLine + 1;

    /// <summary>
    /// Проверяет, содержит ли блок хотя бы один комментарий.
    /// </summary>
    public bool HasComments => HeaderComment is not null || InlineComments?.Count > 0;
}