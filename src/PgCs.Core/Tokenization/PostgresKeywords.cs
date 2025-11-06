namespace PgCs.Core.Tokenization;

/// <summary>
/// PostgreSQL ключевые слова
/// Основано на PostgreSQL 18 документации
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
    /// Проверяет, является ли слово ключевым словом PostgreSQL
    /// </summary>
    public static bool IsKeyword(string word) => 
        ReservedKeywords.Contains(word);

    /// <summary>
    /// Проверяет, является ли слово типом данных PostgreSQL
    /// </summary>
    public static bool IsDataType(string word)
    {
        return word.ToUpperInvariant() switch
        {
            "INTEGER" or "INT" or "INT4" or "BIGINT" or "INT8" or "SMALLINT" or "INT2" or
            "SERIAL" or "BIGSERIAL" or "SMALLSERIAL" or
            "NUMERIC" or "DECIMAL" or "REAL" or "FLOAT4" or "DOUBLE" or "FLOAT8" or
            "VARCHAR" or "CHARACTER" or "CHAR" or "TEXT" or "BYTEA" or "NAME" or
            "DATE" or "TIME" or "TIMESTAMP" or "TIMESTAMPTZ" or "INTERVAL" or
            "BOOLEAN" or "BOOL" or "UUID" or "JSON" or "JSONB" or "XML" or
            "ARRAY" or "HSTORE" or "INET" or "CIDR" or "MACADDR" or "MACADDR8" or
            "POINT" or "LINE" or "LSEG" or "BOX" or "PATH" or "POLYGON" or "CIRCLE" or
            "TSQUERY" or "TSVECTOR" or "MONEY" or "BIT" or "VARBIT" or
            "OID" or "REGPROC" or "REGPROCEDURE" or "REGOPER" or "REGOPERATOR" or
            "REGCLASS" or "REGTYPE" or "REGROLE" or "REGNAMESPACE" or "REGCONFIG" or "REGDICTIONARY" => true,
            _ => false
        };
    }
}