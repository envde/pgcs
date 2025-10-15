using System.Text;
using System.Text.RegularExpressions;

namespace PgCs.SchemaAnalyzer.Utils;

/// <summary>
/// Нормализует SQL скрипт для парсинга
/// </summary>
internal static partial class SqlNormalizer
{
    [GeneratedRegex(@"--[^\r\n]*", RegexOptions.Compiled)]
    private static partial Regex SingleLineCommentRegex();

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex MultiLineCommentRegex();

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex WhitespaceRegex();

    public static string Normalize(string sqlScript)
    {
        if (string.IsNullOrWhiteSpace(sqlScript))
            return string.Empty;

        // Сохраняем COMMENT ON для дальнейшей обработки
        var commentOnStatements = ExtractCommentOnStatements(sqlScript);

        // Удаляем обычные комментарии
        var normalized = SingleLineCommentRegex().Replace(sqlScript, " ");
        normalized = MultiLineCommentRegex().Replace(normalized, " ");

        // Нормализуем пробелы
        normalized = WhitespaceRegex().Replace(normalized, " ");
        normalized = normalized.Trim();

        return normalized;
    }

    public static string PreserveComments(string sqlScript)
    {
        if (string.IsNullOrWhiteSpace(sqlScript))
            return string.Empty;

        return WhitespaceRegex().Replace(sqlScript, " ").Trim();
    }

    private static List<string> ExtractCommentOnStatements(string sqlScript)
    {
        var pattern = @"COMMENT\s+ON\s+(?:TABLE|COLUMN|VIEW|FUNCTION|TYPE)\s+[^;]+;";
        var matches = Regex.Matches(sqlScript, pattern, RegexOptions.IgnoreCase);
        return matches.Select(m => m.Value).ToList();
    }
}