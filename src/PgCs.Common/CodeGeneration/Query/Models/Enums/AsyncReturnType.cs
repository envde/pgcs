namespace PgCs.Common.CodeGeneration.Query.Models.Enums;

/// <summary>
/// Тип возвращаемого значения для асинхронных методов
/// </summary>
public enum AsyncReturnType
{
    /// <summary>
    /// Task<T>
    /// </summary>
    Task,

    /// <summary>
    /// ValueTask<T> (рекомендуется)
    /// </summary>
    ValueTask
}