using PgCs.Common.Generation.Models;

namespace PgCs.Common.QueryGenerator.Models;

/// <summary>
/// Опции генерации методов запросов
/// </summary>
public sealed record QueryGenerationOptions : GenerationOptions
{
    /// <summary>
    /// Имя класса для группировки методов (если не указано, используется "Queries")
    /// </summary>
    public string ClassName { get; init; } = "Queries";

    /// <summary>
    /// Генерировать асинхронные методы
    /// </summary>
    public bool GenerateAsyncMethods { get; init; } = true;

    /// <summary>
    /// Генерировать синхронные методы (в дополнение к асинхронным)
    /// </summary>
    public bool GenerateSyncMethods { get; init; } = false;

    /// <summary>
    /// Использовать ValueTask вместо Task
    /// </summary>
    public bool UseValueTask { get; init; } = true;

    /// <summary>
    /// Генерировать отдельные модели для результатов запросов
    /// </summary>
    public bool GenerateResultModels { get; init; } = true;

    /// <summary>
    /// Генерировать отдельные модели для параметров запросов
    /// </summary>
    public bool GenerateParameterModels { get; init; } = false;

    /// <summary>
    /// Использовать record для моделей результатов
    /// </summary>
    public bool UseRecordsForResults { get; init; } = true;

    /// <summary>
    /// Тип подключения к БД (Npgsql, Dapper и т.д.)
    /// </summary>
    public DatabaseProvider DatabaseProvider { get; init; } = DatabaseProvider.Npgsql;

    /// <summary>
    /// Генерировать интерфейс для класса запросов
    /// </summary>
    public bool GenerateInterface { get; init; } = true;

    /// <summary>
    /// Использовать dependency injection
    /// </summary>
    public bool UseDependencyInjection { get; init; } = true;

    /// <summary>
    /// Генерировать методы с CancellationToken
    /// </summary>
    public bool SupportCancellation { get; init; } = true;

    /// <summary>
    /// Стратегия именования методов
    /// </summary>
    public MethodNamingStrategy MethodNamingStrategy { get; init; } = MethodNamingStrategy.PascalCase;

    /// <summary>
    /// Префикс для имён методов
    /// </summary>
    public string? MethodPrefix { get; init; }

    /// <summary>
    /// Суффикс для имён методов (например, "Async")
    /// </summary>
    public string MethodSuffix { get; init; } = "Async";

    /// <summary>
    /// Дополнительные using директивы
    /// </summary>
    public new IReadOnlyList<string> AdditionalUsings { get; init; } = [];

    /// <summary>
    /// Генерировать логирование
    /// </summary>
    public bool GenerateLogging { get; init; } = false;

    /// <summary>
    /// Генерировать обработку ошибок
    /// </summary>
    public bool GenerateErrorHandling { get; init; } = true;

    /// <summary>
    /// Генерировать транзакционные методы
    /// </summary>
    public bool GenerateTransactionSupport { get; init; } = false;
}

/// <summary>
/// Провайдер базы данных
/// </summary>
public enum DatabaseProvider
{
    /// <summary>
    /// Npgsql (рекомендуется для PostgreSQL)
    /// </summary>
    Npgsql,

    /// <summary>
    /// Dapper поверх Npgsql
    /// </summary>
    Dapper,

    /// <summary>
    /// Entity Framework Core
    /// </summary>
    EntityFrameworkCore
}

/// <summary>
/// Стратегия именования методов
/// </summary>
public enum MethodNamingStrategy
{
    /// <summary>
    /// PascalCase (GetUserById)
    /// </summary>
    PascalCase,

    /// <summary>
    /// camelCase (getUserById)
    /// </summary>
    CamelCase,

    /// <summary>
    /// Без изменений (сохранить имя из аннотации)
    /// </summary>
    AsIs
}
