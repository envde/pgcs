using System.Text.RegularExpressions;
using PgCs.Core.Schema.Analyzer;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante;

/// <summary>
/// Реализация фильтра схемы базы данных.
/// <para>
/// Применяет настройки фильтрации к метаданным схемы, удаляя объекты,
/// которые не соответствуют критериям фильтрации.
/// </para>
/// <para>
/// Фильтрация происходит по следующим параметрам:
/// <list type="bullet">
/// <item><description>Схемы (включение/исключение по имени)</description></item>
/// <item><description>Таблицы (regex паттерны для включения/исключения)</description></item>
/// <item><description>Представления (regex паттерны для включения/исключения)</description></item>
/// <item><description>Типы объектов (Tables, Views, Types, Functions, и т.д.)</description></item>
/// <item><description>Виды типов данных (Enum, Composite, Domain)</description></item>
/// <item><description>Системные объекты PostgreSQL</description></item>
/// <item><description>Комментарии (включение/отключение парсинга)</description></item>
/// </list>
/// </para>
/// </summary>
/// <remarks>
/// Этот класс является internal и создается через <see cref="SchemaFilterBuilder"/>.
/// Прямое создание экземпляров не предусмотрено.
/// </remarks>
internal sealed class SchemaFilter : ISchemaFilter
{
    private readonly IReadOnlySet<string> _excludedSchemas;
    private readonly IReadOnlySet<string> _includedSchemas;
    private readonly IReadOnlyList<Regex> _excludedTablePatterns;
    private readonly IReadOnlyList<Regex> _includedTablePatterns;
    private readonly IReadOnlyList<Regex> _excludedViewPatterns;
    private readonly IReadOnlyList<Regex> _includedViewPatterns;
    private readonly IReadOnlySet<TypeKind>? _includedTypeKinds;
    private readonly IReadOnlySet<SchemaObjectType> _objectTypes;
    private readonly bool _excludeSystemObjects;
    private readonly bool _strictMode;
    private readonly bool _parseComments;

    /// <summary>
    /// Создает новый экземпляр фильтра схемы
    /// </summary>
    internal SchemaFilter(
        IReadOnlySet<string> excludedSchemas,
        IReadOnlySet<string> includedSchemas,
        IReadOnlyList<Regex> excludedTablePatterns,
        IReadOnlyList<Regex> includedTablePatterns,
        IReadOnlyList<Regex> excludedViewPatterns,
        IReadOnlyList<Regex> includedViewPatterns,
        IReadOnlySet<TypeKind>? includedTypeKinds,
        bool excludeSystemObjects,
        IReadOnlySet<SchemaObjectType> objectTypes,
        bool strictMode,
        bool parseComments)
    {
        _excludedSchemas = excludedSchemas;
        _includedSchemas = includedSchemas;
        _excludedTablePatterns = excludedTablePatterns;
        _includedTablePatterns = includedTablePatterns;
        _excludedViewPatterns = excludedViewPatterns;
        _includedViewPatterns = includedViewPatterns;
        _includedTypeKinds = includedTypeKinds;
        _excludeSystemObjects = excludeSystemObjects;
        _objectTypes = objectTypes;
        _strictMode = strictMode;
        _parseComments = parseComments;
    }

