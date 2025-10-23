using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaAnalyzer.Models.Types;

namespace PgCs.Common.SchemaAnalyzer;

public interface ISchemaFilter
{
    /// <summary>
    /// Исключить указанные схемы
    /// </summary>
    SchemaFilter ExcludeSchemas(params string[] schemas);

    /// <summary>
    /// Включить только указанные схемы
    /// </summary>
    SchemaFilter IncludeOnlySchemas(params string[] schemas);

    /// <summary>
    /// Исключить таблицы по regex паттерну
    /// </summary>
    SchemaFilter ExcludeTables(string pattern);

    /// <summary>
    /// Исключить таблицы по нескольким паттернам
    /// </summary>
    SchemaFilter ExcludeTables(params string[] patterns);

    /// <summary>
    /// Включить только таблицы, соответствующие паттерну
    /// </summary>
    SchemaFilter IncludeOnlyTables(string pattern);

    /// <summary>
    /// Включить только таблицы, соответствующие паттернам
    /// </summary>
    SchemaFilter IncludeOnlyTables(params string[] patterns);

    /// <summary>
    /// Исключить представления по regex паттерну
    /// </summary>
    SchemaFilter ExcludeViews(string pattern);

    /// <summary>
    /// Включить только представления, соответствующие паттерну
    /// </summary>
    SchemaFilter IncludeOnlyViews(string pattern);

    /// <summary>
    /// Включить только определённые виды типов
    /// </summary>
    SchemaFilter IncludeOnlyTypes(params TypeKind[] kinds);

    /// <summary>
    /// Удалить системные объекты (pg_catalog, information_schema и т.д.)
    /// </summary>
    SchemaFilter RemoveSystemObjects();

    /// <summary>
    /// Удалить все таблицы
    /// </summary>
    SchemaFilter RemoveTables();

    /// <summary>
    /// Удалить все представления
    /// </summary>
    SchemaFilter RemoveViews();

    /// <summary>
    /// Удалить все пользовательские типы
    /// </summary>
    SchemaFilter RemoveTypes();

    /// <summary>
    /// Удалить все функции
    /// </summary>
    SchemaFilter RemoveFunctions();

    /// <summary>
    /// Удалить все индексы
    /// </summary>
    SchemaFilter RemoveIndexes();

    /// <summary>
    /// Удалить все триггеры
    /// </summary>
    SchemaFilter RemoveTriggers();

    /// <summary>
    /// Удалить все ограничения (constraints)
    /// </summary>
    SchemaFilter RemoveConstraints();

    /// <summary>
    /// Оставить только таблицы и представления
    /// </summary>
    SchemaFilter OnlyTablesAndViews();

    /// <summary>
    /// Оставить только таблицы
    /// </summary>
    SchemaFilter OnlyTables();

    /// <summary>
    /// Применяет фильтры и возвращает отфильтрованные метаданные
    /// </summary>
    SchemaMetadata Build();
}