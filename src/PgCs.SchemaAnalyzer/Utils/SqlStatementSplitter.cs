using System.Text;

namespace PgCs.SchemaAnalyzer.Utils;

/// <summary>
/// Разбивает SQL скрипт на отдельные выражения
/// </summary>
internal static class SqlStatementSplitter
{
    public static IReadOnlyList<string> Split(string sqlScript)
    {
        if (string.IsNullOrWhiteSpace(sqlScript))
            return [];

        var statements = new List<string>();
        var currentStatement = new StringBuilder();
        var inString = false;
        var inDollarQuote = false;
        var dollarQuoteTag = string.Empty;

        for (var i = 0; i < sqlScript.Length; i++)
        {
            var currentChar = sqlScript[i];

            switch (currentChar)
            {
                // Обработка строковых литералов
                case '\'' when !inDollarQuote:
                    inString = !inString;
                    break;
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
                        continue;
                    }

                    break;
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
                    continue;
                }
            }

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
}