using PgCs.Common.CodeGeneration;

namespace PgCs.Common.QueryGenerator.Models.Options;

/// <summary>
/// Fluent API builder для настройки опций генерации SQL запросов
/// </summary>
public sealed class QueryGenerationOptionsBuilder
{
    private string _rootNamespace = "Generated.Queries";
    private string _outputDirectory = "./Generated/Queries";
    private string _repositoryClassName = "QueryRepository";
    private string _repositoryInterfaceName = "IQueryRepository";
    private NamingStrategy _namingStrategy = NamingStrategy.PascalCase;
    private bool _enableNullableReferenceTypes = true;
    private bool _useRecordTypes = true;
    private bool _useInitOnlyProperties = true;
    private bool _generateXmlDocumentation = true;
    private bool _formatCode = true;
    private bool _overwriteExistingFiles = false;
    private bool _generateAsyncMethods = true;
    private bool _useValueTask = true;
    private bool _generateInterface = true;
    private MethodOrganization _methodOrganization = MethodOrganization.SingleRepository;
    private bool _useDapper = false;
    private bool _useNpgsqlDirectly = true;
    private bool _generateParameterModels = false;
    private int _parameterModelThreshold = 5;
    private bool _alwaysGenerateResultModels = true;
    private bool _reuseSchemaModels = true;
    private string? _schemaModelsNamespace;
    private bool _supportCancellation = true;
    private bool _generateExtensionMethods = false;
    private NullHandlingStrategy _nullHandling = NullHandlingStrategy.Nullable;
    private bool _includeSqlInDocumentation = true;
    private bool _usePreparedStatements = true;
    private bool _generateTransactionSupport = true;
    private bool _useNpgsqlDataSource = true;

    /// <summary>
    /// Создаёт новый builder с настройками по умолчанию
    /// </summary>
    public static QueryGenerationOptionsBuilder Create() => new();

    /// <summary>
    /// Устанавливает корневой namespace
    /// </summary>
    public QueryGenerationOptionsBuilder WithNamespace(string @namespace)
    {
        _rootNamespace = @namespace;
        return this;
    }

    /// <summary>
    /// Устанавливает директорию вывода
    /// </summary>
    public QueryGenerationOptionsBuilder OutputTo(string path)
    {
        _outputDirectory = path;
        return this;
    }

    /// <summary>
    /// Устанавливает имя класса репозитория
    /// </summary>
    public QueryGenerationOptionsBuilder WithRepositoryName(string className, string? interfaceName = null)
    {
        _repositoryClassName = className;
        _repositoryInterfaceName = interfaceName ?? $"I{className}";
        return this;
    }

    /// <summary>
    /// Использовать record типы для моделей результатов
    /// </summary>
    public QueryGenerationOptionsBuilder UseRecords()
    {
        _useRecordTypes = true;
        return this;
    }

    /// <summary>
    /// Использовать классы для моделей результатов
    /// </summary>
    public QueryGenerationOptionsBuilder UseClasses()
    {
        _useRecordTypes = false;
        return this;
    }

    /// <summary>
    /// Генерировать XML документацию
    /// </summary>
    public QueryGenerationOptionsBuilder WithXmlDocs()
    {
        _generateXmlDocumentation = true;
        return this;
    }

    /// <summary>
    /// Не генерировать XML документацию
    /// </summary>
    public QueryGenerationOptionsBuilder WithoutXmlDocs()
    {
        _generateXmlDocumentation = false;
        return this;
    }

    /// <summary>
    /// Генерировать асинхронные методы
    /// </summary>
    public QueryGenerationOptionsBuilder UseAsync()
    {
        _generateAsyncMethods = true;
        return this;
    }

    /// <summary>
    /// Генерировать только синхронные методы
    /// </summary>
    public QueryGenerationOptionsBuilder UseSync()
    {
        _generateAsyncMethods = false;
        return this;
    }

    /// <summary>
    /// Использовать ValueTask вместо Task
    /// </summary>
    public QueryGenerationOptionsBuilder UseValueTask()
    {
        _useValueTask = true;
        _generateAsyncMethods = true;
        return this;
    }

    /// <summary>
    /// Использовать Task вместо ValueTask
    /// </summary>
    public QueryGenerationOptionsBuilder UseTask()
    {
        _useValueTask = false;
        return this;
    }

    /// <summary>
    /// Генерировать интерфейс репозитория
    /// </summary>
    public QueryGenerationOptionsBuilder WithInterface()
    {
        _generateInterface = true;
        return this;
    }

    /// <summary>
    /// Не генерировать интерфейс репозитория
    /// </summary>
    public QueryGenerationOptionsBuilder WithoutInterface()
    {
        _generateInterface = false;
        return this;
    }

