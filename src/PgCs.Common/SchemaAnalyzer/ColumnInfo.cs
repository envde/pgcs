namespace PgCs.Common.SchemaAnalyzer;

/// <summary>
/// Информация о колонке
/// </summary>
public class ColumnInfo
{
    /// <summary>
    /// Имя колонки
    /// </summary>
    public required string ColumnName { get; init; }

    /// <summary>
    /// PostgreSQL тип данных
    /// </summary>
    public required string PostgresType { get; init; }

    /// <summary>
    /// C# тип данных
    /// </summary>
    public required string CSharpType { get; init; }

    /// <summary>
    /// Nullable колонка
    /// </summary>
    public bool IsNullable { get; init; }

    /// <summary>
    /// Значение по умолчанию
    /// </summary>
    public string? DefaultValue { get; init; }

    /// <summary>
    /// Является ли частью первичного ключа
    /// </summary>
    public bool IsPrimaryKey { get; init; }

    /// <summary>
    /// Автоинкремент (SERIAL, IDENTITY)
    /// </summary>
    public bool IsAutoIncrement { get; init; }

    /// <summary>
    /// Максимальная длина (для VARCHAR, CHAR)
    /// </summary>
    public int? MaxLength { get; init; }

    /// <summary>
    /// Точность (для NUMERIC, DECIMAL)
    /// </summary>
    public int? Precision { get; init; }

    /// <summary>
    /// Масштаб (для NUMERIC, DECIMAL)
    /// </summary>
    public int? Scale { get; init; }

    /// <summary>
    /// Порядковый номер колонки
    /// </summary>
    public int OrdinalPosition { get; init; }

    /// <summary>
    /// Комментарий к колонке
    /// </summary>
    public string? Comment { get; init; }

    /// <summary>
    /// Является ли колонка пользовательским типом (ENUM, composite)
    /// </summary>
    public bool IsCustomType { get; init; }

    /// <summary>
    /// Имя пользовательского типа
    /// </summary>
    public string? CustomTypeName { get; init; }
}
