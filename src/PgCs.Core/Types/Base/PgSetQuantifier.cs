namespace PgCs.Core.Types.Base;

/// <summary>
/// Квантификатор множества для SELECT запроса
/// Определяет как обрабатываются дубликаты в результате
/// </summary>
public enum PgSetQuantifier
{
    /// <summary>Возвращать все строки включая дубликаты (по умолчанию)</summary>
    All,

    /// <summary>Возвращать только уникальные строки: SELECT DISTINCT</summary>
    Distinct,

    /// <summary>Возвращать уникальные строки по указанным колонкам: SELECT DISTINCT ON (columns)</summary>
    DistinctOn
}
