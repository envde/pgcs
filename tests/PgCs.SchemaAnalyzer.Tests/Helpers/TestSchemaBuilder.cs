namespace PgCs.SchemaAnalyzer.Tests.Helpers;

/// <summary>
/// Builder для создания тестовых данных схемы
/// </summary>
public static class TestSchemaBuilder
{
    /// <summary>
    /// Создать простую таблицу с указанными колонками
    /// </summary>
    public static string CreateSimpleTable(string tableName, params (string name, string type)[] columns)
    {
        var columnsDef = string.Join(",\n    ", 
            columns.Select(c => $"{c.name} {c.type}"));
        
        return $"CREATE TABLE {tableName} (\n    {columnsDef}\n);";
    }

    /// <summary>
    /// Создать ENUM тип
    /// </summary>
    public static string CreateEnumType(string typeName, params string[] values)
    {
        var valuesList = string.Join(", ", values.Select(v => $"'{v}'"));
        return $"CREATE TYPE {typeName} AS ENUM ({valuesList});";
    }

    /// <summary>
    /// Создать индекс
    /// </summary>
    public static string CreateIndex(string indexName, string tableName, params string[] columns)
    {
        var columnsList = string.Join(", ", columns);
        return $"CREATE INDEX {indexName} ON {tableName}({columnsList});";
    }

    /// <summary>
    /// Создать функцию
    /// </summary>
    public static string CreateFunction(string functionName, string returnType, string body)
    {
        return $@"CREATE FUNCTION {functionName}()
RETURNS {returnType}
LANGUAGE plpgsql
AS $$
{body}
$$;";
    }

    /// <summary>
    /// Создать триггер
    /// </summary>
    public static string CreateTrigger(string triggerName, string tableName, string functionName, 
        string timing = "BEFORE", string events = "INSERT")
    {
        return $"CREATE TRIGGER {triggerName} {timing} {events} ON {tableName} FOR EACH ROW EXECUTE FUNCTION {functionName}();";
    }
}
