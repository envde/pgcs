using System.Text;
using System.Text.RegularExpressions;

namespace PgCs.SchemaAnalyzer.Utils;

/// <summary>
/// Разбивает SQL скрипт на отдельные выражения
/// </summary>
internal static partial class SqlStatementSplitter
{
    // Ключевые слова, которые начинают новый SQL statement
    private static readonly HashSet<string> StatementStartKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "CREATE", "ALTER", "DROP", "TRUNCATE", "INSERT", "UPDATE", "DELETE", 
        "SELECT", "WITH", "GRANT", "REVOKE", "COMMENT", "SET", "RESET"
    };
    public static IReadOnlyList<string> Split(string sqlScript)
    {
        if (string.IsNullOrWhiteSpace(sqlScript))
            return [];

        var statements = new List<string>();
        var currentStatement = new StringBuilder();
        var inString = false;
        var inDollarQuote = false;
        var dollarQuoteTag = string.Empty;
        var consecutiveNewlines = 0;

        for (var i = 0; i < sqlScript.Length; i++)
        {
            var currentChar = sqlScript[i];

            switch (currentChar)
            {
                // Обработка строковых литералов
                case '\'' when !inDollarQuote:
                    inString = !inString;
                    consecutiveNewlines = 0;
                    currentStatement.Append(currentChar);
                    continue;
                // Обработка dollar-quoted строк ($$текст$$ или $tag$текст$tag$)
                case '$' when !inString:
                {
                    var tag = ExtractDollarQuoteTag(sqlScript, i);
                    if (!string.IsNullOrEmpty(tag))
                    {
                        if (!inDollarQuote)
                        {
                            inDollarQuote = true;
                            dollarQuoteTag = tag;
                            currentStatement.Append(tag);
                            i += tag.Length - 1;
                        }
                        else if (tag == dollarQuoteTag)
                        {
                            inDollarQuote = false;
                            currentStatement.Append(tag);
                            i += tag.Length - 1;
                            dollarQuoteTag = string.Empty;
                        }
                        else
                        {
                            currentStatement.Append(currentChar);
                        }
                        consecutiveNewlines = 0;
                        continue;
                    }

                    consecutiveNewlines = 0;
                    currentStatement.Append(currentChar);
                    continue;
                }
                // Разделитель - точка с запятой
                case ';' when !inString && !inDollarQuote:
                {
                    var statement = currentStatement.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(statement))
                    {
                        statements.Add(statement);
                    }
                    currentStatement.Clear();
                    consecutiveNewlines = 0;
                    continue;
                }
                // Игнорируем \r, обрабатываем только \n
                case '\r':
                    continue;
                // Отслеживание переводов строк для разделения statements
                case '\n' when !inString && !inDollarQuote:
                {
                    consecutiveNewlines++;
                    
                    // Проверяем, начинается ли следующая строка с ключевого слова
                    if (consecutiveNewlines >= 1 && currentStatement.Length > 0)
                    {
                        var nextNonWhitespacePos = FindNextNonWhitespace(sqlScript, i + 1);
                        if (nextNonWhitespacePos != -1 && IsStatementStart(sqlScript, nextNonWhitespacePos))
                        {
                            var statement = currentStatement.ToString().Trim();
                            if (!string.IsNullOrWhiteSpace(statement))
                            {
                                statements.Add(statement);
                                currentStatement.Clear();
                            }
                            consecutiveNewlines = 0;
                            continue;
                        }
                    }
                    
                    // Добавляем \n только если не было разделения
                    currentStatement.Append(currentChar);
                    
                    // Если две подряд новые строки (пустая строка) - разделяем statements
                    if (consecutiveNewlines >= 2)
                    {
                        var statement = currentStatement.ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(statement))
                        {
                            statements.Add(statement);
                            currentStatement.Clear();
                        }
                        consecutiveNewlines = 0;
                    }
                    continue;
                }
                // Пробелы и табы не сбрасывают счетчик новых строк
                case ' ':
                case '\t':
                    currentStatement.Append(currentChar);
                    continue;
            }

            // Любой другой символ сбрасывает счетчик новых строк
            consecutiveNewlines = 0;
            currentStatement.Append(currentChar);
        }

        // Добавляем последнее выражение, если оно есть
        var lastStatement = currentStatement.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(lastStatement))
        {
            statements.Add(lastStatement);
        }

        return statements;
    }

    private static string ExtractDollarQuoteTag(string sql, int startIndex)
    {
        var endIndex = sql.IndexOf('$', startIndex + 1);
        if (endIndex == -1)
            return string.Empty;

        var tag = sql[startIndex..(endIndex + 1)];
        return tag;
    }

    private static int FindNextNonWhitespace(string sql, int startIndex)
    {
        for (var i = startIndex; i < sql.Length; i++)
        {
            if (!char.IsWhiteSpace(sql[i]))
            {
                return i;
            }
        }
        return -1;
    }

    private static bool IsStatementStart(string sql, int position)
    {
        // Извлекаем слово, начинающееся с позиции position
        var wordEnd = position;
        while (wordEnd < sql.Length && (char.IsLetterOrDigit(sql[wordEnd]) || sql[wordEnd] == '_'))
        {
            wordEnd++;
        }

        if (wordEnd == position)
            return false;

        var word = sql[position..wordEnd];
        
        // Не разделяем на SELECT/WITH внутри CREATE VIEW/MATERIALIZED VIEW
        if (word.Equals("SELECT", StringComparison.OrdinalIgnoreCase) || 
            word.Equals("WITH", StringComparison.OrdinalIgnoreCase))
        {
            // Ищем CREATE VIEW/MATERIALIZED VIEW перед этой позицией
            var precedingText = sql[..position];
            if (precedingText.Contains("CREATE VIEW", StringComparison.OrdinalIgnoreCase) ||
                precedingText.Contains("CREATE MATERIALIZED VIEW", StringComparison.OrdinalIgnoreCase))
            {
                // Проверяем, что после CREATE VIEW еще не было точки с запятой
                var lastSemicolon = precedingText.LastIndexOf(';');
                var lastCreateView = Math.Max(
                    precedingText.LastIndexOf("CREATE VIEW", StringComparison.OrdinalIgnoreCase),
                    precedingText.LastIndexOf("CREATE MATERIALIZED VIEW", StringComparison.OrdinalIgnoreCase)
                );
                
                if (lastCreateView > lastSemicolon)
                {
                    return false; // SELECT/WITH является частью CREATE VIEW
                }
            }
        }
        
        return StatementStartKeywords.Contains(word);
    }
}