    /// <summary>
    /// Применяет фильтр ко всей схеме и возвращает отфильтрованные метаданные
    /// </summary>
    /// <param name="metadata">Исходные метаданные схемы</param>
    /// <returns>Отфильтрованные метаданные схемы</returns>
    public SchemaMetadata ApplyFilter(SchemaMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        return new SchemaMetadata
        {
            Tables = FilterTables(metadata.Tables),
            Views = FilterViews(metadata.Views),
            Enums = FilterEnums(metadata.Enums),
            Composites = FilterComposites(metadata.Composites),
            Domains = FilterDomains(metadata.Domains),
            Functions = FilterFunctions(metadata.Functions),
            Indexes = FilterIndexes(metadata.Indexes),
            Triggers = FilterTriggers(metadata.Triggers),
            Constraints = FilterConstraints(metadata.Constraints),
            Partitions = FilterPartitions(metadata.Partitions),
            CompositeTypeComments = FilterCompositeTypeComments(metadata.CompositeTypeComments),
            TableComments = FilterTableComments(metadata.TableComments),
            ColumnComments = FilterColumnComments(metadata.ColumnComments),
            IndexComments = FilterIndexComments(metadata.IndexComments),
            TriggerComments = FilterTriggerComments(metadata.TriggerComments),
            FunctionComments = FilterFunctionComments(metadata.FunctionComments),
            ConstraintComments = FilterConstraintComments(metadata.ConstraintComments),
            ValidationIssues = metadata.ValidationIssues,
            SourcePaths = metadata.SourcePaths,
            AnalyzedAt = metadata.AnalyzedAt
        };
    }

    /// <summary>
    /// Фильтрует таблицы согласно настройкам
    /// </summary>
    private IReadOnlyList<TableDefinition> FilterTables(IReadOnlyList<TableDefinition> tables)
    {
        if (!_objectTypes.Contains(SchemaObjectType.Tables))
            return [];

        return tables
            .Where(t => ShouldIncludeBySchema(t.Schema))
            .Where(t => ShouldIncludeTable(t.Name))
            .ToList();
    }

    /// <summary>
    /// Фильтрует представления согласно настройкам
    /// </summary>
    private IReadOnlyList<ViewDefinition> FilterViews(IReadOnlyList<ViewDefinition> views)
    {
        if (!_objectTypes.Contains(SchemaObjectType.Views))
            return [];

        return views
            .Where(v => ShouldIncludeBySchema(v.Schema))
            .Where(v => ShouldIncludeView(v.Name))
            .ToList();
    }

    /// <summary>
    /// Фильтрует ENUM типы согласно настройкам
    /// </summary>
    private IReadOnlyList<EnumTypeDefinition> FilterEnums(IReadOnlyList<EnumTypeDefinition> enums)
    {
        if (!_objectTypes.Contains(SchemaObjectType.Types) ||
            _includedTypeKinds != null && !_includedTypeKinds.Contains(TypeKind.Enum))
            return [];

        return enums
            .Where(e => ShouldIncludeBySchema(e.Schema))
            .ToList();
    }

    /// <summary>
    /// Фильтрует композитные типы согласно настройкам
    /// </summary>
    private IReadOnlyList<CompositeTypeDefinition> FilterComposites(IReadOnlyList<CompositeTypeDefinition> composites)
    {
        if (!_objectTypes.Contains(SchemaObjectType.Types))
            return [];

        if (_includedTypeKinds != null && !_includedTypeKinds.Contains(TypeKind.Composite))
            return [];

        return composites
            .Where(c => ShouldIncludeBySchema(c.Schema))
            .ToList();
    }

    /// <summary>
    /// Фильтрует доменные типы согласно настройкам
    /// </summary>
    private IReadOnlyList<DomainTypeDefinition> FilterDomains(IReadOnlyList<DomainTypeDefinition> domains)
    {
        if (!_objectTypes.Contains(SchemaObjectType.Types))
            return [];

        if (_includedTypeKinds != null && !_includedTypeKinds.Contains(TypeKind.Domain))
            return [];

        return domains
            .Where(d => ShouldIncludeBySchema(d.Schema))
            .ToList();
    }

    /// <summary>
    /// Фильтрует функции согласно настройкам
    /// </summary>
    private IReadOnlyList<FunctionDefinition> FilterFunctions(IReadOnlyList<FunctionDefinition> functions)
    {
        if (!_objectTypes.Contains(SchemaObjectType.Functions))
            return [];

        return functions
            .Where(f => ShouldIncludeBySchema(f.Schema))
            .ToList();
    }

