using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Определение представления (CREATE VIEW / CREATE MATERIALIZED VIEW)
/// Представление - виртуальная таблица, основанная на результате SQL запроса
/// </summary>
public sealed record ViewDefinition: DefinitionBase
{
    /// <summary>
    /// Имя представления
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// SQL запрос, определяющий представление (SELECT ...)
    /// </summary>
    public required string Query { get; init; }
    
    /// <summary>
    /// Является ли представление материализованным (MATERIALIZED VIEW)
    /// Материализованные представления физически хранят данные
    /// </summary>
    public bool IsMaterialized { get; init; }
    
    /// <summary>
    /// Список колонок представления с их типами и свойствами
    /// </summary>
    public IReadOnlyList<TableColumn> Columns { get; init; } = [];
    
    /// <summary>
    /// Опция безопасности представления (WITH CHECK OPTION)
    /// Определяет, можно ли изменять данные через представление
    /// </summary>
    public bool WithCheckOption { get; init; }
    
    /// <summary>
    /// Использует ли представление SECURITY BARRIER
    /// Защищает от утечки данных через побочные эффекты функций
    /// </summary>
    public bool IsSecurityBarrier { get; init; }
}