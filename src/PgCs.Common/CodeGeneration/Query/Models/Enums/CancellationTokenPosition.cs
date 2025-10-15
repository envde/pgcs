namespace PgCs.Common.CodeGeneration.Query.Models.Enums;

/// <summary>
/// Позиция CancellationToken в списке параметров метода
/// </summary>
public enum CancellationTokenPosition
{
    /// <summary>
    /// Первый параметр
    /// </summary>
    First,

    /// <summary>
    /// Последний параметр (рекомендуется)
    /// </summary>
    Last,

    /// <summary>
    /// Перед параметрами со значениями по умолчанию
    /// </summary>
    BeforeOptional
}