namespace PgCs.Common.CodeGeneration;

/// <summary>
/// Базовые опции генерации кода (общие для схемы и запросов)
/// </summary>
public abstract record CodeGenerationOptions
{
    /// <summary>
    /// Корневой namespace для сгенерированных типов
    /// </summary>
    public required string RootNamespace { get; init; }

    /// <summary>
    /// Путь к директории для вывода сгенерированных файлов
    /// </summary>
    public required string OutputDirectory { get; init; }

    /// <summary>
    /// Стратегия именования типов
    /// </summary>
    public NamingStrategy NamingStrategy { get; init; } = NamingStrategy.PascalCase;

    /// <summary>
    /// Генерировать nullable reference types (C# 8+)
    /// </summary>
    public bool EnableNullableReferenceTypes { get; init; } = true;

    /// <summary>
    /// Генерировать record типы вместо классов
    /// </summary>
    public bool UseRecordTypes { get; init; } = true;

    /// <summary>
    /// Генерировать init-only свойства
    /// </summary>
    public bool UseInitOnlyProperties { get; init; } = true;

    /// <summary>
    /// Генерировать XML документацию для типов и свойств
    /// </summary>
    public bool GenerateXmlDocumentation { get; init; } = true;

    /// <summary>
    /// Форматировать сгенерированный код
    /// </summary>
    public bool FormatCode { get; init; } = true;

    /// <summary>
    /// Перезаписывать существующие файлы
    /// </summary>
    public bool OverwriteExistingFiles { get; init; } = false;
}
