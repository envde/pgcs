namespace PgCs.Common.SchemaAnalyzer.Models.Enums;

/// <summary>
/// Метод индексирования PostgreSQL
/// </summary>
public enum IndexMethod
{
    BTree,
    Hash,
    Gist,
    Gin,
    SpGist,
    Brin
}