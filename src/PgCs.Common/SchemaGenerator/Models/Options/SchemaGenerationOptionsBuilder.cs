using PgCs.Common.CodeGeneration;

namespace PgCs.Common.SchemaGenerator.Models.Options;

/// <summary>
/// Fluent API builder для настройки опций генерации схемы базы данных
/// </summary>
public sealed class SchemaGenerationOptionsBuilder
{
    private string _rootNamespace = "Generated";
    private string _outputDirectory = "./Generated";
    private NamingStrategy _namingStrategy = NamingStrategy.PascalCase;
    private bool _enableNullableReferenceTypes = true;
    private bool _useRecordTypes = true;
    private bool _useInitOnlyProperties = true;
    private bool _generateXmlDocumentation = true;
    private bool _formatCode = true;
    private bool _overwriteExistingFiles = false;
    private bool _usePrimaryConstructors = false;
    private bool _generateMappingAttributes = true;
    private bool _generateValidationAttributes = true;
    private string? _tablePrefix;
    private string? _tableSuffix;
    private Dictionary<string, string>? _tableNameMappings;
    private Dictionary<string, string>? _columnNameMappings;
    private Dictionary<string, string>? _customTypeMappings;
    private List<string>? _excludeTablePatterns;
    private List<string>? _includeTablePatterns;
    private bool _generateFunctions = true;
    private FileOrganization _fileOrganization = FileOrganization.ByType;

    /// <summary>
    /// Создаёт новый builder с настройками по умолчанию
    /// </summary>
    public static SchemaGenerationOptionsBuilder Create() => new();

    /// <summary>
    /// Устанавливает корневой namespace для генерируемых типов
    /// </summary>
    /// <param name="namespace">Название namespace</param>
    public SchemaGenerationOptionsBuilder WithNamespace(string @namespace)
    {
        _rootNamespace = @namespace;
        return this;
    }

    /// <summary>
    /// Устанавливает директорию для вывода файлов
    /// </summary>
    /// <param name="path">Путь к директории</param>
    public SchemaGenerationOptionsBuilder OutputTo(string path)
    {
        _outputDirectory = path;
        return this;
    }

    /// <summary>
    /// Использовать record типы вместо классов
    /// </summary>
    public SchemaGenerationOptionsBuilder UseRecords()
    {
        _useRecordTypes = true;
        return this;
    }

    /// <summary>
    /// Использовать обычные классы вместо record
    /// </summary>
    public SchemaGenerationOptionsBuilder UseClasses()
    {
        _useRecordTypes = false;
        return this;
    }

    /// <summary>
    /// Включить генерацию XML документации
    /// </summary>
    public SchemaGenerationOptionsBuilder WithXmlDocs()
    {
        _generateXmlDocumentation = true;
        return this;
    }

    /// <summary>
    /// Отключить генерацию XML документации
    /// </summary>
    public SchemaGenerationOptionsBuilder WithoutXmlDocs()
    {
        _generateXmlDocumentation = false;
        return this;
    }

    /// <summary>
    /// Организовать файлы по типам объектов (Tables/, Views/, Functions/)
    /// </summary>
    public SchemaGenerationOptionsBuilder OrganizeByType()
    {
        _fileOrganization = FileOrganization.ByType;
        return this;
    }

    /// <summary>
    /// Организовать файлы по схемам базы данных
    /// </summary>
    public SchemaGenerationOptionsBuilder OrganizeBySchema()
    {
        _fileOrganization = FileOrganization.BySchema;
        return this;
    }

    /// <summary>
    /// Организовать файлы в одну директорию
    /// </summary>
    public SchemaGenerationOptionsBuilder OrganizeFlat()
    {
        _fileOrganization = FileOrganization.Flat;
        return this;
    }

    /// <summary>
    /// Комбинированная организация: схема -> тип объекта
    /// </summary>
    public SchemaGenerationOptionsBuilder OrganizeBySchemaAndType()
    {
        _fileOrganization = FileOrganization.BySchemaAndType;
        return this;
    }

    /// <summary>
    /// Использовать primary constructors (C# 12+)
    /// </summary>
    public SchemaGenerationOptionsBuilder UsePrimaryConstructors()
    {
        _usePrimaryConstructors = true;
        return this;
    }

