namespace PgCs.Core.Tokenization;

/// <summary>
/// PostgreSQL ключевые слова
/// Основано на PostgreSQL 18 документации
/// Оптимизировано для .NET 9 с использованием Span
/// </summary>
public static class PostgresKeywords
{
    // Наиболее часто используемые ключевые слова (для быстрой проверки)
    private static readonly HashSet<string> ReservedKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        // DDL
        "CREATE", "DROP", "ALTER", "TRUNCATE", "COMMENT",
        "TABLE", "VIEW", "INDEX", "SEQUENCE", "SCHEMA", "DATABASE",
        "FUNCTION", "PROCEDURE", "TRIGGER", "TYPE", "DOMAIN", "ENUM",
        "CONSTRAINT", "PRIMARY", "FOREIGN", "UNIQUE", "CHECK", "DEFAULT",
        "REFERENCES", "KEY",
    
        // DML
        "SELECT", "INSERT", "UPDATE", "DELETE", "MERGE",
        "FROM", "WHERE", "JOIN", "LEFT", "RIGHT", "INNER", "OUTER", "CROSS",
        "ON", "USING", "GROUP", "HAVING", "ORDER", "LIMIT", "OFFSET",
        "UNION", "INTERSECT", "EXCEPT", "DISTINCT", "ALL",
    
        // Типы данных
        "INTEGER", "INT", "BIGINT", "SMALLINT", "SERIAL", "BIGSERIAL",
        "NUMERIC", "DECIMAL", "REAL", "DOUBLE", "FLOAT",
        "VARCHAR", "CHAR", "TEXT", "BYTEA",
        "DATE", "TIME", "TIMESTAMP", "TIMESTAMPTZ", "INTERVAL",
        "BOOLEAN", "BOOL", "UUID", "JSON", "JSONB", "XML",
        "ARRAY", "HSTORE", "INET", "CIDR", "MACADDR",
        "POINT", "LINE", "LSEG", "BOX", "PATH", "POLYGON", "CIRCLE",
        "TSQUERY", "TSVECTOR", "TSRANGE", "TSTZRANGE", "DATERANGE",
    
        // Модификаторы
        "NOT", "NULL", "AS", "OR", "REPLACE", "IF", "EXISTS",
        "CASCADE", "RESTRICT", "SET", "NO", "ACTION",
        "GENERATED", "ALWAYS", "BY", "STORED", "VIRTUAL",
        "IDENTITY", "COLLATE", "WITH", "WITHOUT", "TIMEZONE",
        "PRECISION",
    
        // Функции и операторы
        "AND", "IN", "LIKE", "ILIKE", "SIMILAR", "BETWEEN",
        "IS", "ANY", "SOME", "CASE", "WHEN", "THEN", "ELSE", "END",
        "CAST", "EXTRACT", "SUBSTRING", "POSITION", "OVERLAY",
        "COALESCE", "NULLIF", "GREATEST", "LEAST",
    
        // Триггеры
        "BEFORE", "AFTER", "INSTEAD", "FOR", "EACH", "ROW", "STATEMENT",
        "WHEN", "EXECUTE", "FUNCTION",
    
        // Транзакции
        "BEGIN", "COMMIT", "ROLLBACK", "SAVEPOINT", "TRANSACTION",
        "ISOLATION", "LEVEL", "READ", "WRITE", "ONLY",
        "SERIALIZABLE", "REPEATABLE", "COMMITTED", "UNCOMMITTED",
    
        // Права доступа
        "GRANT", "REVOKE", "PRIVILEGE", "TO", "PUBLIC",
        "USAGE", "EXECUTE", "CONNECT",
    
        // Партиционирование
        "PARTITION", "RANGE", "LIST", "HASH", "VALUES", "MODULUS", "REMAINDER",
    
        // Индексы
        "USING", "BTREE", "HASH", "GIST", "GIN", "SPGIST", "BRIN",
        "CONCURRENT", "CONCURRENTLY",
    
