using PgCs.Common.CodeGeneration;

namespace PgCs.Common.SchemaGenerator.Models.Options;

/// <summary>
/// Опции генерации схемы базы данных
/// </summary>
public sealed record SchemaGenerationOptions : CodeGenerationOptions
{
    /// <summary>
    /// Генерировать primary constructors (C# 12+)
    /// </summary>
    public bool UsePrimaryConstructors { get; init; } = false;

    /// <summary>
    /// Генерировать атрибуты для маппинга (Column, Table и т.д.)
    /// </summary>
    public bool GenerateMappingAttributes { get; init; } = true;

    /// <summary>
    /// Генерировать атрибуты валидации (Required, MaxLength и т.д.)
    /// </summary>
    public bool GenerateValidationAttributes { get; init; } = true;

    /// <summary>
    /// Префикс для имен таблиц (будет удален из имени типа)
    /// </summary>
    public string? TablePrefix { get; init; }

    /// <summary>
    /// Суффикс для имен таблиц (будет удален из имени типа)
    /// </summary>
    public string? TableSuffix { get; init; }

    /// <summary>
    /// Маппинг имен таблиц на имена типов
    /// </summary>
    public IReadOnlyDictionary<string, string>? TableNameMappings { get; init; }

    /// <summary>
    /// Маппинг имен колонок на имена свойств
    /// </summary>
    public IReadOnlyDictionary<string, string>? ColumnNameMappings { get; init; }

    /// <summary>
    /// Маппинг PostgreSQL типов на C# типы
    /// </summary>
    public IReadOnlyDictionary<string, string>? CustomTypeMappings { get; init; }

    /// <summary>
    /// Исключить таблицы из генерации (regex паттерны)
    /// </summary>
    public IReadOnlyList<string>? ExcludeTablePatterns { get; init; }

    /// <summary>
    /// Включить только эти таблицы в генерацию (regex паттерны)
    /// </summary>
    public IReadOnlyList<string>? IncludeTablePatterns { get; init; }

    /// <summary>
    /// Генерировать методы для функций базы данных
    /// </summary>
    public bool GenerateFunctions { get; init; } = true;

    /// <summary>
    /// Организация файлов по типам объектов
    /// </summary>
    public FileOrganization FileOrganization { get; init; } = FileOrganization.ByType;

    /// <summary>
    /// Создаёт новый builder для настройки опций через Fluent API
    /// </summary>
    /// <returns>Builder для конфигурации опций</returns>
    public static SchemaGenerationOptionsBuilder CreateBuilder() => new();
}