    /// <summary>
    /// Генерировать атрибуты маппинга ([Table], [Column] и т.д.)
    /// </summary>
    public SchemaGenerationOptionsBuilder WithMappingAttributes()
    {
        _generateMappingAttributes = true;
        return this;
    }

    /// <summary>
    /// Не генерировать атрибуты маппинга
    /// </summary>
    public SchemaGenerationOptionsBuilder WithoutMappingAttributes()
    {
        _generateMappingAttributes = false;
        return this;
    }

    /// <summary>
    /// Генерировать атрибуты валидации ([Required], [MaxLength] и т.д.)
    /// </summary>
    public SchemaGenerationOptionsBuilder WithValidationAttributes()
    {
        _generateValidationAttributes = true;
        return this;
    }

    /// <summary>
    /// Не генерировать атрибуты валидации
    /// </summary>
    public SchemaGenerationOptionsBuilder WithoutValidationAttributes()
    {
        _generateValidationAttributes = false;
        return this;
    }

    /// <summary>
    /// Включить nullable reference types
    /// </summary>
    public SchemaGenerationOptionsBuilder EnableNullable()
    {
        _enableNullableReferenceTypes = true;
        return this;
    }

    /// <summary>
    /// Отключить nullable reference types
    /// </summary>
    public SchemaGenerationOptionsBuilder DisableNullable()
    {
        _enableNullableReferenceTypes = false;
        return this;
    }

    /// <summary>
    /// Использовать init-only свойства
    /// </summary>
    public SchemaGenerationOptionsBuilder UseInitProperties()
    {
        _useInitOnlyProperties = true;
        return this;
    }

    /// <summary>
    /// Использовать обычные set свойства
    /// </summary>
    public SchemaGenerationOptionsBuilder UseSetProperties()
    {
        _useInitOnlyProperties = false;
        return this;
    }

    /// <summary>
    /// Форматировать сгенерированный код
    /// </summary>
    public SchemaGenerationOptionsBuilder WithFormatting()
    {
        _formatCode = true;
        return this;
    }

    /// <summary>
    /// Не форматировать сгенерированный код
    /// </summary>
    public SchemaGenerationOptionsBuilder WithoutFormatting()
    {
        _formatCode = false;
        return this;
    }

    /// <summary>
    /// Перезаписывать существующие файлы
    /// </summary>
    public SchemaGenerationOptionsBuilder OverwriteFiles()
    {
        _overwriteExistingFiles = true;
        return this;
    }

    /// <summary>
    /// Не перезаписывать существующие файлы
    /// </summary>
    public SchemaGenerationOptionsBuilder PreserveFiles()
    {
        _overwriteExistingFiles = false;
        return this;
    }

    /// <summary>
    /// Удалить префикс из имён таблиц при генерации типов
    /// </summary>
    /// <param name="prefix">Префикс для удаления (например, "tbl_")</param>
    public SchemaGenerationOptionsBuilder RemoveTablePrefix(string prefix)
    {
        _tablePrefix = prefix;
        return this;
    }

    /// <summary>
    /// Удалить суффикс из имён таблиц при генерации типов
    /// </summary>
    /// <param name="suffix">Суффикс для удаления</param>
    public SchemaGenerationOptionsBuilder RemoveTableSuffix(string suffix)
    {
        _tableSuffix = suffix;
        return this;
    }

    /// <summary>
    /// Добавить маппинг имени таблицы на имя типа
    /// </summary>
    /// <param name="tableName">Имя таблицы в БД</param>
    /// <param name="typeName">Желаемое имя типа</param>
    public SchemaGenerationOptionsBuilder MapTable(string tableName, string typeName)
    {
        _tableNameMappings ??= new Dictionary<string, string>();
        _tableNameMappings[tableName] = typeName;
        return this;
    }

    /// <summary>
    /// Добавить маппинг имени колонки на имя свойства
    /// </summary>
    /// <param name="columnName">Имя колонки в БД</param>
    /// <param name="propertyName">Желаемое имя свойства</param>
    public SchemaGenerationOptionsBuilder MapColumn(string columnName, string propertyName)
    {
        _columnNameMappings ??= new Dictionary<string, string>();
        _columnNameMappings[columnName] = propertyName;
        return this;
    }

