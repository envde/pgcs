namespace PgCs.Common.QueryAnalyzer.Models.Results;

/// <summary>
/// Кардинальность возвращаемого значения (sqlc style)
/// </summary>
public enum ReturnCardinality
{
    /// <summary>
    /// Возвращает один объект или null (:one)
    /// </summary>
    One,

    /// <summary>
    /// Возвращает список объектов (:many)
    /// </summary>
    Many,

    /// <summary>
    /// Ничего не возвращает, только выполняет (:exec)
    /// </summary>
    Exec,

    /// <summary>
    /// Возвращает количество затронутых строк (:execrows)
    /// </summary>
    ExecRows
}
