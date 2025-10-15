namespace PgCs.Common.CodeGeneration.Formatting.Models.Enums;

/// <summary>
/// Стиль размещения фигурных скобок
/// </summary>
public enum BraceStyle
{
    /// <summary>
    /// На той же строке
    /// </summary>
    SameLine,

    /// <summary>
    /// На следующей строке (Allman style, рекомендуется для C#)
    /// </summary>
    NextLine
}