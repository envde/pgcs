namespace PgCs.Core.Extraction.Block;

/// <summary>
/// Парсер SQL скрипта на отдельные блоки команд.
/// Разделяет SQL текст по точкам с запятой, сохраняя комментарии и исходное форматирование.
/// </summary>
public sealed class BlockExtractor : IBlockExtractor
{
    private readonly CommentProcessor _commentProcessor = new CommentProcessor();

    /// <summary>
    /// Извлекает блоки команд из SQL текста.
    /// Блоки разделяются точками с запятой, комментарии связываются с соответствующими блоками.
    /// </summary>
    /// <param name="sql">SQL текст для парсинга</param>
    /// <returns>Упорядоченный список SQL блоков</returns>
    /// <exception cref="ArgumentException">Если SQL текст пуст или содержит только пробелы</exception>
    public IReadOnlyList<SqlBlock> Extract(string sql)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        var lines = sql.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
        var blocks = new List<SqlBlock>();
        var accumulator = new BlockAccumulator();
        var state = new BlockExtractorState();

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var lineNumber = i + 1;

            ProcessLine(line, lineNumber, accumulator, state, blocks);
        }

        // Завершающий блок без точки с запятой
        if (state.IsInsideBlock && !accumulator.IsEmpty)
        {
            blocks.Add(accumulator.BuildBlock(lines.Length));
        }

        return blocks;
    }

    /// <summary>
    /// Обрабатывает одну строку SQL текста.
    /// </summary>
    public void ProcessLine(
        string line,
        int lineNumber,
        BlockAccumulator accumulator,
        BlockExtractorState state,
        List<SqlBlock> blocks)
    {
        var trimmed = line.AsSpan().Trim();

        // Пустая строка - завершает текущий блок
        if (trimmed.IsWhiteSpace() || trimmed.IsEmpty)
        {
            HandleEmptyLine(accumulator, state, blocks, lineNumber);
            return;
        }

        // Строка с комментарием
        if (_commentProcessor.IsCommentLine(line))
        {
            HandleCommentLine(line, accumulator, state);
            return;
        }

        // SQL команда
        HandleSqlLine(line, lineNumber, trimmed, accumulator, state, blocks);
    }

    /// <summary>
    /// Обрабатывает пустую строку.
    /// </summary>
    public void HandleEmptyLine(
        BlockAccumulator accumulator,
        BlockExtractorState state,
        List<SqlBlock> blocks,
        int lineNumber)
    {
        if (state.IsInsideBlock && !accumulator.IsEmpty)
        {
            blocks.Add(accumulator.BuildBlock(lineNumber - 1));
            accumulator.Reset();
            state.ExitBlock();
        }
        else if (accumulator.HasHeaderComments)
        {
            state.MarkEmptyLineAfterComment();
        }
    }

    /// <summary>
    /// Обрабатывает строку с комментарием.
    /// </summary>
    public void HandleCommentLine(string line, BlockAccumulator accumulator, BlockExtractorState state)
    {
        if (!state.IsInsideBlock)
        {
            // Комментарий перед блоком
            if (state.HasEmptyLineAfterComment)
            {
                accumulator.ClearHeaderComments();
                state.ResetEmptyLineFlag();
            }
            
            var commentText = _commentProcessor.ExtractCommentText(line);
            accumulator.AddHeaderComment(commentText);
        }
        else
        {
            // Строка с комментарием внутри блока (полностью комментарий, не inline)
            accumulator.AddRawLine(line);
        }
    }

    /// <summary>
    /// Обрабатывает строку с SQL кодом.
    /// </summary>
    public void HandleSqlLine(
        string line,
        int lineNumber,
        ReadOnlySpan<char> trimmed,
        BlockAccumulator accumulator,
        BlockExtractorState state,
        List<SqlBlock> blocks)
    {
        // Начало нового блока
        if (!state.IsInsideBlock)
        {
            state.EnterBlock(lineNumber);
            accumulator.StartBlock(lineNumber);
        }

        accumulator.AddRawLine(line);

        // Обработка inline комментариев
        var (code, inlineComment) = _commentProcessor.SplitInlineComment(line);
        if (inlineComment is not null)
        {
            // Вычисляем позицию ПЕРЕД добавлением строки в Content
            var position = accumulator.CalculateCurrentPosition();
            accumulator.AddContentLine(code);
            accumulator.AddInlineComment(position, code, inlineComment);
        }
        else
        {
            accumulator.AddContentLine(line);
        }

        // Проверка завершения блока (точка с запятой в конце)
        if (trimmed.EndsWith(";"))
        {
            blocks.Add(accumulator.BuildBlock(lineNumber));
            accumulator.Reset();
            state.ExitBlock();
        }
    }
}
