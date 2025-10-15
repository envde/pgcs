namespace PgCs.Common.CodeGeneration.Schema.Models.Enums;

/// <summary>
/// Режим преобразования множественного числа
/// </summary>
public enum PluralizeMode
{
    /// <summary>
    /// Не изменять имя
    /// </summary>
    None,

    /// <summary>
    /// Преобразовать во единственное число (users -> User)
    /// </summary>
    Singularize,

    /// <summary>
    /// Преобразовать во множественное число (user -> Users)
    /// </summary>
    Pluralize
}