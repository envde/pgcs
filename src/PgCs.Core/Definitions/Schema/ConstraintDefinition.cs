using PgCs.Core.Definitions.Schema.Base;

namespace PgCs.Core.Definitions.Schema;

/// <summary>
/// Определение ограничения (constraint)
/// </summary>
public sealed record ConstraintDefinition: DefinitionBase
{
    /// <summary>
    /// Тип ограничения (PrimaryKey, ForeignKey, Unique, Check, Exclude)
    /// </summary>
    public required ConstraintType Type { get; init; }
    
    /// <summary>
    /// Список колонок, участвующих в ограничении
    /// </summary>
    public IReadOnlyList<string> Columns { get; init; } = [];
    
    /// <summary>
    /// Имя таблицы, на которую ссылается внешний ключ (для ForeignKey)
    /// </summary>
    public string? ReferencedTable { get; init; }
    
    /// <summary>
    /// Список колонок в референсной таблице (для ForeignKey)
    /// </summary>
    public IReadOnlyList<string>? ReferencedColumns { get; init; }
    
    /// <summary>
    /// Действие при удалении референсной записи (для ForeignKey)
    /// </summary>
    public ReferentialAction? OnDelete { get; init; }
    
    /// <summary>
    /// Действие при обновлении референсной записи (для ForeignKey)
    /// </summary>
    public ReferentialAction? OnUpdate { get; init; }
    
    /// <summary>
    /// SQL выражение для CHECK ограничения
    /// </summary>
    public string? CheckExpression { get; init; }
    
    /// <summary>
    /// Может ли ограничение быть отложенным (DEFERRABLE)
    /// </summary>
    public bool IsDeferrable { get; init; }
    
    /// <summary>
    /// Отложено ли ограничение по умолчанию (INITIALLY DEFERRED)
    /// </summary>
    public bool IsInitiallyDeferred { get; init; }
}