namespace PgCs.Core.Tokenization;

/// <summary>
/// Снимок позиции курсора
/// </summary>
public readonly record struct CursorPosition(int Position, int Line, int Column);