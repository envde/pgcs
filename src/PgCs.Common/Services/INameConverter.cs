namespace PgCs.Common.Services;

/// <summary>
/// Конвертер имен из PostgreSQL конвенций в C# конвенции
/// </summary>
public interface INameConverter
{
    /// <summary>
    /// Преобразует имя таблицы в имя класса (PascalCase, singular)
    /// Например: user_profiles -> UserProfile
    /// </summary>
    string ToClassName(string tableName);

    /// <summary>
    /// Преобразует имя колонки в имя свойства (PascalCase)
    /// Например: first_name -> FirstName
    /// </summary>
    string ToPropertyName(string columnName);

    /// <summary>
    /// Преобразует имя enum значения в C# стиль
    /// Например: ACTIVE_USER -> ActiveUser
    /// </summary>
    string ToEnumMemberName(string enumValue);

    /// <summary>
    /// Преобразует имя функции в имя метода (PascalCase)
    /// Например: get_user_by_id -> GetUserById
    /// </summary>
    string ToMethodName(string functionName);

    /// <summary>
    /// Преобразует имя параметра функции в имя параметра метода (camelCase)
    /// Например: user_id -> userId
    /// </summary>
    string ToParameterName(string parameterName);
}
