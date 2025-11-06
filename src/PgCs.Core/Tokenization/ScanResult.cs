namespace PgCs.Core.Tokenization;

/// <summary>
/// Результат сканирования токена
/// </summary>
public readonly record struct ScanResult(TokenType Type, int Length);