namespace PgCs.Common.Generation.Models;

/// <summary>
/// Тип генерируемой модели
/// </summary>
public enum ModelType
{
    /// <summary>
    /// Модель таблицы базы данных
    /// </summary>
    Table,

    /// <summary>
    /// Модель представления (VIEW)
    /// </summary>
    View,

    /// <summary>
    /// Перечисление (ENUM)
    /// </summary>
    Enum,

    /// <summary>
    /// Пользовательский тип (DOMAIN, COMPOSITE)
    /// </summary>
    CustomType,

    /// <summary>
    /// Параметры функции/процедуры
    /// </summary>
    FunctionParameters,

    /// <summary>
    /// Результат запроса (query result model)
    /// </summary>
    QueryResult,

    /// <summary>
    /// Параметры запроса (query parameters model)
    /// </summary>
    QueryParameters
}
