namespace PgCs.Common.CodeGeneration;

/// <summary>
/// Стратегия именования типов и свойств
/// </summary>
public enum NamingStrategy
{
    /// <summary>
    /// PascalCase (MyTableName, MyProperty)
    /// </summary>
    PascalCase,

    /// <summary>
    /// camelCase (myTableName, myProperty)
    /// </summary>
    CamelCase,

    /// <summary>
    /// Использовать оригинальные имена без преобразования
    /// </summary>
    Original
}
