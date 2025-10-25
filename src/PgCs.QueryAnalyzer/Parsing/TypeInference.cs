namespace PgCs.QueryAnalyzer.Parsing;

/// <summary>
/// Вывод типов данных для параметров и колонок на основе контекста SQL запроса
/// </summary>
internal static class TypeInference
{
    /// <summary>
    /// Определяет PostgreSQL и C# тип параметра на основе явного приведения типа в SQL
    /// </summary>
    /// <param name="sqlQuery">SQL запрос с контекстом использования параметра</param>
    /// <param name="paramName">Имя параметра для анализа</param>
    /// <returns>Кортеж (PostgreSQL тип, C# тип, nullable)</returns>
    public static (string PostgresType, string CSharpType, bool IsNullable) InferParameterType(
        string sqlQuery, string paramName)
    {
        var upper = sqlQuery.ToUpperInvariant();
        var paramUpper = paramName.ToUpperInvariant();
        
        // Явное приведение типа ::type
        if (ContainsTypeCast(upper, paramUpper, "INT"))
            return ("integer", "int", false);
        
        if (ContainsTypeCast(upper, paramUpper, "BIGINT"))
            return ("bigint", "long", false);
        
        if (ContainsTypeCast(upper, paramUpper, "TIMESTAMP"))
            return ("timestamp", "DateTime", false);
        
        if (ContainsTypeCast(upper, paramUpper, "BOOLEAN") || ContainsTypeCast(upper, paramUpper, "BOOL"))
            return ("boolean", "bool", false);

        if (ContainsTypeCast(upper, paramUpper, "UUID"))
            return ("uuid", "Guid", false);

        if (ContainsTypeCast(upper, paramUpper, "DECIMAL") || ContainsTypeCast(upper, paramUpper, "NUMERIC"))
            return ("numeric", "decimal", false);
        
        // По умолчанию string (самый безопасный вариант)
        return ("text", "string", false);
    }

    /// <summary>
    /// Определяет тип колонки из SQL выражения (агрегатные функции, приведение типов)
    /// </summary>
    /// <param name="columnExpression">SQL выражение колонки</param>
    /// <returns>Кортеж (PostgreSQL тип, C# тип)</returns>
    public static (string PostgresType, string CSharpType) InferColumnType(string columnExpression)
    {
        var upper = columnExpression.ToUpperInvariant();
        
        // Агрегатные функции
        if (upper.Contains("COUNT(") || upper.Contains("SUM("))
            return ("bigint", "long");
        
        if (upper.Contains("AVG("))
            return ("numeric", "decimal");
        
        // Функции даты/времени
        if (upper.Contains("NOW(") || upper.Contains("CURRENT_TIMESTAMP"))
            return ("timestamp", "DateTime");

        // Явное приведение типов
        if (upper.Contains("::INT"))
            return ("integer", "int");

        if (upper.Contains("::BIGINT"))
            return ("bigint", "long");

        if (upper.Contains("::BOOLEAN") || upper.Contains("::BOOL"))
            return ("boolean", "bool");

        if (upper.Contains("::UUID"))
            return ("uuid", "Guid");

        if (upper.Contains("::NUMERIC") || upper.Contains("::DECIMAL"))
            return ("numeric", "decimal");

        if (upper.Contains("::TEXT") || upper.Contains("::VARCHAR"))
            return ("text", "string");
        
        // По умолчанию string
        return ("text", "string");
    }

    /// <summary>
    /// Проверяет наличие явного приведения типа для параметра ($param::type)
    /// </summary>
    private static bool ContainsTypeCast(string query, string paramName, string type)
    {
        return query.Contains($"${paramName}::{type}") || 
               query.Contains($"@{paramName}::{type}");
    }
}