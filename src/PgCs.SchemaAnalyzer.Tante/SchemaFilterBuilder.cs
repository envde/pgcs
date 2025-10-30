using System.Text.RegularExpressions;
using PgCs.Core.Schema.Analyzer;
using PgCs.Core.Schema.Common;

namespace PgCs.SchemaAnalyzer.Tante;

/// <summary>
/// Fluent API билдер для построения настроек фильтрации схемы базы данных
/// <para>
/// Примеры использования:
/// <code>
/// // Простая фильтрация - только public схема без системных объектов
/// var filter = new SchemaFilterBuilder()
///     .IncludeOnlySchemas("public")
///     .ExcludeSystemObjects()
///     .Build();
/// 
/// // Фильтрация с паттернами - исключаем временные таблицы
/// var filter = new SchemaFilterBuilder()
///     .ExcludeTables("^temp_.*", ".*_backup$")
///     .ExcludeViews("^test_.*")
///     .WithCommentParsing(true)
///     .Build();
/// 
/// // Только таблицы и представления
/// var filter = new SchemaFilterBuilder()
///     .OnlyTablesAndViews()
///     .ExcludeSystemObjects()
///     .Build();
/// 
/// // Фильтрация типов - только ENUM и Domain
/// var filter = new SchemaFilterBuilder()
///     .WithObjects(SchemaObjectType.Types)
///     .IncludeOnlyTypes(TypeKind.Enum, TypeKind.Domain)
///     .Build();
/// 
/// // Применение фильтра к метаданным
/// var filteredMetadata = filter.ApplyFilter(originalMetadata);
/// </code>
/// </para>
/// </summary>
public sealed class SchemaFilterBuilder : ISchemaFilterBuilder
{
    private readonly HashSet<string> _excludedSchemas = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _includedSchemas = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _excludedTablePatterns = [];
    private readonly List<string> _includedTablePatterns = [];
    private readonly List<string> _excludedViewPatterns = [];
    private readonly List<string> _includedViewPatterns = [];
    private readonly HashSet<TypeKind> _includedTypeKinds = [];
    private readonly HashSet<SchemaObjectType> _objectTypes = [];
    
    private bool _excludeSystemObjects;
    private bool _strictMode;
    private bool _parseComments = true;
    
    /// <summary>
    /// Исключить указанные схемы из анализа
    /// </summary>
    /// <param name="schemas">Список имен схем для исключения</param>
    /// <returns>Текущий билдер для цепочки вызовов</returns>
    public ISchemaFilterBuilder ExcludeSchemas(params string[] schemas)
    {
        ArgumentNullException.ThrowIfNull(schemas);
        
        foreach (var schema in schemas)
        {
            if (!string.IsNullOrWhiteSpace(schema))
            {
                _excludedSchemas.Add(schema.Trim());
            }
        }
        
        return this;
    }

    /// <summary>
    /// Включить только указанные схемы в анализ
    /// </summary>
    /// <param name="schemas">Список имен схем для включения</param>
    /// <returns>Текущий билдер для цепочки вызовов</returns>
    public ISchemaFilterBuilder IncludeOnlySchemas(params string[] schemas)
    {
        ArgumentNullException.ThrowIfNull(schemas);
        
        foreach (var schema in schemas)
        {
            if (!string.IsNullOrWhiteSpace(schema))
            {
                _includedSchemas.Add(schema.Trim());
            }
        }
        
        return this;
    }

    /// <summary>
    /// Исключить таблицы по regex паттернам
    /// </summary>
    /// <param name="patterns">Regex паттерны для исключения таблиц</param>
    /// <returns>Текущий билдер для цепочки вызовов</returns>
    public ISchemaFilterBuilder ExcludeTables(params string[] patterns)
    {
        ArgumentNullException.ThrowIfNull(patterns);
        
        foreach (var pattern in patterns)
        {
            if (!string.IsNullOrWhiteSpace(pattern))
            {
                _excludedTablePatterns.Add(pattern.Trim());
            }
        }
        
        return this;
    }

    /// <summary>
    /// Включить только таблицы, соответствующие паттернам
    /// </summary>
    /// <param name="patterns">Regex паттерны для включения таблиц</param>
    /// <returns>Текущий билдер для цепочки вызовов</returns>
    public ISchemaFilterBuilder IncludeOnlyTables(params string[] patterns)
    {
        ArgumentNullException.ThrowIfNull(patterns);
        
        foreach (var pattern in patterns)
        {
            if (!string.IsNullOrWhiteSpace(pattern))
            {
                _includedTablePatterns.Add(pattern.Trim());
            }
        }
        
        return this;
    }

    /// <summary>
    /// Исключить представления по regex паттернам
    /// </summary>
    /// <param name="patterns">Regex паттерны для исключения представлений</param>
    /// <returns>Текущий билдер для цепочки вызовов</returns>
    public ISchemaFilterBuilder ExcludeViews(params string[] patterns)
    {
        ArgumentNullException.ThrowIfNull(patterns);
        
        foreach (var pattern in patterns)
        {
            if (!string.IsNullOrWhiteSpace(pattern))
            {
                _excludedViewPatterns.Add(pattern.Trim());
            }
        }
        
        return this;
    }

    /// <summary>
    /// Включить только представления, соответствующие паттернам
    /// </summary>
    /// <param name="patterns">Regex паттерны для включения представлений</param>
    /// <returns>Текущий билдер для цепочки вызовов</returns>
    public ISchemaFilterBuilder IncludeOnlyViews(params string[] patterns)
    {
        ArgumentNullException.ThrowIfNull(patterns);
        
        foreach (var pattern in patterns)
        {
            if (!string.IsNullOrWhiteSpace(pattern))
            {
                _includedViewPatterns.Add(pattern.Trim());
            }
        }
        
        return this;
    }

