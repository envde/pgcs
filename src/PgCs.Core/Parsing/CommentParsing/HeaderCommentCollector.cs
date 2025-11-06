namespace PgCs.Core.Parsing.CommentParsing;

/// <summary>
/// Сборщик header комментариев перед SQL блоком
/// Отслеживает комментарии до начала блока и правила их сброса
/// </summary>
public sealed class HeaderCommentCollector
{
    private readonly List<string> _comments = [];
    private bool _hasEmptyLineAfterComment;

    /// <summary>
    /// Есть ли накопленные комментарии
    /// </summary>
    public bool HasComments => _comments.Count > 0;

    /// <summary>
    /// Накопленные комментарии
    /// </summary>
    public IReadOnlyList<string> Comments => _comments;

    /// <summary>
    /// Добавляет комментарий
    /// </summary>
    public void AddComment(string comment)
    {
        if (_hasEmptyLineAfterComment)
        {
            // Пустая строка перед комментарием - сбрасываем старые
            _comments.Clear();
            _hasEmptyLineAfterComment = false;
        }

        _comments.Add(comment);
    }

    /// <summary>
    /// Отмечает наличие пустой строки после комментария
    /// </summary>
    public void MarkEmptyLine()
    {
        _hasEmptyLineAfterComment = true;
    }

    /// <summary>
    /// Извлекает объединенный комментарий и очищает состояние
    /// </summary>
    public string? ExtractAndClear()
    {
        if (_comments.Count == 0)
        {
            return null;
        }

        var result = string.Join(" ", _comments);
        Clear();
        return result;
    }

    /// <summary>
    /// Очищает все комментарии
    /// </summary>
    public void Clear()
    {
        _comments.Clear();
        _hasEmptyLineAfterComment = false;
    }
}
