using PgCs.Core.Definitions.Schema.Base;

namespace PgCs.Core.SchemaAnalyzer;

/// <summary>
/// Fluent API для построения настроек фильтрации схемы
/// </summary>
public interface ISchemaFilterBuilder
{
    /// <summary>
    /// Исключить указанные схемы
    /// </summary>
    ISchemaFilterBuilder ExcludeSchemas(params string[] schemas);

    /// <summary>
    /// Включить только указанные схемы
    /// </summary>
    ISchemaFilterBuilder IncludeOnlySchemas(params string[] schemas);

    /// <summary>
    /// Исключить таблицы по regex паттернам
    /// </summary>
    ISchemaFilterBuilder ExcludeTables(params string[] patterns);

    /// <summary>
    /// Включить только таблицы, соответствующие паттернам
    /// </summary>
    ISchemaFilterBuilder IncludeOnlyTables(params string[] patterns);

    /// <summary>
    /// Исключить представления по regex паттернам
    /// </summary>
    ISchemaFilterBuilder ExcludeViews(params string[] patterns);

    /// <summary>
    /// Включить только представления, соответствующие паттернам
    /// </summary>
    ISchemaFilterBuilder IncludeOnlyViews(params string[] patterns);

    /// <summary>
    /// Включить только определённые виды типов
    /// </summary>
    ISchemaFilterBuilder IncludeOnlyTypes(params TypeKind[] kinds);

    /// <summary>
    /// Исключить системные объекты (pg_catalog, information_schema и т.д.)
    /// </summary>
    ISchemaFilterBuilder ExcludeSystemObjects();

    /// <summary>
    /// Указать, какие объекты анализировать
    /// </summary>
    ISchemaFilterBuilder WithObjects(params SchemaObjectType[] objectTypes);

    /// <summary>
    /// Анализировать только таблицы
    /// </summary>
    ISchemaFilterBuilder OnlyTables();

    /// <summary>
    /// Анализировать только таблицы и представления
    /// </summary>
    ISchemaFilterBuilder OnlyTablesAndViews();

    /// <summary>
    /// Установить максимальную глубину анализа зависимостей (0 = не анализировать)
    /// </summary>
    ISchemaFilterBuilder WithDependencyDepth(int depth);

    /// <summary>
    /// Включить строгий режим - генерировать ошибки для неизвестных блоков
    /// </summary>
    ISchemaFilterBuilder WithStrictMode(bool enabled = true);

    /// <summary>
    /// Включить или отключить парсинг комментариев
    /// </summary>
    ISchemaFilterBuilder WithCommentParsing(bool enabled = true);

    /// <summary>
    /// Построить объект фильтра
    /// </summary>
    ISchemaFilter Build();
}