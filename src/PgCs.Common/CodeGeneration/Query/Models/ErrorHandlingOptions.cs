namespace PgCs.Common.CodeGeneration.Query.Models;

/// <summary>
/// Настройки обработки ошибок
/// </summary>
public sealed record ErrorHandlingOptions
{
    /// <summary>
    /// Оборачивать PostgresException в кастомные исключения
    /// </summary>
    public bool WrapExceptions { get; init; } = false;

    /// <summary>
    /// Имя базового класса для кастомных исключений
    /// </summary>
    public string? CustomExceptionBase { get; init; }

    /// <summary>
    /// Добавлять retry политики (требует Polly)
    /// </summary>
    public bool AddRetryPolicies { get; init; } = false;

    /// <summary>
    /// Количество повторов по умолчанию
    /// </summary>
    public int DefaultRetryCount { get; init; } = 3;

    /// <summary>
    /// Генерировать методы для обработки специфичных ошибок PG
    /// </summary>
    public bool HandlePostgresErrors { get; init; } = true;

    /// <summary>
    /// Логировать ошибки автоматически
    /// </summary>
    public bool AutoLogErrors { get; init; } = true;
}