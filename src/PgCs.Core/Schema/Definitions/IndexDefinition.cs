using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение индекса базы данных (CREATE INDEX)
/// Индекс ускоряет поиск данных в таблице
/// </summary>
public sealed record IndexDefinition: DefinitionBase
{
    /// <summary>
    /// Имя индекса
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Имя таблицы, для которой создан индекс
    /// </summary>
    public required string TableName { get; init; }
    
    /// <summary>
    /// Список колонок или выражений, включенных в индекс
    /// Может содержать имена колонок или выражения (например, "LOWER(email)")
    /// </summary>
    public required IReadOnlyList<string> Columns { get; init; }
    
    /// <summary>
    /// Метод индексирования: BTree, Hash, GiST, GIN, SP-GiST, BRIN, Bloom
    /// </summary>
    public IndexMethod Method { get; init; } = IndexMethod.BTree;
    
    /// <summary>
    /// Уникальный ли индекс (CREATE UNIQUE INDEX)
    /// Гарантирует уникальность значений в индексированных колонках
    /// </summary>
    public bool IsUnique { get; init; }
    
    /// <summary>
    /// Является ли индекс первичным ключом (создан через PRIMARY KEY)
    /// </summary>
    public bool IsPrimary { get; init; }
    
    /// <summary>
    /// Частичный ли индекс (имеет WHERE условие)
    /// Индексирует только строки, удовлетворяющие условию
    /// </summary>
    public bool IsPartial { get; init; }
    
    /// <summary>
    /// WHERE условие для частичного индекса
    /// Например: "WHERE status = 'active'"
    /// </summary>
    public string? WhereClause { get; init; }
    
    /// <summary>
    /// INCLUDE колонки (INCLUDE clause) - не участвуют в индексе, но хранятся в нём
    /// Полезно для covering indexes
    /// </summary>
    public IReadOnlyList<string>? IncludeColumns { get; init; }
    
    /// <summary>
    /// Табличное пространство, в котором хранится индекс (TABLESPACE)
    /// </summary>
    public string? Tablespace { get; init; }
    
    /// <summary>
    /// Параметры индекса (storage parameters), например fillfactor
    /// </summary>
    public IReadOnlyDictionary<string, string>? StorageParameters { get; init; }
}