    /// <summary>
    /// Включить только определённые виды типов
    /// </summary>
    /// <param name="kinds">Типы для включения (Enum, Composite, Domain и т.д.)</param>
    /// <returns>Текущий билдер для цепочки вызовов</returns>
    public ISchemaFilterBuilder IncludeOnlyTypes(params TypeKind[] kinds)
    {
        ArgumentNullException.ThrowIfNull(kinds);
        
        foreach (var kind in kinds)
        {
            _includedTypeKinds.Add(kind);
        }
        
        return this;
    }

    /// <summary>
    /// Исключить системные объекты (pg_catalog, information_schema и т.д.)
    /// </summary>
    /// <returns>Текущий билдер для цепочки вызовов</returns>
    public ISchemaFilterBuilder ExcludeSystemObjects()
    {
        _excludeSystemObjects = true;
        
        // Добавляем стандартные системные схемы PostgreSQL
        ExcludeSchemas(
            "pg_catalog",
            "information_schema",
            "pg_toast",
            "pg_temp",
            "pg_toast_temp"
        );
        
        // Также добавляем паттерны для системных таблиц
        ExcludeTables("^pg_.*", "^sql_.*");
        
        return this;
    }

    /// <summary>
    /// Указать, какие объекты схемы анализировать
    /// </summary>
    /// <param name="objectTypes">Типы объектов для анализа</param>
    /// <returns>Текущий билдер для цепочки вызовов</returns>
    public ISchemaFilterBuilder WithObjects(params SchemaObjectType[] objectTypes)
    {
        ArgumentNullException.ThrowIfNull(objectTypes);
        
        _objectTypes.Clear();
        
        foreach (var objectType in objectTypes)
        {
            if (objectType != SchemaObjectType.None)
            {
                _objectTypes.Add(objectType);
            }
        }
        
        return this;
    }

    /// <summary>
    /// Анализировать только таблицы
    /// </summary>
    /// <returns>Текущий билдер для цепочки вызовов</returns>
    public ISchemaFilterBuilder OnlyTables()
    {
        return WithObjects(
            SchemaObjectType.Tables,
            SchemaObjectType.Indexes,
            SchemaObjectType.Constraints,
            SchemaObjectType.Triggers
        );
    }

    /// <summary>
    /// Анализировать только таблицы и представления
    /// </summary>
    /// <returns>Текущий билдер для цепочки вызовов</returns>
    public ISchemaFilterBuilder OnlyTablesAndViews()
    {
        return WithObjects(
            SchemaObjectType.Tables,
            SchemaObjectType.Views
        );
    }

    /// <summary>
    /// Включить строгий режим - генерировать ошибки для неизвестных блоков
    /// </summary>
    /// <param name="enabled">Включить или отключить строгий режим</param>
    /// <returns>Текущий билдер для цепочки вызовов</returns>
    public ISchemaFilterBuilder WithStrictMode(bool enabled = true)
    {
        _strictMode = enabled;
        return this;
    }

    /// <summary>
    /// Включить или отключить парсинг комментариев
    /// </summary>
    /// <param name="enabled">Включить или отключить парсинг комментариев</param>
    /// <returns>Текущий билдер для цепочки вызовов</returns>
    public ISchemaFilterBuilder WithCommentParsing(bool enabled = true)
    {
        _parseComments = enabled;
        return this;
    }

    /// <summary>
    /// Построить объект фильтра с заданными настройками
    /// </summary>
    /// <returns>Экземпляр ISchemaFilter</returns>
    public ISchemaFilter Build()
    {
        // Если не указаны типы объектов, по умолчанию включаем все
        IReadOnlySet<SchemaObjectType> objectTypes = _objectTypes.Count > 0 
            ? _objectTypes.ToHashSet() 
            : new HashSet<SchemaObjectType>
            {
                SchemaObjectType.Tables, 
                SchemaObjectType.Views, 
                SchemaObjectType.Types, 
                SchemaObjectType.Functions, 
                SchemaObjectType.Indexes, 
                SchemaObjectType.Triggers, 
                SchemaObjectType.Constraints, 
                SchemaObjectType.Comments
            };
        
        // Компилируем regex паттерны
        var excludedTableRegexes = _excludedTablePatterns
            .Select(CreateRegex)
            .ToArray();
        
        var includedTableRegexes = _includedTablePatterns
            .Select(CreateRegex)
            .ToArray();
        
        var excludedViewRegexes = _excludedViewPatterns
            .Select(CreateRegex)
            .ToArray();
        
        var includedViewRegexes = _includedViewPatterns
            .Select(CreateRegex)
            .ToArray();
        
        return new SchemaFilter(
            excludedSchemas: _excludedSchemas.ToHashSet(StringComparer.OrdinalIgnoreCase),
            includedSchemas: _includedSchemas.ToHashSet(StringComparer.OrdinalIgnoreCase),
            excludedTablePatterns: excludedTableRegexes,
            includedTablePatterns: includedTableRegexes,
            excludedViewPatterns: excludedViewRegexes,
            includedViewPatterns: includedViewRegexes,
            includedTypeKinds: _includedTypeKinds.Count > 0 ? _includedTypeKinds.ToHashSet() : null,
            excludeSystemObjects: _excludeSystemObjects,
            objectTypes: objectTypes,
            strictMode: _strictMode,
            parseComments: _parseComments
        );
    }
    
    /// <summary>
    /// Создает скомпилированный Regex из паттерна с обработкой ошибок
    /// </summary>
    private static Regex CreateRegex(string pattern)
    {
        try
        {
            return new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline, TimeSpan.FromSeconds(1));
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"Invalid regex pattern: '{pattern}'", nameof(pattern), ex);
        }
    }
}
