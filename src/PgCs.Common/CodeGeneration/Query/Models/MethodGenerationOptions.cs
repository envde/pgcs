

using PgCs.Common.CodeGeneration.Query.Models.Enums;

namespace PgCs.Common.CodeGeneration.Query.Models;

/// <summary>
/// Настройки генерации методов запросов
/// </summary>
public sealed record MethodGenerationOptions
{
    /// <summary>
    /// Пространство имен для сгенерированных репозиториев
    /// </summary>
    public string Namespace { get; init; } = "Generated.Repositories";

    /// <summary>
    /// Режим генерации методов (синхронные/асинхронные/оба)
    /// </summary>
    public MethodGenerationMode Mode { get; init; } = MethodGenerationMode.AsyncOnly;

    /// <summary>
    /// Возвращаемый тип для асинхронных методов
    /// </summary>
    public AsyncReturnType AsyncReturnType { get; init; } = AsyncReturnType.ValueTask;

    /// <summary>
    /// Использовать CancellationToken в методах
    /// </summary>
    public bool UseCancellationToken { get; init; } = true;

    /// <summary>
    /// Положение CancellationToken в списке параметров
    /// </summary>
    public CancellationTokenPosition CancellationTokenPosition { get; init; } =
        CancellationTokenPosition.Last;

    /// <summary>
    /// Использовать IAsyncEnumerable для методов :many
    /// </summary>
    public bool UseAsyncEnumerable { get; init; } = false;

    /// <summary>
    /// Настройки именования методов
    /// </summary>
    public MethodNamingOptions Naming { get; init; } = new();

    /// <summary>
    /// Генерировать интерфейсы для репозиториев
    /// </summary>
    public bool GenerateInterfaces { get; init; } = true;

    /// <summary>
    /// Режим доступа к данным
    /// </summary>
    public DataAccessMode DataAccessMode { get; init; } = DataAccessMode.Npgsql;

    /// <summary>
    /// Настройки для батч операций
    /// </summary>
    public BatchOperationOptions BatchOperations { get; init; } = new();

    /// <summary>
    /// Настройки транзакций
    /// </summary>
    public TransactionOptions Transactions { get; init; } = new();

    /// <summary>
    /// Настройки обработки ошибок
    /// </summary>
    public ErrorHandlingOptions ErrorHandling { get; init; } = new();

    /// <summary>
    /// Добавлять логирование (ILogger)
    /// </summary>
    public bool AddLogging { get; init; } = true;

    /// <summary>
    /// Добавлять метрики/телеметрию (Activity)
    /// </summary>
    public bool AddTelemetry { get; init; } = false;

    /// <summary>
    /// Генерировать XML комментарии
    /// </summary>
    public bool GenerateXmlComments { get; init; } = true;

    /// <summary>
    /// Использовать интерполированные строки для SQL (C# 10+)
    /// </summary>
    public bool UseInterpolatedSqlStrings { get; init; } = false;

    /// <summary>
    /// Генерировать partial методы для расширения
    /// </summary>
    public bool GeneratePartialMethods { get; init; } = true;

    /// <summary>
    /// Использовать file-scoped namespaces
    /// </summary>
    public bool UseFileScopedNamespaces { get; init; } = true;

    /// <summary>
    /// Генерировать один файл или разделять по классам
    /// </summary>
    public bool GenerateSeparateFiles { get; init; } = true;

    /// <summary>
    /// Добавлять SQL запрос в комментарии метода
    /// </summary>
    public bool IncludeSqlInComments { get; init; } = true;

    /// <summary>
    /// Использовать NpgsqlDataSource (рекомендуется для Npgsql 7+)
    /// </summary>
    public bool UseNpgsqlDataSource { get; init; } = true;

    /// <summary>
    /// Генерировать методы для prepared statements
    /// </summary>
    public bool GeneratePreparedStatements { get; init; } = false;
}