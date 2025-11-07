using PgCs.Core.Parsing.SqlMetadata;

namespace PgCs.Core.Types.Base;

/// <summary>
/// Параметр функции или процедуры PostgreSQL
/// </summary>
public sealed record PgFunctionParameter
{
    /// <summary>
    /// Имя параметра
    /// </summary>
    /// <remarks>
    /// Может отсутствовать для позиционных параметров (безымянных).
    /// В таком случае параметр доступен только по позиции: $1, $2, $3...
    /// </remarks>
    public string? Name { get; init; }

    /// <summary>
    /// Тип данных PostgreSQL параметра
    /// </summary>
    /// <example>integer, text, timestamp, RECORD, my_custom_type</example>
    public required string DataType { get; init; }

    /// <summary>
    /// Режим параметра: IN (входной), OUT (выходной), INOUT, VARIADIC
    /// </summary>
    public PgParameterMode Mode { get; init; } = PgParameterMode.In;

    /// <summary>
    /// Значение по умолчанию для параметра (DEFAULT expression)
    /// </summary>
    /// <example>0, 'unknown', NULL, now()</example>
    public string? DefaultValue { get; init; }

    /// <summary>
    /// Является ли параметр массивом
    /// </summary>
    /// <example>integer[], text[]</example>
    public bool IsArray { get; init; }

    /// <summary>
    /// SQL комментарий к параметру (может содержать служебные метаданные)
    /// </summary>
    public SqlComment? Comment { get; init; }

    /// <summary>
    /// Порядковый номер параметра (начиная с 1)
    /// </summary>
    public int? OrdinalPosition { get; init; }
}
