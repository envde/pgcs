namespace PgCs.Common.Generation.Models;

/// <summary>
/// Базовые опции для генерации кода
/// </summary>
public abstract record GenerationOptions
{
    /// <summary>
    /// Путь к выходной директории для генерации файлов
    /// </summary>
    public required string OutputDirectory { get; init; }

    /// <summary>
    /// Namespace для генерируемых файлов
    /// </summary>
    public required string Namespace { get; init; }

    /// <summary>
    /// Генерировать XML документацию
    /// </summary>
    public bool GenerateXmlDocumentation { get; init; } = true;

    /// <summary>
    /// Использовать nullable reference types
    /// </summary>
    public bool UseNullableReferenceTypes { get; init; } = true;

    /// <summary>
    /// Стиль отступов (пробелы или табуляция)
    /// </summary>
    public IndentationStyle IndentationStyle { get; init; } = IndentationStyle.Spaces;

    /// <summary>
    /// Размер отступа (количество пробелов или табов)
    /// </summary>
    public int IndentationSize { get; init; } = 4;

    /// <summary>
    /// Дополнительные using директивы
    /// </summary>
    public IReadOnlyList<string> AdditionalUsings { get; init; } = [];

    /// <summary>
    /// Перезаписывать существующие файлы
    /// </summary>
    public bool OverwriteExistingFiles { get; init; } = true;

    /// <summary>
    /// Генерировать partial классы
    /// </summary>
    public bool GeneratePartialClasses { get; init; } = true;
}