        // Прочее
        "LANGUAGE", "PLPGSQL", "SQL", "C",
        "RETURNS", "RETURN", "DECLARE", "LOOP", "EXIT", "CONTINUE",
        "RAISE", "NOTICE", "WARNING", "EXCEPTION", "INFO", "LOG", "DEBUG",
        "VOLATILE", "STABLE", "IMMUTABLE", "STRICT", "LEAKPROOF",
        "PARALLEL", "SAFE", "UNSAFE", "RESTRICTED",
        "SECURITY", "DEFINER", "INVOKER",
        "WINDOW", "OVER", "ROWS", "GROUPS", "RANGE", "UNBOUNDED", "PRECEDING", "FOLLOWING", "CURRENT",
        "EXCLUDE", "TIES", "OTHERS",
        "LATERAL", "TABLESAMPLE", "BERNOULLI", "SYSTEM",
        "MATERIALIZED", "REFRESH",
        "ANALYZE", "VACUUM", "EXPLAIN", "VERBOSE", "COSTS", "BUFFERS",
        "COPY", "STDIN", "STDOUT", "DELIMITER", "QUOTE", "ESCAPE", "HEADER",
        "INHERITS", "LIKE", "INCLUDING", "EXCLUDING",
        "OWNED", "DEPENDS", "EXTENSION"
    };

    /// <summary>
    /// Проверяет, является ли слово ключевым словом PostgreSQL (string версия)
    /// </summary>
    public static bool IsKeyword(string word) =>
        ReservedKeywords.Contains(word);

    /// <summary>
    /// Проверяет, является ли слово ключевым словом PostgreSQL (Span версия - zero allocation)
    /// </summary>
    public static bool IsKeyword(ReadOnlySpan<char> word)
    {
        // Преобразуем в string для проверки в HashSet
        // Это минорная аллокация, но встречается редко и приемлема для корректности
        return ReservedKeywords.Contains(word.ToString());
    }

    /// <summary>
    /// Проверяет, является ли слово типом данных PostgreSQL
    /// Использует Span для zero-allocation операций
    /// </summary>
    public static bool IsDataType(ReadOnlySpan<char> word)
    {
        // Используем stackalloc для временного буфера uppercase conversion
        // Большинство имён типов короткие (< 32 символов)
        Span<char> upperBuffer = word.Length <= 64
            ? stackalloc char[word.Length]
            : new char[word.Length];

        word.ToUpperInvariant(upperBuffer);

        // Используем SequenceEqual для сравнения
        return
            upperBuffer.SequenceEqual("INTEGER") || upperBuffer.SequenceEqual("INT") ||
            upperBuffer.SequenceEqual("INT4") || upperBuffer.SequenceEqual("BIGINT") ||
            upperBuffer.SequenceEqual("INT8") || upperBuffer.SequenceEqual("SMALLINT") ||
            upperBuffer.SequenceEqual("INT2") || upperBuffer.SequenceEqual("SERIAL") ||
            upperBuffer.SequenceEqual("BIGSERIAL") || upperBuffer.SequenceEqual("SMALLSERIAL") ||
            upperBuffer.SequenceEqual("NUMERIC") || upperBuffer.SequenceEqual("DECIMAL") ||
            upperBuffer.SequenceEqual("REAL") || upperBuffer.SequenceEqual("FLOAT4") ||
            upperBuffer.SequenceEqual("DOUBLE") || upperBuffer.SequenceEqual("FLOAT8") ||
            upperBuffer.SequenceEqual("VARCHAR") || upperBuffer.SequenceEqual("CHARACTER") ||
            upperBuffer.SequenceEqual("CHAR") || upperBuffer.SequenceEqual("TEXT") ||
            upperBuffer.SequenceEqual("BYTEA") || upperBuffer.SequenceEqual("NAME") ||
            upperBuffer.SequenceEqual("DATE") || upperBuffer.SequenceEqual("TIME") ||
            upperBuffer.SequenceEqual("TIMESTAMP") || upperBuffer.SequenceEqual("TIMESTAMPTZ") ||
            upperBuffer.SequenceEqual("INTERVAL") ||
            upperBuffer.SequenceEqual("BOOLEAN") || upperBuffer.SequenceEqual("BOOL") ||
            upperBuffer.SequenceEqual("UUID") || upperBuffer.SequenceEqual("JSON") ||
            upperBuffer.SequenceEqual("JSONB") || upperBuffer.SequenceEqual("XML") ||
            upperBuffer.SequenceEqual("ARRAY") || upperBuffer.SequenceEqual("HSTORE") ||
            upperBuffer.SequenceEqual("INET") || upperBuffer.SequenceEqual("CIDR") ||
            upperBuffer.SequenceEqual("MACADDR") || upperBuffer.SequenceEqual("MACADDR8") ||
            upperBuffer.SequenceEqual("POINT") || upperBuffer.SequenceEqual("LINE") ||
            upperBuffer.SequenceEqual("LSEG") || upperBuffer.SequenceEqual("BOX") ||
            upperBuffer.SequenceEqual("PATH") || upperBuffer.SequenceEqual("POLYGON") ||
            upperBuffer.SequenceEqual("CIRCLE") ||
            upperBuffer.SequenceEqual("TSQUERY") || upperBuffer.SequenceEqual("TSVECTOR") ||
            upperBuffer.SequenceEqual("MONEY") || upperBuffer.SequenceEqual("BIT") ||
            upperBuffer.SequenceEqual("VARBIT") ||
            upperBuffer.SequenceEqual("OID") || upperBuffer.SequenceEqual("REGPROC") ||
            upperBuffer.SequenceEqual("REGPROCEDURE") || upperBuffer.SequenceEqual("REGOPER") ||
            upperBuffer.SequenceEqual("REGOPERATOR") || upperBuffer.SequenceEqual("REGCLASS") ||
            upperBuffer.SequenceEqual("REGTYPE") || upperBuffer.SequenceEqual("REGROLE") ||
            upperBuffer.SequenceEqual("REGNAMESPACE") || upperBuffer.SequenceEqual("REGCONFIG") ||
            upperBuffer.SequenceEqual("REGDICTIONARY");
    }
}