namespace PgCs.Common.CodeGeneration.Schema.Models;
/// <summary>
/// Настройки маппинга типов PostgreSQL -> C#
/// </summary>
public sealed record TypeMappingOptions
{
    /// <summary>
    /// Использовать DateOnly/TimeOnly для date/time типов (C# 10+)
    /// </summary>
    public bool UseDateOnlyTimeOnly { get; init; } = true;

    /// <summary>
    /// Тип для decimal/numeric (decimal, double, float)
    /// </summary>
    public string DecimalType { get; init; } = "decimal";

    /// <summary>
    /// Тип для json/jsonb полей
    /// </summary>
    public string JsonType { get; init; } = "JsonDocument";

    /// <summary>
    /// Использовать System.Collections.Frozen для readonly коллекций
    /// </summary>
    public bool UseFrozenCollections { get; init; } = false;

    /// <summary>
    /// Тип для массивов PostgreSQL (List, Array, IReadOnlyList)
    /// </summary>
    public string ArrayType { get; init; } = "List";

    /// <summary>
    /// Использовать NpgsqlRange<T> для range типов
    /// </summary>
    public bool UseNpgsqlRangeTypes { get; init; } = true;

    /// <summary>
    /// Использовать System.Net.IPAddress для inet/cidr
    /// </summary>
    public bool UseSystemNetTypes { get; init; } = true;

    /// <summary>
    /// Использовать NpgsqlPoint, NpgsqlLine и т.д. для геометрии
    /// </summary>
    public bool UseNpgsqlGeometryTypes { get; init; } = false;

    /// <summary>
    /// Пользовательские маппинги типов PostgreSQL -> C#
    /// </summary>
    public IReadOnlyDictionary<string, string> CustomTypeMappings { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    /// Делать nullable все reference типы по умолчанию
    /// </summary>
    public bool MakeReferenceTypesNullableByDefault { get; init; } = false;
}