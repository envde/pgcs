namespace PgCs.Common.SchemaAnalyzer.Models.Types;

/// <summary>
/// Вид пользовательского типа данных в PostgreSQL
/// </summary>
public enum TypeKind
{
    /// <summary>
    /// ENUM - перечисление с фиксированным набором значений
    /// </summary>
    Enum,

    /// <summary>
    /// Composite - составной тип с несколькими именованными полями
    /// </summary>
    Composite,

    /// <summary>
    /// Domain - ограниченный базовый тип с дополнительными проверками
    /// </summary>
    Domain,

    /// <summary>
    /// Range - тип диапазона значений (например, daterange, int4range)
    /// </summary>
    Range
}