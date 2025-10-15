using PgCs.Common.CodeGeneration.Schema.Models.Enums;

namespace PgCs.Common.CodeGeneration.Schema.Models;

/// <summary>
/// Стратегия именования классов и свойств
/// </summary>
public sealed record NamingStrategy
{
    /// <summary>
    /// Преобразование snake_case в PascalCase
    /// </summary>
    public bool ConvertSnakeCaseToPascalCase { get; init; } = true;

    /// <summary>
    /// Режим работы с множественным/единственным числом
    /// </summary>
    public PluralizeMode PluralizeMode { get; init; } = PluralizeMode.Singularize;

    /// <summary>
    /// Префикс для классов моделей
    /// </summary>
    public string? ClassPrefix { get; init; }

    /// <summary>
    /// Суффикс для классов моделей
    /// </summary>
    public string? ClassSuffix { get; init; }

    /// <summary>
    /// Префикс для перечислений
    /// </summary>
    public string? EnumPrefix { get; init; }

    /// <summary>
    /// Суффикс для перечислений
    /// </summary>
    public string? EnumSuffix { get; init; }

    /// <summary>
    /// Суффикс для классов представлений (View)
    /// </summary>
    public string? ViewSuffix { get; init; }

    /// <summary>
    /// Правила переименования таблиц (имя таблицы -> имя класса)
    /// </summary>
    public IReadOnlyDictionary<string, string> TableNameOverrides { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    /// Правила переименования колонок (имя колонки -> имя свойства)
    /// </summary>
    public IReadOnlyDictionary<string, string> ColumnNameOverrides { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    /// Удалять префиксы из имен таблиц (например: tbl_, tb_)
    /// </summary>
    public IReadOnlyList<string> RemoveTablePrefixes { get; init; } = [];

    /// <summary>
    /// Удалять суффиксы из имен таблиц
    /// </summary>
    public IReadOnlyList<string> RemoveTableSuffixes { get; init; } = [];
}