using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaAnalyzer.Models.Types;

namespace PgCs.Common.SchemaAnalyzer;

public interface ISchemaFilter
{
    /// <summary>
    /// Исключить указанные схемы
    /// </summary>
    ISchemaFilter ExcludeSchemas(params string[] schemas);

    /// <summary>
    /// Включить только указанные схемы
    /// </summary>
    ISchemaFilter IncludeOnlySchemas(params string[] schemas);

    /// <summary>
    /// Исключить таблицы по regex паттерну
    /// </summary>
    ISchemaFilter ExcludeTables(string pattern);

    /// <summary>
    /// Исключить таблицы по нескольким паттернам
    /// </summary>
    ISchemaFilter ExcludeTables(params string[] patterns);

    /// <summary>
    /// Включить только таблицы, соответствующие паттерну
    /// </summary>
    ISchemaFilter IncludeOnlyTables(string pattern);

    /// <summary>
    /// Включить только таблицы, соответствующие паттернам
    /// </summary>
    ISchemaFilter IncludeOnlyTables(params string[] patterns);

    /// <summary>
    /// Исключить представления по regex паттерну
    /// </summary>
    ISchemaFilter ExcludeViews(string pattern);

    /// <summary>
    /// Включить только представления, соответствующие паттерну
    /// </summary>
    ISchemaFilter IncludeOnlyViews(string pattern);

    /// <summary>
    /// Включить только определённые виды типов
    /// </summary>
    ISchemaFilter IncludeOnlyTypes(params TypeKind[] kinds);

    /// <summary>
    /// Удалить системные объекты (pg_catalog, information_schema и т.д.)
    /// </summary>
    ISchemaFilter RemoveSystemObjects();

    /// <summary>
    /// Удалить все таблицы
    /// </summary>
    ISchemaFilter RemoveTables();

    /// <summary>
    /// Удалить все представления
    /// </summary>
    ISchemaFilter RemoveViews();

    /// <summary>
    /// Удалить все пользовательские типы
    /// </summary>
    ISchemaFilter RemoveTypes();

    /// <summary>
    /// Удалить все функции
    /// </summary>
    ISchemaFilter RemoveFunctions();

    /// <summary>
    /// Удалить все индексы
    /// </summary>
    ISchemaFilter RemoveIndexes();

    /// <summary>
    /// Удалить все триггеры
    /// </summary>
    ISchemaFilter RemoveTriggers();

    /// <summary>
    /// Удалить все ограничения (constraints)
    /// </summary>
    ISchemaFilter RemoveConstraints();

    /// <summary>
    /// Оставить только таблицы и представления
    /// </summary>
    ISchemaFilter OnlyTablesAndViews();

    /// <summary>
    /// Оставить только таблицы
    /// </summary>
    ISchemaFilter OnlyTables();

    /// <summary>
    /// Применяет фильтры и возвращает отфильтрованные метаданные
    /// </summary>
    SchemaMetadata Build();
}