namespace PgCs.Common.SchemaAnalyzer.Models.Indexes;

/// <summary>
/// Метод индексирования PostgreSQL
/// </summary>
public enum IndexMethod
{
    /// <summary>
    /// B-дерево - стандартный метод индексирования для большинства типов данных
    /// </summary>
    BTree,

    /// <summary>
    /// Hash - быстрый поиск по равенству, не поддерживает диапазоны
    /// </summary>
    Hash,

    /// <summary>
    /// GiST (Generalized Search Tree) - для геометрических и полнотекстовых данных
    /// </summary>
    Gist,

    /// <summary>
    /// GIN (Generalized Inverted Index) - для составных значений (массивы, JSONB, полнотекстовый поиск)
    /// </summary>
    Gin,

    /// <summary>
    /// SP-GiST (Space-Partitioned GiST) - для несбалансированных структур данных
    /// </summary>
    SpGist,

    /// <summary>
    /// BRIN (Block Range Index) - компактный индекс для очень больших таблиц
    /// </summary>
    Brin
}