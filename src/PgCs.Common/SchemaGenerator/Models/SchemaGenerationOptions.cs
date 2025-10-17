using PgCs.Common.Formatting;
using PgCs.Common.Generation.Models;

namespace PgCs.Common.SchemaGenerator.Models;

/// <summary>
/// Опции генерации моделей схемы
/// </summary>
public sealed record SchemaGenerationOptions : GenerationOptions
{
    /// <summary>
    /// Использовать record вместо class
    /// </summary>
    public bool UseRecords { get; init; } = true;

    /// <summary>
    /// Использовать init-only свойства
    /// </summary>
    public bool UseInitOnlyProperties { get; init; } = true;

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
}
