namespace PgCs.Common.SchemaAnalyzer.Models.Tables;

/// <summary>
/// Определение колонки таблицы
/// </summary>
public sealed record ColumnDefinition
{
    /// <summary>
    /// Имя колонки
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Тип данных PostgreSQL (varchar, integer, timestamp и т.д.)
    /// </summary>
    public required string DataType { get; init; }
    
    /// <summary>
    /// Допускает ли колонка NULL значения
    /// </summary>
    public bool IsNullable { get; init; }
    
    /// <summary>
    /// Является ли колонка первичным ключом
    /// </summary>
    public bool IsPrimaryKey { get; init; }
    
    /// <summary>
    /// Имеет ли колонка ограничение уникальности
    /// </summary>
    public bool IsUnique { get; init; }
    
    /// <summary>
    /// Является ли тип массивом (например, integer[])
    /// </summary>
    public bool IsArray { get; init; }
    
    /// <summary>
    /// Значение по умолчанию (DEFAULT)
    /// </summary>
    public string? DefaultValue { get; init; }
    
    /// <summary>
    /// Максимальная длина для строковых типов (VARCHAR(n))
    /// </summary>
    public int? MaxLength { get; init; }
    
    /// <summary>
    /// Точность для числовых типов (NUMERIC(precision, scale))
    /// </summary>
    public int? NumericPrecision { get; init; }
    
    /// <summary>
    /// Масштаб для числовых типов (NUMERIC(precision, scale))
    /// </summary>
    public int? NumericScale { get; init; }
    
    /// <summary>
    /// Комментарий к колонке (COMMENT ON COLUMN)
    /// </summary>
    public string? Comment { get; init; }
    
    /// <summary>
    /// Список CHECK ограничений, применяемых к колонке
    /// </summary>
    public IReadOnlyList<string> CheckConstraints { get; init; } = [];
}