    /// <summary>
    /// Добавить кастомный маппинг PostgreSQL типа на C# тип
    /// </summary>
    /// <param name="pgType">Тип PostgreSQL</param>
    /// <param name="csharpType">C# тип</param>
    public SchemaGenerationOptionsBuilder MapType(string pgType, string csharpType)
    {
        _customTypeMappings ??= new Dictionary<string, string>();
        _customTypeMappings[pgType] = csharpType;
        return this;
    }

    /// <summary>
    /// Исключить таблицы по regex паттерну
    /// </summary>
    /// <param name="patterns">Regex паттерны для исключения</param>
    public SchemaGenerationOptionsBuilder ExcludeTables(params string[] patterns)
    {
        _excludeTablePatterns ??= new List<string>();
        _excludeTablePatterns.AddRange(patterns);
        return this;
    }

    /// <summary>
    /// Включить только указанные таблицы (regex паттерны)
    /// </summary>
    /// <param name="patterns">Regex паттерны для включения</param>
    public SchemaGenerationOptionsBuilder IncludeOnlyTables(params string[] patterns)
    {
        _includeTablePatterns ??= new List<string>();
        _includeTablePatterns.AddRange(patterns);
        return this;
    }

    /// <summary>
    /// Генерировать методы для функций базы данных
    /// </summary>
    public SchemaGenerationOptionsBuilder WithFunctions()
    {
        _generateFunctions = true;
        return this;
    }

    /// <summary>
    /// Не генерировать методы для функций базы данных
    /// </summary>
    public SchemaGenerationOptionsBuilder WithoutFunctions()
    {
        _generateFunctions = false;
        return this;
    }

    /// <summary>
    /// Установить стратегию именования
    /// </summary>
    /// <param name="strategy">Стратегия именования</param>
    public SchemaGenerationOptionsBuilder WithNamingStrategy(NamingStrategy strategy)
    {
        _namingStrategy = strategy;
        return this;
    }

    /// <summary>
    /// Использовать PascalCase для именования (по умолчанию)
    /// </summary>
    public SchemaGenerationOptionsBuilder UsePascalCase()
    {
        _namingStrategy = NamingStrategy.PascalCase;
        return this;
    }

    /// <summary>
    /// Использовать camelCase для именования
    /// </summary>
    public SchemaGenerationOptionsBuilder UseCamelCase()
    {
        _namingStrategy = NamingStrategy.CamelCase;
        return this;
    }

    /// <summary>
    /// Использовать оригинальные имена без преобразования
    /// </summary>
    public SchemaGenerationOptionsBuilder UseOriginalNames()
    {
        _namingStrategy = NamingStrategy.Original;
        return this;
    }

    /// <summary>
    /// Строит финальные опции генерации
    /// </summary>
    /// <returns>Настроенные опции генерации схемы</returns>
    public SchemaGenerationOptions Build()
    {
        return new SchemaGenerationOptions
        {
            RootNamespace = _rootNamespace,
            OutputDirectory = _outputDirectory,
            NamingStrategy = _namingStrategy,
            EnableNullableReferenceTypes = _enableNullableReferenceTypes,
            UseRecordTypes = _useRecordTypes,
            UseInitOnlyProperties = _useInitOnlyProperties,
            GenerateXmlDocumentation = _generateXmlDocumentation,
            FormatCode = _formatCode,
            OverwriteExistingFiles = _overwriteExistingFiles,
            UsePrimaryConstructors = _usePrimaryConstructors,
            GenerateMappingAttributes = _generateMappingAttributes,
            GenerateValidationAttributes = _generateValidationAttributes,
            TablePrefix = _tablePrefix,
            TableSuffix = _tableSuffix,
            TableNameMappings = _tableNameMappings,
            ColumnNameMappings = _columnNameMappings,
            CustomTypeMappings = _customTypeMappings,
            ExcludeTablePatterns = _excludeTablePatterns,
            IncludeTablePatterns = _includeTablePatterns,
            GenerateFunctions = _generateFunctions,
            FileOrganization = _fileOrganization
        };
    }
}