    /// <summary>
    /// Организовать методы в один репозиторий
    /// </summary>
    public QueryGenerationOptionsBuilder InSingleRepository()
    {
        _methodOrganization = MethodOrganization.SingleRepository;
        return this;
    }

    /// <summary>
    /// Группировать методы по типу запроса (Select, Insert, Update, Delete)
    /// </summary>
    public QueryGenerationOptionsBuilder GroupByQueryType()
    {
        _methodOrganization = MethodOrganization.ByQueryType;
        return this;
    }

    /// <summary>
    /// Группировать методы по сущностям
    /// </summary>
    public QueryGenerationOptionsBuilder GroupByEntity()
    {
        _methodOrganization = MethodOrganization.ByEntity;
        return this;
    }

    /// <summary>
    /// Генерировать отдельный файл для каждого метода
    /// </summary>
    public QueryGenerationOptionsBuilder OneMethodPerFile()
    {
        _methodOrganization = MethodOrganization.PerMethod;
        return this;
    }

    /// <summary>
    /// Использовать Dapper для выполнения запросов
    /// </summary>
    public QueryGenerationOptionsBuilder UseDapper()
    {
        _useDapper = true;
        _useNpgsqlDirectly = false;
        return this;
    }

    /// <summary>
    /// Использовать NpgsqlDataReader напрямую
    /// </summary>
    public QueryGenerationOptionsBuilder UseNpgsqlDirectly()
    {
        _useNpgsqlDirectly = true;
        _useDapper = false;
        return this;
    }

    /// <summary>
    /// Генерировать модели параметров для сложных запросов
    /// </summary>
    /// <param name="threshold">Минимальное количество параметров</param>
    public QueryGenerationOptionsBuilder WithParameterModels(int threshold = 5)
    {
        _generateParameterModels = true;
        _parameterModelThreshold = threshold;
        return this;
    }

    /// <summary>
    /// Не генерировать модели параметров
    /// </summary>
    public QueryGenerationOptionsBuilder WithoutParameterModels()
    {
        _generateParameterModels = false;
        return this;
    }

    /// <summary>
    /// Всегда генерировать модели результатов
    /// </summary>
    public QueryGenerationOptionsBuilder AlwaysGenerateResultModels()
    {
        _alwaysGenerateResultModels = true;
        return this;
    }

    /// <summary>
    /// Генерировать модели результатов только при необходимости
    /// </summary>
    public QueryGenerationOptionsBuilder GenerateResultModelsOnDemand()
    {
        _alwaysGenerateResultModels = false;
        return this;
    }

    /// <summary>
    /// Повторно использовать модели схемы
    /// </summary>
    /// <param name="schemaNamespace">Namespace моделей схемы</param>
    public QueryGenerationOptionsBuilder ReuseSchemaModels(string? schemaNamespace = null)
    {
        _reuseSchemaModels = true;
        _schemaModelsNamespace = schemaNamespace;
        return this;
    }

    /// <summary>
    /// Не использовать модели схемы повторно
    /// </summary>
    public QueryGenerationOptionsBuilder WithoutSchemaModels()
    {
        _reuseSchemaModels = false;
        return this;
    }

    /// <summary>
    /// Добавить поддержку CancellationToken
    /// </summary>
    public QueryGenerationOptionsBuilder WithCancellation()
    {
        _supportCancellation = true;
        return this;
    }

    /// <summary>
    /// Не добавлять CancellationToken
    /// </summary>
    public QueryGenerationOptionsBuilder WithoutCancellation()
    {
        _supportCancellation = false;
        return this;
    }

    /// <summary>
    /// Генерировать методы расширения для IDbConnection
    /// </summary>
    public QueryGenerationOptionsBuilder AsExtensionMethods()
    {
        _generateExtensionMethods = true;
        return this;
    }

    /// <summary>
    /// Генерировать обычные методы в классе
    /// </summary>
    public QueryGenerationOptionsBuilder AsInstanceMethods()
    {
        _generateExtensionMethods = false;
        return this;
    }

    /// <summary>
    /// Установить стратегию обработки NULL
    /// </summary>
    public QueryGenerationOptionsBuilder WithNullHandling(NullHandlingStrategy strategy)
    {
        _nullHandling = strategy;
        return this;
    }

    /// <summary>
    /// Использовать nullable типы для NULL значений
    /// </summary>
    public QueryGenerationOptionsBuilder UseNullableTypes()
    {
        _nullHandling = NullHandlingStrategy.Nullable;
        return this;
    }

    /// <summary>
    /// Использовать default значения для NULL
    /// </summary>
    public QueryGenerationOptionsBuilder UseDefaultValues()
    {
        _nullHandling = NullHandlingStrategy.DefaultValues;
        return this;
    }

