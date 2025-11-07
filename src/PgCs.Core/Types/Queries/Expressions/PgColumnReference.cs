namespace PgCs.Core.Types.Queries.Expressions;

/// <summary>
/// Ссылка на колонку таблицы в SQL выражении
/// Примеры: column_name, table.column, schema.table.column
/// </summary>
public sealed record PgColumnReference : PgExpression
{
    /// <summary>
    /// Имя колонки (обязательно)
    /// </summary>
    public required string ColumnName { get; init; }

    /// <summary>
    /// Имя таблицы или алиас (опционально)
    /// Примеры: "users", "u" (алиас)
    /// </summary>
    public string? TableName { get; init; }

    /// <summary>
    /// Имя схемы (опционально)
    /// Пример: "public", "app"
    /// </summary>
    public string? SchemaName { get; init; }

    /// <summary>
    /// Звёздочка для выбора всех колонок: SELECT *
    /// </summary>
    public bool IsWildcard { get; init; }
}
