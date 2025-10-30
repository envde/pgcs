using PgCs.Core.Schema.Analyzer;

namespace PgCs.SchemaAnalyzer.Tante;

/// <summary>
/// Расширения для удобного создания фильтров схемы
/// </summary>
public static class SchemaFilterBuilderExtensions
{
    /// <summary>
    /// Создать новый билдер фильтра схемы
    /// </summary>
    /// <returns>Новый экземпляр ISchemaFilterBuilder</returns>
    public static ISchemaFilterBuilder CreateFilter() => new SchemaFilterBuilder();
    
    /// <summary>
    /// Создать фильтр только для production схемы без системных объектов
    /// </summary>
    /// <returns>Готовый ISchemaFilter</returns>
    public static ISchemaFilter CreateProductionFilter()
    {
        return new SchemaFilterBuilder()
            .IncludeOnlySchemas("public")
            .ExcludeSystemObjects()
            .WithCommentParsing()
            .Build();
    }
    
    /// <summary>
    /// Создать фильтр для анализа только таблиц и представлений
    /// </summary>
    /// <returns>Готовый ISchemaFilter</returns>
    public static ISchemaFilter CreateTablesAndViewsFilter()
    {
        return new SchemaFilterBuilder()
            .OnlyTablesAndViews()
            .ExcludeSystemObjects()
            .Build();
    }
    
    /// <summary>
    /// Создать фильтр с исключением временных и тестовых таблиц
    /// </summary>
    /// <returns>Готовый ISchemaFilter</returns>
    public static ISchemaFilter CreateCleanSchemaFilter()
    {
        return new SchemaFilterBuilder()
            .ExcludeSystemObjects()
            .ExcludeTables("^temp_.*", "^test_.*", ".*_backup$", ".*_old$")
            .ExcludeViews("^temp_.*", "^test_.*")
            .WithCommentParsing()
            .Build();
    }
}