    /// <summary>
    /// Генерировать исключение при NULL
    /// </summary>
    public QueryGenerationOptionsBuilder ThrowOnNull()
    {
        _nullHandling = NullHandlingStrategy.ThrowException;
        return this;
    }

    /// <summary>
    /// Включать SQL в XML комментарии методов
    /// </summary>
    public QueryGenerationOptionsBuilder IncludeSqlInDocs()
    {
        _includeSqlInDocumentation = true;
        return this;
    }

    /// <summary>
    /// Не включать SQL в комментарии
    /// </summary>
    public QueryGenerationOptionsBuilder ExcludeSqlFromDocs()
    {
        _includeSqlInDocumentation = false;
        return this;
    }

    /// <summary>
    /// Использовать prepared statements
    /// </summary>
    public QueryGenerationOptionsBuilder WithPreparedStatements()
    {
        _usePreparedStatements = true;
        return this;
    }

    /// <summary>
    /// Не использовать prepared statements
    /// </summary>
    public QueryGenerationOptionsBuilder WithoutPreparedStatements()
    {
        _usePreparedStatements = false;
        return this;
    }

    /// <summary>
    /// Добавить поддержку транзакций
    /// </summary>
    public QueryGenerationOptionsBuilder WithTransactionSupport()
    {
        _generateTransactionSupport = true;
        return this;
    }

    /// <summary>
    /// Без поддержки транзакций
    /// </summary>
    public QueryGenerationOptionsBuilder WithoutTransactionSupport()
    {
        _generateTransactionSupport = false;
        return this;
    }

    /// <summary>
    /// Использовать NpgsqlDataSource
    /// </summary>
    public QueryGenerationOptionsBuilder UseDataSource()
    {
        _useNpgsqlDataSource = true;
        return this;
    }

    /// <summary>
    /// Не использовать NpgsqlDataSource
    /// </summary>
    public QueryGenerationOptionsBuilder WithoutDataSource()
    {
        _useNpgsqlDataSource = false;
        return this;
    }

    /// <summary>
    /// Форматировать код
    /// </summary>
    public QueryGenerationOptionsBuilder WithFormatting()
    {
        _formatCode = true;
        return this;
    }

    /// <summary>
    /// Не форматировать код
    /// </summary>
    public QueryGenerationOptionsBuilder WithoutFormatting()
    {
        _formatCode = false;
        return this;
    }

    /// <summary>
    /// Перезаписывать файлы
    /// </summary>
    public QueryGenerationOptionsBuilder OverwriteFiles()
    {
        _overwriteExistingFiles = true;
        return this;
    }

    /// <summary>
    /// Сохранять существующие файлы
    /// </summary>
    public QueryGenerationOptionsBuilder PreserveFiles()
    {
        _overwriteExistingFiles = false;
        return this;
    }

    /// <summary>
    /// Строит финальные опции генерации
    /// </summary>
    public QueryGenerationOptions Build()
    {
        return new QueryGenerationOptions
        {
            RootNamespace = _rootNamespace,
            OutputDirectory = _outputDirectory,
            RepositoryClassName = _repositoryClassName,
            RepositoryInterfaceName = _repositoryInterfaceName,
            NamingStrategy = _namingStrategy,
            EnableNullableReferenceTypes = _enableNullableReferenceTypes,
            UseRecordTypes = _useRecordTypes,
            UseInitOnlyProperties = _useInitOnlyProperties,
            GenerateXmlDocumentation = _generateXmlDocumentation,
            FormatCode = _formatCode,
            OverwriteExistingFiles = _overwriteExistingFiles,
            GenerateAsyncMethods = _generateAsyncMethods,
            UseValueTask = _useValueTask,
            GenerateInterface = _generateInterface,
            MethodOrganization = _methodOrganization,
            UseDapper = _useDapper,
            UseNpgsqlDirectly = _useNpgsqlDirectly,
            GenerateParameterModels = _generateParameterModels,
            ParameterModelThreshold = _parameterModelThreshold,
            AlwaysGenerateResultModels = _alwaysGenerateResultModels,
            ReuseSchemaModels = _reuseSchemaModels,
            SchemaModelsNamespace = _schemaModelsNamespace,
            SupportCancellation = _supportCancellation,
            GenerateExtensionMethods = _generateExtensionMethods,
            NullHandling = _nullHandling,
            IncludeSqlInDocumentation = _includeSqlInDocumentation,
            UsePreparedStatements = _usePreparedStatements,
            GenerateTransactionSupport = _generateTransactionSupport,
            UseNpgsqlDataSource = _useNpgsqlDataSource
        };
    }
}
