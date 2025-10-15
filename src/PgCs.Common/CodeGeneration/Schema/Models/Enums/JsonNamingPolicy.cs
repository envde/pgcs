namespace PgCs.Common.CodeGeneration.Schema.Models.Enums;

/// <summary>
/// Политика именования свойств JSON
/// </summary>
public enum JsonNamingPolicy
{
    /// <summary>
    /// camelCase (userName)
    /// </summary>
    CamelCase,

    /// <summary>
    /// PascalCase (UserName)
    /// </summary>
    PascalCase,

    /// <summary>
    /// snake_case (user_name)
    /// </summary>
    SnakeCase,

    /// <summary>
    /// kebab-case (user-name)
    /// </summary>
    KebabCase,

    /// <summary>
    /// Не изменять имя
    /// </summary>
    None
}