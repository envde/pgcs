using System.Text.RegularExpressions;

namespace PgCs.SchemaAnalyzer.Tante;

/// <summary>
/// Детектор типа объекта схемы PostgreSQL по SQL блоку
/// </summary>
public static partial class SchemaDefinitionDetector
{
    // TODO: Нужно реализовать определения объекта, то есть к какому типу Definition относится SqlBlock
    
    [GeneratedRegex(@"^\s*CREATE\s+TYPE\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CreateTypePattern();

    [GeneratedRegex(@"\s+AS\s+ENUM\s*\(", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex EnumDefinitionPattern();

    [GeneratedRegex(@"^\s*COMMENT\s+ON\s+TYPE\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CommentTypePattern();

    [GeneratedRegex(@"^\s*CREATE\s+TABLE\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CreateTablePattern();

    [GeneratedRegex(@"^\s*CREATE\s+.*VIEW\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CreateViewPattern();

    [GeneratedRegex(@"^\s*CREATE\s+.*FUNCTION\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex CreateFunctionPattern();
    
    // TODO: Добавить остальные [GeneratedRegex] для оставшихся типов Schema.sql (src/PgCs.Core/Example/Schema.sql)
}
