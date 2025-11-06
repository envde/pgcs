using PgCs.Core.Parsing.Blocks;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Экстрактор для извлечения определений индексов из SQL-скриптов
/// </summary>
public sealed class IndexExtractor : IIndexExtractor
{

    /// <inheritdoc />
    public bool CanExtract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        
        // Быстрая проверка по содержимому
        var content = block.Content;
        return content.Contains("CREATE", StringComparison.OrdinalIgnoreCase) &&
               content.Contains("INDEX", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public IndexDefinition? Extract(SqlBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        
        if (!CanExtract(block))
        {
            return null;
        }

        var content = block.Content.Trim();
        
        // Простой парсинг для обработки вложенных скобок
        var isUnique = content.StartsWith("CREATE UNIQUE", StringComparison.OrdinalIgnoreCase);
        
        // Находим имя индекса и таблицу
        var onIndex = content.IndexOf(" ON ", StringComparison.OrdinalIgnoreCase);
        if (onIndex < 0)
        {
            return null;
        }

        var indexPart = content[..onIndex].Trim();
        var afterOn = content[(onIndex + 4)..].Trim();
        
        // Извлекаем имя индекса
        var indexTokens = indexPart.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var indexName = indexTokens[^1]; // Последний токен перед ON
        
        // Извлекаем схему индекса если есть
        string? indexSchema = null;
        if (indexName.Contains('.'))
        {
            var parts = indexName.Split('.');
            indexSchema = parts[0];
            indexName = parts[1];
        }
        
        // Извлекаем имя таблицы
        var usingIndex = afterOn.IndexOf(" USING ", StringComparison.OrdinalIgnoreCase);
        var parenIndex = afterOn.IndexOf('(');
        if (parenIndex < 0)
        {
            return null;
        }
        
        var tableEnd = usingIndex > 0 && usingIndex < parenIndex ? usingIndex : parenIndex;
        var tableName = afterOn[..tableEnd].Trim();
        
        // Извлекаем схему таблицы
        string? tableSchema = null;
        if (tableName.Contains('.'))
        {
            var parts = tableName.Split('.');
            tableSchema = parts[0];
            tableName = parts[1];
        }
        
        // Извлекаем метод индексирования
        var method = IndexMethod.BTree;
        if (usingIndex > 0)
        {
            var methodStart = usingIndex + 7;
            var methodEnd = parenIndex;
            var methodText = afterOn[methodStart..methodEnd].Trim();
            method = ParseIndexMethod(methodText);
        }
        
        // Извлекаем колонки (с учётом вложенных скобок)
        var columnsText = ExtractParenthesizedContent(afterOn, parenIndex);
        if (string.IsNullOrWhiteSpace(columnsText))
        {
            return null;
        }
        
        var columns = ExtractColumns(columnsText);
        if (columns.Count == 0)
        {
            return null;
        }
        
        // Извлекаем INCLUDE columns
        var includeIndex = afterOn.IndexOf(" INCLUDE ", StringComparison.OrdinalIgnoreCase);
        IReadOnlyList<string>? includeColumns = null;
        if (includeIndex > 0)
        {
            var includeParenIndex = afterOn.IndexOf('(', includeIndex);
            if (includeParenIndex > 0)
            {
                var includeText = ExtractParenthesizedContent(afterOn, includeParenIndex);
                if (!string.IsNullOrWhiteSpace(includeText))
                {
                    includeColumns = ExtractColumns(includeText);
                }
            }
        }
        
        // Извлекаем WHERE clause
        var whereIndex = afterOn.IndexOf(" WHERE ", StringComparison.OrdinalIgnoreCase);
        string? whereClause = null;
        if (whereIndex > 0)
        {
            var whereStart = whereIndex + 7;
            var whereText = afterOn[whereStart..].Trim();
            // Удаляем точку с запятой если есть
            if (whereText.EndsWith(';'))
            {
                whereText = whereText[..^1].Trim();
            }
            whereClause = whereText;
        }
        
        var schema = tableSchema ?? indexSchema;

        return new IndexDefinition
        {
            Name = indexName,
            TableName = tableName,
            Columns = columns,
            Schema = schema,
            Method = method,
            IsUnique = isUnique,
            IsPrimary = false,
            IsPartial = whereClause is not null,
            WhereClause = whereClause,
            IncludeColumns = includeColumns,
            SqlComment = block.HeaderComment,
            RawSql = block.RawContent
        };
    }

    /// <summary>
    /// Извлекает содержимое в скобках с учётом вложенности
    /// </summary>
    private static string ExtractParenthesizedContent(string text, int startIndex)
    {
        if (startIndex < 0 || startIndex >= text.Length || text[startIndex] != '(')
        {
            return string.Empty;
        }
        
        var depth = 0;
        var start = startIndex + 1;
        
        for (int i = startIndex; i < text.Length; i++)
        {
            if (text[i] == '(')
            {
                depth++;
            }
            else if (text[i] == ')')
            {
                depth--;
                if (depth == 0)
                {
                    return text[start..i];
                }
            }
        }
        
        return string.Empty;
    }

    /// <summary>
    /// Извлекает список колонок из строки, разделённой запятыми
    /// Учитывает вложенные скобки и кавычки
    /// </summary>
    private static IReadOnlyList<string> ExtractColumns(string columnsText)
    {
        var columns = new List<string>();
        var current = new System.Text.StringBuilder();
        var depth = 0;
        var inSingleQuote = false;
        var inDoubleQuote = false;
        
        foreach (var ch in columnsText)
        {
            switch (ch)
            {
                case '(' when !inSingleQuote && !inDoubleQuote:
                    depth++;
                    current.Append(ch);
                    break;
                    
                case ')' when !inSingleQuote && !inDoubleQuote:
                    depth--;
                    current.Append(ch);
                    break;
                    
                case '\'' when !inDoubleQuote:
                    inSingleQuote = !inSingleQuote;
                    current.Append(ch);
                    break;
                    
                case '"' when !inSingleQuote:
                    inDoubleQuote = !inDoubleQuote;
                    current.Append(ch);
                    break;
                    
                case ',' when depth == 0 && !inSingleQuote && !inDoubleQuote:
                    var column = current.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(column))
                    {
                        columns.Add(column);
                    }
                    current.Clear();
                    break;
                    
                default:
                    current.Append(ch);
                    break;
            }
        }
        
        // Добавляем последнюю колонку
        var lastColumn = current.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(lastColumn))
        {
            columns.Add(lastColumn);
        }
        
        return columns;
    }

    /// <summary>
    /// Парсит метод индексирования из строки
    /// </summary>
    /// <param name="methodText">Текст метода (btree, hash, gist, gin, и т.д.)</param>
    /// <returns>Значение IndexMethod</returns>
    private static IndexMethod ParseIndexMethod(string methodText)
    {
        return methodText.ToLowerInvariant() switch
        {
            "btree" => IndexMethod.BTree,
            "hash" => IndexMethod.Hash,
            "gist" => IndexMethod.Gist,
            "gin" => IndexMethod.Gin,
            "spgist" => IndexMethod.SpGist,
            "brin" => IndexMethod.Brin,
            "bloom" => IndexMethod.Bloom,
            _ => IndexMethod.BTree // Default
        };
    }
}
