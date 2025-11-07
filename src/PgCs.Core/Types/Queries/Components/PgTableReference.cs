namespace PgCs.Core.Types.Queries.Components;

/// <summary>
/// Ссылка на таблицу в SQL запросе
/// Может быть использована в FROM, JOIN, INSERT, UPDATE, DELETE клаузах
/// </summary>
public sealed record PgTableReference
{
    /// <summary>
    /// Имя таблицы (обязательно)
    /// </summary>
    public required string TableName { get; init; }

    /// <summary>
    /// Имя схемы (опционально)
    /// Пример: "public", "app"
    /// </summary>
    public string? SchemaName { get; init; }

    /// <summary>
    /// Алиас таблицы (опционально)
    /// Пример: SELECT * FROM users AS u
    /// </summary>
    public string? Alias { get; init; }

    /// <summary>
    /// Использовано ли ключевое слово AS перед алиасом
    /// true для "users AS u", false для "users u"
    /// </summary>
    public bool HasExplicitAs { get; init; }

    /// <summary>
    /// Является ли это ONLY таблицей (без наследников)
    /// Пример: SELECT * FROM ONLY parent_table
    /// </summary>
    public bool IsOnly { get; init; }
}