    /// <summary>
    /// Фильтрует индексы согласно настройкам
    /// </summary>
    private IReadOnlyList<IndexDefinition> FilterIndexes(IReadOnlyList<IndexDefinition> indexes)
    {
        if (!_objectTypes.Contains(SchemaObjectType.Indexes))
            return [];

        return indexes
            .Where(i => ShouldIncludeBySchema(i.Schema))
            .ToList();
    }

    /// <summary>
    /// Фильтрует триггеры согласно настройкам
    /// </summary>
    private IReadOnlyList<TriggerDefinition> FilterTriggers(IReadOnlyList<TriggerDefinition> triggers)
    {
        if (!_objectTypes.Contains(SchemaObjectType.Triggers))
            return [];

        return triggers
            .Where(t => ShouldIncludeBySchema(t.Schema))
            .ToList();
    }

    /// <summary>
    /// Фильтрует ограничения согласно настройкам
    /// </summary>
    private IReadOnlyList<ConstraintDefinition> FilterConstraints(IReadOnlyList<ConstraintDefinition> constraints)
    {
        if (!_objectTypes.Contains(SchemaObjectType.Constraints))
            return [];

        return constraints
            .Where(c => ShouldIncludeBySchema(c.Schema))
            .ToList();
    }

    /// <summary>
    /// Фильтрует партиции согласно настройкам
    /// </summary>
    private IReadOnlyList<PartitionDefinition> FilterPartitions(IReadOnlyList<PartitionDefinition> partitions)
    {
        // Партиции связаны с таблицами, поэтому фильтруем если включены таблицы
        if (!_objectTypes.Contains(SchemaObjectType.Tables))
            return [];

        return partitions
            .Where(p => ShouldIncludeBySchema(p.Schema))
            .ToList();
    }

    /// <summary>
    /// Фильтрует комментарии к композитным типам
    /// </summary>
    private IReadOnlyList<CompositeTypeCommentDefinition> FilterCompositeTypeComments(
        IReadOnlyList<CompositeTypeCommentDefinition> comments)
    {
        if (!_parseComments || !_objectTypes.Contains(SchemaObjectType.Comments))
            return [];

        if (!_objectTypes.Contains(SchemaObjectType.Types))
            return [];

        return comments
            .Where(c => ShouldIncludeBySchema(c.Schema))
            .ToList();
    }

    /// <summary>
    /// Фильтрует комментарии к таблицам
    /// </summary>
    private IReadOnlyList<TableCommentDefinition> FilterTableComments(
        IReadOnlyList<TableCommentDefinition> comments)
    {
        if (!_parseComments || !_objectTypes.Contains(SchemaObjectType.Comments))
            return [];

        if (!_objectTypes.Contains(SchemaObjectType.Tables))
            return [];

        return comments
            .Where(c => ShouldIncludeBySchema(c.Schema))
            .Where(c => ShouldIncludeTable(c.TableName))
            .ToList();
    }

    /// <summary>
    /// Фильтрует комментарии к колонкам
    /// </summary>
    private IReadOnlyList<ColumnCommentDefinition> FilterColumnComments(
        IReadOnlyList<ColumnCommentDefinition> comments)
    {
        if (!_parseComments || !_objectTypes.Contains(SchemaObjectType.Comments))
            return [];

        if (!_objectTypes.Contains(SchemaObjectType.Tables))
            return [];

        return comments
            .Where(c => ShouldIncludeBySchema(c.Schema))
            .Where(c => ShouldIncludeTable(c.TableName))
            .ToList();
    }

    /// <summary>
    /// Фильтрует комментарии к индексам
    /// </summary>
    private IReadOnlyList<IndexCommentDefinition> FilterIndexComments(
        IReadOnlyList<IndexCommentDefinition> comments)
    {
        if (!_parseComments || !_objectTypes.Contains(SchemaObjectType.Comments))
            return [];

        if (!_objectTypes.Contains(SchemaObjectType.Indexes))
            return [];

        return comments
            .Where(c => ShouldIncludeBySchema(c.Schema))
            .ToList();
    }

