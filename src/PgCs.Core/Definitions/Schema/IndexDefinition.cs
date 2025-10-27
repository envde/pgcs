using PgCs.Core.Definitions.Schema.Base;

namespace PgCs.Core.Definitions.Schema;

/// <summary>
/// Определение индекса
/// </summary>
public sealed record IndexDefinition: DefinitionBase
{
    /// <summary>
    /// Список колонок, включенных в индекс
    /// </summary>
    public required IReadOnlyList<string> Columns { get; init; }
    
    /// <summary>
    /// Метод индексирования (BTree, Hash, GiST, GIN и т.д.)
    /// </summary>
    public IndexMethod Method { get; init; } = IndexMethod.BTree;
    
    /// <summary>
    /// Уникальный ли индекс (UNIQUE)
    /// </summary>
    public bool IsUnique { get; init; }
    
    /// <summary>
    /// Является ли индекс первичным ключом
    /// </summary>
    public bool IsPrimary { get; init; }
    
    /// <summary>
    /// Частичный ли индекс (имеет WHERE условие)
    /// </summary>
    public bool IsPartial { get; init; }
    
    /// <summary>
    /// WHERE условие для частичного индекса
    /// </summary>
    public string? WhereClause { get; init; }
}