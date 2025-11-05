namespace PgCs.Core.Schema.Common;

/// <summary>
/// Определение колонки таблицы или представления
/// </summary>
public sealed record TableColumn
{
    /// <summary>
    /// Имя колонки
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Содержит переименованное название колонки, которое можно указать с помощью специального комментария
    /// </summary>
    public string? ReName { get; init; }
    
    /// <summary>
    /// Тип данных PostgreSQL (varchar, integer, timestamp, uuid и т.д.)
    /// </summary>
    public required string DataType { get; init; }
    
    /// <summary>
    /// Допускает ли колонка NULL значения
    /// По умолчанию true (nullable), false для NOT NULL
    /// </summary>
    public bool IsNullable { get; init; } = true;
    
    /// <summary>
    /// Является ли колонка частью первичного ключа (PRIMARY KEY)
    /// </summary>
    public bool IsPrimaryKey { get; init; }
    
    /// <summary>
    /// Имеет ли колонка ограничение уникальности (UNIQUE)
    /// </summary>
    public bool IsUnique { get; init; }
    
    /// <summary>
    /// Является ли тип массивом (например, integer[], text[])
    /// </summary>
    public bool IsArray { get; init; }
    
    /// <summary>
    /// Значение по умолчанию (DEFAULT expression)
    /// Может быть константой, функцией или последовательностью
    /// </summary>
    public string? DefaultValue { get; init; }
    
    /// <summary>
    /// Максимальная длина для строковых типов (VARCHAR(n), CHAR(n))
    /// </summary>
    public int? MaxLength { get; init; }
    
    /// <summary>
    /// Точность для числовых типов (NUMERIC(precision, scale))
    /// Общее количество значащих цифр
    /// </summary>
    public int? NumericPrecision { get; init; }
    
    /// <summary>
    /// Масштаб для числовых типов (NUMERIC(precision, scale))
    /// Количество цифр после десятичной точки
    /// </summary>
    public int? NumericScale { get; init; }
    
    /// <summary>
    /// Комментарий к колонке
    /// </summary>
    public string? SqlComment { get; init; }
    
    /// <summary>
    /// Является ли колонка автоинкрементной (SERIAL, BIGSERIAL, GENERATED ALWAYS AS IDENTITY)
    /// </summary>
    public bool IsIdentity { get; init; }
    
    /// <summary>
    /// Тип генерации значения: ALWAYS или BY DEFAULT (для IDENTITY колонок)
    /// </summary>
    public string? IdentityGeneration { get; init; }
    
    /// <summary>
    /// Является ли колонка вычисляемой (GENERATED ALWAYS AS ... STORED)
    /// </summary>
    public bool IsGenerated { get; init; }
    
    /// <summary>
    /// Выражение для вычисляемой колонки
    /// </summary>
    public string? GenerationExpression { get; init; }
    
    /// <summary>
    /// Collation (правило сортировки) для строковых колонок
    /// </summary>
    public string? Collation { get; init; }
    
    /// <summary>
    /// Порядковый номер колонки в таблице (начиная с 1)
    /// </summary>
    public int? OrdinalPosition { get; init; }
}