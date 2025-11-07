using PgCs.Core.Parser.Metadata;

namespace PgCs.Core.Types.Base;

/// <summary>
/// Атрибут (поле) композитного типа PostgreSQL
/// </summary>
/// <remarks>
/// Композитный тип (CREATE TYPE ... AS) состоит из нескольких именованных атрибутов,
/// подобно структуре или record в других языках программирования.
/// </remarks>
/// <example>
/// CREATE TYPE address AS (
///     street varchar(100),
///     city varchar(50),
///     zip_code varchar(10)
/// );
/// </example>
public sealed record PgCompositeTypeAttribute
{
    /// <summary>
    /// Имя атрибута (поля) композитного типа
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Тип данных PostgreSQL атрибута
    /// </summary>
    /// <example>varchar, integer, timestamp, uuid</example>
    public required string DataType { get; init; }

    /// <summary>
    /// Максимальная длина для строковых типов (VARCHAR(n), CHAR(n))
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
    /// Является ли тип массивом
    /// </summary>
    /// <example>integer[], text[]</example>
    public bool IsArray { get; init; }

    /// <summary>
    /// SQL комментарий к атрибуту (может содержать служебные метаданные)
    /// </summary>
    public Comment? Comment { get; init; }

    /// <summary>
    /// Порядковый номер атрибута в композитном типе (начиная с 1)
    /// </summary>
    public int? OrdinalPosition { get; init; }
}
