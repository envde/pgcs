using PgCs.Core.Parsing.SqlMetadata;

namespace PgCs.Core.Types.Base;

/// <summary>
/// Определение колонки таблицы или представления PostgreSQL
/// </summary>
public sealed record PgColumn
{
    /// <summary>
    /// Имя колонки
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Тип данных PostgreSQL
    /// </summary>
    /// <example>varchar, integer, timestamp with time zone, uuid, jsonb</example>
    public required string DataType { get; init; }

    /// <summary>
    /// Допускает ли колонка NULL значения
    /// </summary>
    /// <remarks>
    /// По умолчанию true (nullable), false для NOT NULL constraint
    /// </remarks>
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
    /// Является ли тип массивом
    /// </summary>
    /// <example>integer[], text[], uuid[]</example>
    public bool IsArray { get; init; }

    /// <summary>
    /// Значение по умолчанию (DEFAULT expression)
    /// </summary>
    /// <example>0, 'unknown', now(), gen_random_uuid()</example>
    public string? DefaultValue { get; init; }

    /// <summary>
    /// Максимальная длина для строковых типов (VARCHAR(n), CHAR(n))
    /// </summary>
    public int? MaxLength { get; init; }

    /// <summary>
    /// Точность для числовых типов (NUMERIC(precision, scale))
    /// </summary>
    /// <remarks>
    /// Общее количество значащих цифр
    /// </remarks>
    public int? NumericPrecision { get; init; }

    /// <summary>
    /// Масштаб для числовых типов (NUMERIC(precision, scale))
    /// </summary>
    /// <remarks>
    /// Количество цифр после десятичной точки
    /// </remarks>
    public int? NumericScale { get; init; }

    /// <summary>
    /// SQL комментарий к колонке (может содержать служебные метаданные)
    /// </summary>
    public SqlComment? Comment { get; init; }

    /// <summary>
    /// Является ли колонка автоинкрементной
    /// </summary>
    /// <remarks>
    /// Включает SERIAL, BIGSERIAL, GENERATED ALWAYS AS IDENTITY
    /// </remarks>
    public bool IsIdentity { get; init; }

    /// <summary>
    /// Тип генерации значения для IDENTITY колонок
    /// </summary>
    /// <example>ALWAYS, BY DEFAULT</example>
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
    /// <example>en_US.UTF-8, C, POSIX</example>
    public string? Collation { get; init; }

    /// <summary>
    /// Порядковый номер колонки в таблице (начиная с 1)
    /// </summary>
    public int? OrdinalPosition { get; init; }
}
