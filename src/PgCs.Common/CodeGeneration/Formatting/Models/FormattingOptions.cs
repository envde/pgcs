using PgCs.Common.CodeGeneration.Formatting.Models.Enums;

namespace PgCs.Common.CodeGeneration.Formatting.Models;

/// <summary>
/// Опции форматирования C# кода
/// </summary>
public sealed record FormattingOptions
{
    /// <summary>
    /// Стиль отступов
    /// </summary>
    public IndentStyle IndentStyle { get; init; } = IndentStyle.Spaces;

    /// <summary>
    /// Размер отступа
    /// </summary>
    public int IndentSize { get; init; } = 4;

    /// <summary>
    /// Добавлять пустую строку между членами класса
    /// </summary>
    public bool AddBlankLinesBetweenMembers { get; init; } = true;

    /// <summary>
    /// Сортировать using директивы
    /// </summary>
    public bool SortUsings { get; init; } = true;

    /// <summary>
    /// Удалять неиспользуемые using
    /// </summary>
    public bool RemoveUnusedUsings { get; init; } = true;

    /// <summary>
    /// Использовать file-scoped namespaces (C# 10+)
    /// </summary>
    public bool UseFileScopedNamespaces { get; init; } = true;

    /// <summary>
    /// Стиль скобок
    /// </summary>
    public BraceStyle BraceStyle { get; init; } = BraceStyle.NextLine;

    /// <summary>
    /// Максимальная длина строки
    /// </summary>
    public int MaxLineLength { get; init; } = 120;

    /// <summary>
    /// Добавлять пробел перед открывающей скобкой
    /// </summary>
    public bool SpaceBeforeOpenBrace { get; init; } = true;

    /// <summary>
    /// Использовать var где возможно
    /// </summary>
    public bool UseImplicitTyping { get; init; } = false;

    /// <summary>
    /// Форматировать в соответствии с .editorconfig
    /// </summary>
    public bool RespectEditorConfig { get; init; } = true;
}