namespace PgCs.Common.SchemaAnalyzer.Models.Indexes;

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