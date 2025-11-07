using PgCs.Core.Types.Base;

namespace PgCs.Core.Types.Queries;

/// <summary>
/// Базовый абстрактный класс для всех типов SQL запросов PostgreSQL
/// Запросы НЕ наследуются от PgObject, т.к. это runtime конструкции, а не объекты схемы
/// </summary>
public abstract record PgQuery
{
    /// <summary>
    /// Тип запроса (SELECT, INSERT, UPDATE, DELETE, WITH)
    /// </summary>
    public required PgQueryType QueryType { get; init; }

    /// <summary>
    /// CTE (Common Table Expressions) определённые в WITH клаузе
    /// Применяется ко всему запросу
    /// </summary>
    public IReadOnlyList<PgCommonTableExpression> WithClauses { get; init; } = [];

    /// <summary>
    /// Рекурсивный WITH (WITH RECURSIVE)
    /// </summary>
    public bool IsRecursive { get; init; }

    /// <summary>
    /// Исходный SQL текст запроса (если доступен)
    /// Используется для отладки и восстановления оригинального SQL
    /// </summary>
    public string? RawSql { get; init; }

    /// <summary>
    /// Позиция начала запроса в исходном SQL тексте
    /// </summary>
    public int StartPosition { get; init; }

    /// <summary>
    /// Позиция конца запроса в исходном SQL тексте
    /// </summary>
    public int EndPosition { get; init; }
}
