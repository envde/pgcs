namespace PgCs.Common.QueryAnalyzer.Models.Metadata;

/// <summary>
/// Тип SQL запроса
/// </summary>
public enum QueryType
{
    Select,
    Insert,
    Update,
    Delete,
    Unknown
}
