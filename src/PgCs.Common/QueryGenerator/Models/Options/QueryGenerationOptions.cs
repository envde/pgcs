using PgCs.Common.CodeGeneration;

namespace PgCs.Common.QueryGenerator.Models.Options;

/// <summary>
/// Опции генерации методов для SQL запросов
/// </summary>
public sealed record QueryGenerationOptions : CodeGenerationOptions
{
    /// <summary>
    /// Имя класса репозитория
    /// </summary>
    public string RepositoryClassName { get; init; } = "QueryRepository";

    /// <summary>
    /// Имя интерфейса репозитория
    /// </summary>
    public string RepositoryInterfaceName { get; init; } = "IQueryRepository";

    /// <summary>
    /// Генерировать асинхронные методы (async/await)
    /// </summary>
    public bool GenerateAsyncMethods { get; init; } = true;

    /// <summary>
    /// Использовать ValueTask вместо Task для асинхронных методов
    /// </summary>
    public bool UseValueTask { get; init; } = true;

    /// <summary>
    /// Генерировать отдельный интерфейс репозитория
    /// </summary>
    public bool GenerateInterface { get; init; } = true;

    /// <summary>
    /// Стратегия организации методов
    /// </summary>
    public MethodOrganization MethodOrganization { get; init; } = MethodOrganization.SingleRepository;

    /// <summary>
    /// Использовать Dapper для выполнения запросов
    /// </summary>
    public bool UseDapper { get; init; } = false;

    /// <summary>
    /// Использовать NpgsqlDataReader напрямую
    /// </summary>
    public bool UseNpgsqlDirectly { get; init; } = true;

    /// <summary>
    /// Генерировать модели параметров для сложных запросов
    /// </summary>
    public bool GenerateParameterModels { get; init; } = false;

    /// <summary>
    /// Порог количества параметров для генерации модели параметров
    /// </summary>
    public int ParameterModelThreshold { get; init; } = 5;

    /// <summary>
    /// Генерировать модели результатов для всех запросов
    /// </summary>
    public bool AlwaysGenerateResultModels { get; init; } = true;

    /// <summary>
    /// Повторно использовать существующие модели схемы для результатов
    /// </summary>
    public bool ReuseSchemaModels { get; init; } = true;

    /// <summary>
    /// Namespace моделей схемы для повторного использования
    /// </summary>
    public string? SchemaModelsNamespace { get; init; }

    /// <summary>
    /// Генерировать методы с CancellationToken
    /// </summary>
    public bool SupportCancellation { get; init; } = true;

    /// <summary>
    /// Генерировать методы расширения для IDbConnection
    /// </summary>
    public bool GenerateExtensionMethods { get; init; } = false;

    /// <summary>
    /// Стратегия обработки NULL значений
    /// </summary>
    public NullHandlingStrategy NullHandling { get; init; } = NullHandlingStrategy.Nullable;

    /// <summary>
    /// Включать SQL запрос в комментарии метода
    /// </summary>
    public bool IncludeSqlInDocumentation { get; init; } = true;

    /// <summary>
    /// Генерировать prepared statements
    /// </summary>
    public bool UsePreparedStatements { get; init; } = true;

    /// <summary>
    /// Генерировать поддержку транзакций (параметр NpgsqlTransaction?)
    /// </summary>
    public bool GenerateTransactionSupport { get; init; } = true;

    /// <summary>
    /// Использовать NpgsqlDataSource для управления соединениями
    /// </summary>
    public bool UseNpgsqlDataSource { get; init; } = true;
}
