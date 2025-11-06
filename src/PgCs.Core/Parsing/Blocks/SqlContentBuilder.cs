using System.Buffers;
using PgCs.Core.Tokenization;

namespace PgCs.Core.Parsing.Blocks;

/// <summary>
/// Строитель SQL Content из токенов с оптимизацией памяти для .NET 9
/// Использует string.Create и ArrayPool для zero-allocation построения строк
/// </summary>
public sealed class SqlContentBuilder
{
    /// <summary>
    /// Строит Content из токенов (без trivia)
    /// Добавляет пробелы между токенами где необходимо
    /// </summary>
    public string Build(IReadOnlyList<SqlToken> tokens)
    {
        if (tokens.Count == 0)
        {
            return string.Empty;
        }

        // Фаза 1: Подсчитываем длину и количество значащих токенов
        var (totalLength, significantCount) = CalculateLengthAndCount(tokens);

        if (totalLength == 0)
        {
            return string.Empty;
        }

        // Фаза 2: Арендуем буфер для индексов
        var rentedBuffer = ArrayPool<int>.Shared.Rent(significantCount);

        try
        {
            // Фаза 3: Собираем индексы значащих токенов
            CollectSignificantIndices(tokens, rentedBuffer.AsSpan(0, significantCount));

            // Фаза 4: Строим строку через string.Create (zero extra allocations)
            return string.Create(totalLength, (tokens, rentedBuffer, significantCount, this), static (span, state) =>
            {
                var (tokens, indices, count, builder) = state;
                var position = 0;
                SqlToken? prevToken = null;

                for (int i = 0; i < count; i++)
                {
                    var token = tokens[indices[i]];

                    // Добавляем пробел между токенами если нужно
                    if (prevToken.HasValue && builder.NeedsSpaceBetween(prevToken.Value, token))
                    {
                        span[position++] = ' ';
                    }

                    // Копируем токен в результирующий span
                    token.ValueMemory.Span.CopyTo(span[position..]);
                    position += token.ValueMemory.Length;

                    prevToken = token;
                }
            });
        }
        finally
        {
            // Возвращаем арендованный буфер в pool
            ArrayPool<int>.Shared.Return(rentedBuffer);
        }
    }

    /// <summary>
    /// Вычисляет итоговую длину и количество значащих токенов (pass 1)
    /// </summary>
    private (int TotalLength, int SignificantCount) CalculateLengthAndCount(IReadOnlyList<SqlToken> tokens)
    {
        var totalLength = 0;
        var significantCount = 0;
        SqlToken? prevToken = null;

        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];

            // Пропускаем trivia
            if (token.IsTrivia())
            {
                continue;
            }

            // Добавляем длину токена
            totalLength += token.ValueMemory.Length;

            // Добавляем +1 для пробела если нужен
            if (prevToken.HasValue && NeedsSpaceBetween(prevToken.Value, token))
            {
                totalLength++;
            }

            significantCount++;
            prevToken = token;
        }

        return (totalLength, significantCount);
    }

    /// <summary>
    /// Собирает индексы значащих токенов (pass 2)
    /// </summary>
    private static void CollectSignificantIndices(IReadOnlyList<SqlToken> tokens, Span<int> indices)
    {
        var position = 0;

        for (int i = 0; i < tokens.Count; i++)
        {
            if (!tokens[i].IsTrivia())
            {
                indices[position++] = i;
            }
        }
    }

    /// <summary>
    /// Определяет, нужен ли пробел между двумя токенами
    /// </summary>
    public bool NeedsSpaceBetween(SqlToken prev, SqlToken current)
    {
        // Не нужен пробел между ключевым словом и открывающей скобкой: CREATE TABLE(
        if (prev.IsKeyword() && current.Type == TokenType.OpenParen)
        {
            return false;
        }

        // Не нужен пробел перед пунктуацией
        if (current.Type is TokenType.Comma or TokenType.Semicolon
            or TokenType.CloseParen or TokenType.Dot)
        {
            return false;
        }

        // Не нужен пробел после открывающих скобок
        if (prev.Type == TokenType.OpenParen)
        {
            return false;
        }

        // Не нужен пробел перед и после точки: schema.table
        if (prev.Type == TokenType.Dot || current.Type == TokenType.Dot)
        {
            return false;
        }

        // Не нужен пробел с оператором :: (PostgreSQL cast)
        if (prev.Type == TokenType.Operator && IsDoubleColon(prev.ValueMemory.Span))
        {
            return false;
        }

        if (current.Type == TokenType.Operator && IsDoubleColon(current.ValueMemory.Span))
        {
            return false;
        }

        // Не нужен пробел после @ в операторах @>, <@, etc.
        if (prev.Type == TokenType.Operator && prev.ValueMemory.Length > 0 && prev.ValueMemory.Span[0] == '@')
        {
            return false;
        }

        // В остальных случаях нужен пробел
        return true;
    }

    private static bool IsDoubleColon(ReadOnlySpan<char> span)
    {
        return span.Length == 2 && span[0] == ':' && span[1] == ':';
    }
}
