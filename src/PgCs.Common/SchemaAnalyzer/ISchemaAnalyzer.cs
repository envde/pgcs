namespace PgCs.Common.SchemaAnalyzer;

/// <summary>
/// Анализирует структуру PostgreSQL базы данных
/// </summary>
public interface ISchemaAnalyzer
{
    /// <summary>
    /// Анализирует полную схему базы данных
    /// </summary>
    /// <param name="connectionString">Строка подключения к PostgreSQL</param>
    /// <param name="schemaName">Имя схемы (по умолчанию "public")</param>
    /// <returns>Полная информация о схеме</returns>
    Task<DatabaseSchema> AnalyzeSchemaAsync(string connectionString, string schemaName = "public");

    /// <summary>
    /// Получает список всех таблиц в схеме
    /// </summary>
    Task<IReadOnlyList<TableInfo>> GetTablesAsync(string connectionString, string schemaName = "public");

    /// <summary>
    /// Получает детальную информацию о конкретной таблице
    /// </summary>
    Task<TableInfo> GetTableInfoAsync(string connectionString, string schemaName, string tableName);

    /// <summary>
    /// Получает все связи (foreign keys) между таблицами
    /// </summary>
    Task<IReadOnlyList<ForeignKeyInfo>> GetForeignKeysAsync(string connectionString, string schemaName = "public");

    /// <summary>
    /// Получает пользовательские типы (ENUMs, композитные типы)
    /// </summary>
    Task<IReadOnlyList<CustomTypeInfo>> GetCustomTypesAsync(string connectionString, string schemaName = "public");

    /// <summary>
    /// Получает все ENUM типы
    /// </summary>
    Task<IReadOnlyList<EnumTypeInfo>> GetEnumsAsync(string connectionString, string schemaName = "public");

    /// <summary>
    /// Получает индексы для таблицы
    /// </summary>
    Task<IReadOnlyList<IndexInfo>> GetIndexesAsync(string connectionString, string schemaName, string tableName);

    /// <summary>
    /// Получает constraints (проверки) для таблицы
    /// </summary>
    Task<IReadOnlyList<ConstraintInfo>> GetConstraintsAsync(string connectionString, string schemaName, string tableName);

    /// <summary>
    /// Получает представления (views)
    /// </summary>
    Task<IReadOnlyList<ViewInfo>> GetViewsAsync(string connectionString, string schemaName = "public");

    /// <summary>
    /// Получает хранимые процедуры и функции
    /// </summary>
    Task<IReadOnlyList<FunctionInfo>> GetFunctionsAsync(string connectionString, string schemaName = "public");
}