    /// <summary>
    /// Фильтрует комментарии к триггерам
    /// </summary>
    private IReadOnlyList<TriggerCommentDefinition> FilterTriggerComments(
        IReadOnlyList<TriggerCommentDefinition> comments)
    {
        if (!_parseComments || !_objectTypes.Contains(SchemaObjectType.Comments))
            return [];

        if (!_objectTypes.Contains(SchemaObjectType.Triggers))
            return [];

        return comments
            .Where(c => ShouldIncludeBySchema(c.Schema))
            .ToList();
    }

    /// <summary>
    /// Фильтрует комментарии к функциям
    /// </summary>
    private IReadOnlyList<FunctionCommentDefinition> FilterFunctionComments(
        IReadOnlyList<FunctionCommentDefinition> comments)
    {
        if (!_parseComments || !_objectTypes.Contains(SchemaObjectType.Comments))
            return [];

        if (!_objectTypes.Contains(SchemaObjectType.Functions))
            return [];

        return comments
            .Where(c => ShouldIncludeBySchema(c.Schema))
            .ToList();
    }

    /// <summary>
    /// Фильтрует комментарии к ограничениям
    /// </summary>
    private IReadOnlyList<ConstraintCommentDefinition> FilterConstraintComments(
        IReadOnlyList<ConstraintCommentDefinition> comments)
    {
        if (!_parseComments || !_objectTypes.Contains(SchemaObjectType.Comments))
            return [];

        if (!_objectTypes.Contains(SchemaObjectType.Constraints))
            return [];

        return comments
            .Where(c => ShouldIncludeBySchema(c.Schema))
            .ToList();
    }

    /// <summary>
    /// Проверяет, должна ли быть включена схема по имени
    /// </summary>
    private bool ShouldIncludeBySchema(string? schema)
    {
        // Если схема не указана, считаем это public схемой
        var schemaName = schema ?? "public";

        // Проверка на системные объекты
        if (_excludeSystemObjects && IsSystemSchema(schemaName))
            return false;

        // Если есть список включаемых схем, проверяем принадлежность
        if (_includedSchemas.Count > 0)
            return _includedSchemas.Contains(schemaName);

        // Проверяем исключения
        if (_excludedSchemas.Count > 0 && _excludedSchemas.Contains(schemaName))
            return false;

        return true;
    }

    /// <summary>
    /// Проверяет, должна ли быть включена таблица по имени
    /// </summary>
    private bool ShouldIncludeTable(string tableName)
    {
        // Если есть паттерны включения, проверяем соответствие
        if (_includedTablePatterns.Count > 0)
            return _includedTablePatterns.Any(p => p.IsMatch(tableName));

        // Проверяем паттерны исключения
        if (_excludedTablePatterns.Count > 0)
            return !_excludedTablePatterns.Any(p => p.IsMatch(tableName));

        return true;
    }

    /// <summary>
    /// Проверяет, должно ли быть включено представление по имени
    /// </summary>
    private bool ShouldIncludeView(string viewName)
    {
        // Если есть паттерны включения, проверяем соответствие
        if (_includedViewPatterns.Count > 0)
            return _includedViewPatterns.Any(p => p.IsMatch(viewName));

        // Проверяем паттерны исключения
        if (_excludedViewPatterns.Count > 0)
            return !_excludedViewPatterns.Any(p => p.IsMatch(viewName));

        return true;
    }

    /// <summary>
    /// Проверяет, является ли схема системной
    /// </summary>
    private static bool IsSystemSchema(string schemaName)
    {
        return schemaName.StartsWith("pg_", StringComparison.OrdinalIgnoreCase) ||
               schemaName.Equals("information_schema", StringComparison.OrdinalIgnoreCase);
    }
}