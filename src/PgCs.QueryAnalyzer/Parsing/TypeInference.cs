namespace PgCs.QueryAnalyzer.Parsing;

internal static class TypeInference
{
    /// <summary>
    /// Определяет тип параметра из контекста SQL
    /// </summary>
    public static (string PostgresType, string CSharpType, bool IsNullable) InferParameterType(
        string sqlQuery, string paramName)
    {
        var upper = sqlQuery.ToUpperInvariant();
        var paramUpper = paramName.ToUpperInvariant();
        
        // Явное приведение типа
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
        
        // По умолчанию
        return ("text", "string", false);
    }

    /// <summary>
    /// Определяет тип колонки из SQL выражения
    /// </summary>
    public static (string PostgresType, string CSharpType) InferColumnType(string columnExpression)
    {
        var upper = columnExpression.ToUpperInvariant();
        
        if (upper.Contains("COUNT(") || upper.Contains("SUM("))
            return ("bigint", "long");
        
        if (upper.Contains("AVG("))
            return ("numeric", "decimal");
        
        if (upper.Contains("NOW(") || upper.Contains("CURRENT_TIMESTAMP"))
            return ("timestamp", "DateTime");

        if (upper.Contains("::INT"))
            return ("integer", "int");

        if (upper.Contains("::BIGINT"))
            return ("bigint", "long");

        if (upper.Contains("::BOOLEAN") || upper.Contains("::BOOL"))
            return ("boolean", "bool");

        if (upper.Contains("::UUID"))
            return ("uuid", "Guid");
        
        // По умолчанию
        return ("text", "string");
    }

    private static bool ContainsTypeCast(string query, string paramName, string type)
    {
        return query.Contains($"${paramName}::{type}") || 
               query.Contains($"@{paramName}::{type}");
    }
}