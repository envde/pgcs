namespace PgCs.Common.CodeGeneration.Query.Models.Enums;

/// <summary>
/// Режим доступа к данным
/// </summary>
public enum DataAccessMode
{
    /// <summary>
    /// Использовать Npgsql напрямую (рекомендуется)
    /// </summary>
    Npgsql,

    /// <summary>
    /// Использовать Dapper поверх Npgsql
    /// </summary>
    Dapper,

    /// <summary>
    /// Использовать Entity Framework Core
    /// </summary>
    EntityFrameworkCore
}