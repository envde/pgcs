namespace PgCs.Common.CodeGeneration.Query.Models;


/// <summary>
/// Настройки транзакций
/// </summary>
public sealed record TransactionOptions
{
    /// <summary>
    /// Генерировать методы с поддержкой транзакций
    /// </summary>
    public bool GenerateTransactionalMethods { get; init; } = false;

    /// <summary>
    /// Принимать NpgsqlTransaction как параметр
    /// </summary>
    public bool AcceptTransactionParameter { get; init; } = false;

    /// <summary>
    /// Уровень изоляции по умолчанию
    /// </summary>
    public string DefaultIsolationLevel { get; init; } = "ReadCommitted";

    /// <summary>
    /// Генерировать методы для работы с savepoints
    /// </summary>
    public bool GenerateSavepointMethods { get; init; } = false;
}