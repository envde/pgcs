using PgCs.Core.Definitions.Schema.Base;

namespace PgCs.Core.Definitions.Schema;

/// <summary>
/// Определение партиции
/// </summary>
public sealed record PartitionDefinition: DefinitionBase
{
    
    /// <summary>
    /// Начальное значение диапазона для RANGE партиции (FROM)
    /// </summary>
    public string? FromValue { get; init; }
    
    /// <summary>
    /// Конечное значение диапазона для RANGE партиции (TO)
    /// </summary>
    public string? ToValue { get; init; }
    
    /// <summary>
    /// Список значений для LIST партиции (IN)
    /// </summary>
    public IReadOnlyList<string>? InValues { get; init; }
    
    /// <summary>
    /// Модуль для HASH партиции (WITH MODULUS)
    /// </summary>
    public int? Modulus { get; init; }
    
    /// <summary>
    /// Остаток для HASH партиции (WITH REMAINDER)
    /// </summary>
    public int? Remainder { get; init; }
}