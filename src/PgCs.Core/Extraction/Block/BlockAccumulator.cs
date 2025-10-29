namespace PgCs.Core.Extraction.Block;

/// <summary>
/// Аккумулятор для построения SQL блока.
/// Собирает строки кода, комментарии и метаданные в процессе парсинга.
/// </summary>
public sealed class BlockAccumulator
{
    private readonly List<string> _contentLines = [];
    private readonly List<string> _rawLines = [];
    private readonly List<string> _headerComments = [];
    private readonly List<InlineComment> _inlineComments = [];
    
    private int _startLine = -1;

    /// <summary>
    /// Проверяет, пуст ли аккумулятор (нет SQL кода).
    /// </summary>
    public bool IsEmpty => _contentLines.Count == 0;

    /// <summary>
    /// Проверяет, есть ли комментарии перед блоком.
    /// </summary>
    public bool HasHeaderComments => _headerComments.Count > 0;

    /// <summary>
    /// Устанавливает номер начальной строки блока.
    /// </summary>
    public void StartBlock(int lineNumber)
    {
        _startLine = lineNumber;
    }

    /// <summary>
    /// Добавляет комментарий перед блоком (header comment).
    /// </summary>
    public void AddHeaderComment(string comment)
    {
        _headerComments.Add(comment);
    }

    /// <summary>
    /// Очищает все накопленные header комментарии.
    /// Используется когда между комментарием и блоком есть пустая строка.
    /// </summary>
    public void ClearHeaderComments()
    {
        _headerComments.Clear();
    }

    /// <summary>
    /// Добавляет строку чистого SQL кода (без комментариев).
    /// </summary>
    public void AddContentLine(string line)
    {
        _contentLines.Add(line);
    }

    /// <summary>
    /// Добавляет строку исходного текста (с комментариями и форматированием).
    /// </summary>
    public void AddRawLine(string line)
    {
        _rawLines.Add(line);
    }

    /// <summary>
    /// Добавляет inline комментарий на указанной позиции в коде.
    /// </summary>
    /// <param name="position">Позиция комментария в тексте Content</param>
    /// <param name="codeBeforeComment">Код перед комментарием для извлечения ключа</param>
    /// <param name="comment">Текст комментария</param>
    public void AddInlineComment(int position, string codeBeforeComment, string comment)
    {
        var key = ExtractKeyFromCode(codeBeforeComment);
        _inlineComments.Add(new InlineComment
        {
            Key = key,
            Comment = comment,
            Position = position
        });
    }

    /// <summary>
    /// Извлекает ключ (имя поля, параметра) из кода перед комментарием.
    /// </summary>
    /// <param name="code">Код перед комментарием</param>
    /// <returns>Последний идентификатор в коде</returns>
    private static string ExtractKeyFromCode(string code)
    {
        // Убираем лишние пробелы и запятые в конце
        var trimmed = code.Trim().TrimEnd(',', ' ', '\t');
        
        // Ищем последнее слово (идентификатор)
        var parts = trimmed.Split([' ', '\t', '\n', '\r', ',', '(', ')'], StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[^1] : trimmed;
    }

    /// <summary>
    /// Строит готовый SQL блок из накопленных данных.
    /// </summary>
    /// <param name="endLine">Номер конечной строки блока</param>
    /// <returns>Сформированный SQL блок</returns>
    public SqlBlock BuildBlock(int endLine)
    {
        var headerComment = _headerComments.Count > 0 
            ? string.Join(" ", _headerComments) 
            : null;

        var content = string.Join(Environment.NewLine, _contentLines).Trim();
        
        // RawContent должен включать header комментарии, если они есть
        var rawContent = string.Join(Environment.NewLine, _rawLines).Trim();
        if (headerComment is not null)
        {
            var headerLines = _headerComments.Select(c => $"-- {c}");
            var headerBlock = string.Join(Environment.NewLine, headerLines);
            rawContent = $"{headerBlock}{Environment.NewLine}{rawContent}";
        }

        return new SqlBlock
        {
            Content = content,
            RawContent = rawContent,
            HeaderComment = headerComment,
            InlineComments = _inlineComments.Count > 0 
                ? new List<InlineComment>(_inlineComments) 
                : null,
            StartLine = _startLine,
            EndLine = endLine
        };
    }

    /// <summary>
    /// Сбрасывает состояние аккумулятора для начала накопления нового блока.
    /// </summary>
    public void Reset()
    {
        _contentLines.Clear();
        _rawLines.Clear();
        _headerComments.Clear();
        _inlineComments.Clear();
        _startLine = -1;
    }

    /// <summary>
    /// Вычисляет текущую позицию в накопленном коде для inline комментария.
    /// </summary>
    public int CalculateCurrentPosition()
    {
        return _contentLines.Sum(line => line.Length + Environment.NewLine.Length);
    }
}