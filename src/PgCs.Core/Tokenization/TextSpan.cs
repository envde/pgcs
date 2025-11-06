namespace PgCs.Core.Tokenization;

/// <summary>
/// Позиция в тексте
/// </summary>
public readonly record struct TextSpan(int Start, int Length)
{
    public int End => Start + Length;
}