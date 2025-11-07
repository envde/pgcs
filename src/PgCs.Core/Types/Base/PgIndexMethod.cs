namespace PgCs.Core.Types.Base;

/// <summary>
/// Метод индексирования PostgreSQL
/// </summary>
public enum PgIndexMethod
{
    /// <summary>
    /// B-дерево - стандартный метод индексирования для большинства типов данных
    /// Поддерживает операции сравнения (<, <=, =, >=, >) и сортировку
    /// </summary>
    BTree,

    /// <summary>
    /// Hash - быстрый поиск только по равенству (=)
    /// Не поддерживает диапазоны и сортировку
    /// </summary>
    Hash,

    /// <summary>
    /// GiST (Generalized Search Tree) - для геометрических и полнотекстовых данных
    /// Поддерживает пользовательские типы данных и операторы
    /// </summary>
    Gist,

    /// <summary>
    /// GIN (Generalized Inverted Index) - для составных значений
    /// Идеален для массивов, JSONB, полнотекстового поиска
    /// </summary>
    Gin,

    /// <summary>
    /// SP-GiST (Space-Partitioned GiST) - для несбалансированных структур данных
    /// Подходит для quad-trees, k-d trees и других пространственных структур
    /// </summary>
    SpGist,

    /// <summary>
    /// BRIN (Block Range Index) - компактный индекс для очень больших таблиц
    /// Хранит сводную информацию о диапазонах страниц
    /// </summary>
    Brin,

    /// <summary>
    /// Bloom - вероятностная структура данных для проверки множественных условий равенства
    /// </summary>
    /// <remarks>
    /// Требует расширение: CREATE EXTENSION bloom
    /// Полезен для таблиц с множеством колонок, где запросы используют разные комбинации
    /// </remarks>
    Bloom
}
