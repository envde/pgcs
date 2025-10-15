namespace PgCs.Common.SchemaAnalyzer;

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