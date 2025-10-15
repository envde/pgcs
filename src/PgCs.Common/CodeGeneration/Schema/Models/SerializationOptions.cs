using PgCs.Common.CodeGeneration.Schema.Models.Enums;

namespace PgCs.Common.CodeGeneration.Schema.Models;

/// <summary>
/// Настройки сериализации моделей
/// </summary>
public sealed record SerializationOptions
{
    /// <summary>
    /// Добавлять атрибуты System.Text.Json
    /// </summary>
    public bool AddSystemTextJsonAttributes { get; init; } = true;

    /// <summary>
    /// Добавлять атрибуты Newtonsoft.Json
    /// </summary>
    public bool AddNewtonsoftJsonAttributes { get; init; } = false;

    /// <summary>
    /// Политика именования для JSON свойств
    /// </summary>
    public JsonNamingPolicy JsonNamingPolicy { get; init; } = JsonNamingPolicy.CamelCase;

    /// <summary>
    /// Игнорировать null значения при сериализации
    /// </summary>
    public bool IgnoreNullValues { get; init; } = false;

    /// <summary>
    /// Использовать JsonSourceGenerators для AOT компиляции (C# 11+)
    /// </summary>
    public bool UseJsonSourceGenerators { get; init; } = true;

    /// <summary>
    /// Добавлять атрибуты для Npgsql типов (например, [PgName])
    /// </summary>
    public bool AddNpgsqlAttributes { get; init; } = false;

    /// <summary>
    /// Генерировать конвертеры для специальных типов
    /// </summary>
    public bool GenerateJsonConverters { get; init; } = false;

    /// <summary>
    /// Имя JsonSerializerContext (если используется source generation)
    /// </summary>
    public string? JsonSerializerContextName { get; init; }
}