namespace PgCs.Core.Types.Queries.Expressions;

/// <summary>
/// Базовый абстрактный класс для всех SQL выражений PostgreSQL
/// Выражения могут использоваться в WHERE, SELECT, HAVING, JOIN ON, и других клаузах
/// </summary>
public abstract record PgExpression
{
    /// <summary>
    /// Тип данных результата выражения (если известен после анализа)
    /// Примеры: "integer", "text", "boolean", "timestamp", etc.
    /// </summary>
    public string? ResultType { get; init; }

    /// <summary>
    /// Может ли выражение возвращать NULL
    /// true - может быть NULL, false - гарантированно NOT NULL
    /// </summary>
    public bool IsNullable { get; init; } = true;

    /// <summary>
    /// Исходный SQL текст выражения (если доступен)
    /// Используется для отладки и восстановления оригинального SQL
    /// </summary>
    public string? RawSql { get; init; }

    /// <summary>
    /// Позиция начала выражения в исходном SQL тексте
    /// </summary>
    public int StartPosition { get; init; }

    /// <summary>
    /// Позиция конца выражения в исходном SQL тексте
    /// </summary>
    public int EndPosition { get; init; }
}
