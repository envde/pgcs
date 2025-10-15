using PgCs.Common.CodeGeneration.Models.Enums;

namespace PgCs.Common.CodeGeneration.Schema.Models;

/// <summary>
/// Настройки генерации моделей из схемы БД
/// </summary>
public sealed record ModelGenerationOptions
{
    /// <summary>
    /// Пространство имен для сгенерированных моделей
    /// </summary>
    public string Namespace { get; init; } = "Generated.Models";

    /// <summary>
    /// Стратегия именования классов
    /// </summary>
    public NamingStrategy Naming { get; init; } = new();

    /// <summary>
    /// Настройки сериализации
    /// </summary>
    public SerializationOptions Serialization { get; init; } = new();

    /// <summary>
    /// Настройки маппинга типов PostgreSQL -> C#
    /// </summary>
    public TypeMappingOptions TypeMapping { get; init; } = new();

    /// <summary>
    /// Использовать nullable reference types
    /// </summary>
    public bool UseNullableReferenceTypes { get; init; } = true;

    /// <summary>
    /// Генерировать record вместо class
    /// </summary>
    public bool UseRecords { get; init; } = true;

    /// <summary>
    /// Использовать required для обязательных свойств (C# 11+)
    /// </summary>
    public bool UseRequiredModifier { get; init; } = true;

    /// <summary>
    /// Использовать primary constructors (C# 12+)
    /// </summary>
    public bool UsePrimaryConstructors { get; init; } = false;

    /// <summary>
    /// Добавлять атрибуты валидации из System.ComponentModel.DataAnnotations
    /// </summary>
    public bool AddValidationAttributes { get; init; } = false;

    /// <summary>
    /// Генерировать partial классы
    /// </summary>
    public bool UsePartialClasses { get; init; } = true;

    /// <summary>
    /// Добавлять XML комментарии
    /// </summary>
    public bool GenerateXmlComments { get; init; } = true;

    /// <summary>
    /// Добавлять атрибут [Table] с именем таблицы
    /// </summary>
    public bool AddTableAttribute { get; init; } = false;

    /// <summary>
    /// Добавлять атрибут [Column] с именем колонки
    /// </summary>
    public bool AddColumnAttributes { get; init; } = false;

    /// <summary>
    /// Использовать collection expressions (C# 12+)
    /// </summary>
    public bool UseCollectionExpressions { get; init; } = true;

    /// <summary>
    /// Группировать модели по схемам БД
    /// </summary>
    public bool GroupBySchema { get; init; } = false;

    /// <summary>
    /// Режим генерации файлов
    /// </summary>
    public FileGenerationMode FileMode { get; init; } = FileGenerationMode.OnePerTable;

    /// <summary>
    /// Генерировать классы для композитных типов PostgreSQL
    /// </summary>
    public bool GenerateCompositeTypes { get; init; } = true;

    /// <summary>
    /// Генерировать классы для domain типов
    /// </summary>
    public bool GenerateDomainTypes { get; init; } = false;

    /// <summary>
    /// Использовать file-scoped namespaces (C# 10+)
    /// </summary>
    public bool UseFileScopedNamespaces { get; init; } = true;

    /// <summary>
    /// Добавлять комментарии из схемы БД
    /// </summary>
    public bool IncludeDatabaseComments { get; init; } = true;

    /// <summary>
    /// Генерировать расширения для работы с моделями
    /// </summary>
    public bool GenerateExtensions { get; init; } = false;
}