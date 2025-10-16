namespace PgCs.Common.SchemaGenerator.Models;

/// <summary>
/// Опции генерации моделей схемы
/// </summary>
public sealed record SchemaGenerationOptions
{
    /// <summary>
    /// Путь к выходной директории для генерации файлов
    /// </summary>
    public required string OutputDirectory { get; init; }

    /// <summary>
    /// Namespace для генерируемых моделей
    /// </summary>
    public required string Namespace { get; init; }

    /// <summary>
    /// Использовать record вместо class
    /// </summary>
    public bool UseRecords { get; init; } = true;

    /// <summary>
    /// Использовать nullable reference types
    /// </summary>
    public bool UseNullableReferenceTypes { get; init; } = true;

    /// <summary>
    /// Использовать init-only свойства
    /// </summary>
    public bool UseInitOnlyProperties { get; init; } = true;

    /// <summary>
    /// Генерировать XML документацию
    /// </summary>
    public bool GenerateXmlDocumentation { get; init; } = true;

    /// <summary>
    /// Генерировать Data Annotations для валидации
    /// </summary>
    public bool GenerateDataAnnotations { get; init; } = true;

    /// <summary>
    /// Генерировать атрибуты для маппинга (например, [Column], [Table])
    /// </summary>
    public bool GenerateMappingAttributes { get; init; } = true;

    /// <summary>
    /// Префикс для имён моделей
    /// </summary>
    public string? ModelPrefix { get; init; }

    /// <summary>
    /// Суффикс для имён моделей
    /// </summary>
    public string? ModelSuffix { get; init; }

    /// <summary>
    /// Генерировать отдельный файл для каждой модели
    /// </summary>
    public bool OneFilePerModel { get; init; } = true;

    /// <summary>
    /// Стратегия именования (PascalCase, camelCase и т.д.)
    /// </summary>
    public NamingStrategy NamingStrategy { get; init; } = NamingStrategy.PascalCase;

    /// <summary>
    /// Генерировать интерфейсы для моделей
    /// </summary>
    public bool GenerateInterfaces { get; init; } = false;

    /// <summary>
    /// Использовать primary constructor (C# 12+)
    /// </summary>
    public bool UsePrimaryConstructor { get; init; } = false;

    /// <summary>
    /// Генерировать методы сравнения и хеширования
    /// </summary>
    public bool GenerateEqualityMembers { get; init; } = false;

    /// <summary>
    /// Формат отступов (пробелы или табуляция)
    /// </summary>
    public IndentationStyle IndentationStyle { get; init; } = IndentationStyle.Spaces;

    /// <summary>
    /// Размер отступа
    /// </summary>
    public int IndentationSize { get; init; } = 4;

    /// <summary>
    /// Дополнительные using директивы
    /// </summary>
    public IReadOnlyList<string> AdditionalUsings { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Перезаписывать существующие файлы
    /// </summary>
    public bool OverwriteExistingFiles { get; init; } = true;

    /// <summary>
    /// Генерировать partial классы
    /// </summary>
    public bool GeneratePartialClasses { get; init; } = true;
}

/// <summary>
/// Стратегия именования
/// </summary>
public enum NamingStrategy
{
    /// <summary>
    /// PascalCase (MyClassName)
    /// </summary>
    PascalCase,

    /// <summary>
    /// camelCase (myClassName)
    /// </summary>
    CamelCase,

    /// <summary>
    /// snake_case (my_class_name)
    /// </summary>
    SnakeCase,

    /// <summary>
    /// Без изменений (сохранить исходное имя)
    /// </summary>
    AsIs
}

/// <summary>
/// Стиль отступов
/// </summary>
public enum IndentationStyle
{
    /// <summary>
    /// Пробелы
    /// </summary>
    Spaces,

    /// <summary>
    /// Табуляция
    /// </summary>
    Tabs
}
