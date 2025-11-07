namespace PgCs.Core.Types.Base;

/// <summary>
/// Направление сортировки в ORDER BY клаузе
/// </summary>
public enum PgOrderDirection
{
    /// <summary>По возрастанию (по умолчанию): ASC</summary>
    Ascending,

    /// <summary>По убыванию: DESC</summary>
    Descending
}
