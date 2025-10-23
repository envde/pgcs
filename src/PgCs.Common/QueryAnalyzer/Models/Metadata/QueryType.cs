namespace PgCs.Common.QueryAnalyzer.Models.Metadata;

/// <summary>
/// Тип SQL запроса (определяется по первому ключевому слову)
/// </summary>
public enum QueryType
{
    /// <summary>
    /// SELECT - запрос для чтения данных
    /// </summary>
    Select,

    /// <summary>
    /// INSERT - запрос для добавления новых данных
    /// </summary>
    Insert,

    /// <summary>
    /// UPDATE - запрос для изменения существующих данных
    /// </summary>
    Update,

    /// <summary>
    /// DELETE - запрос для удаления данных
    /// </summary>
    Delete,

    /// <summary>
    /// Unknown - неопределенный или неподдерживаемый тип запроса
    /// </summary>
    Unknown
}
