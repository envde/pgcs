namespace PgCs.Core.Tokenization;

/// <summary>
/// Курсор для навигации по тексту
/// Предоставляет удобный API для токенизации
/// </summary>
public sealed class TextCursor
{
    private readonly string _text;
    private int _position;
    private int _line = 1;
    private int _column = 1;

    public TextCursor(string text)
    {
        _text = text;
    }

    /// <summary>Текущая позиция в тексте</summary>
    public int Position => _position;

    /// <summary>Текущая строка (с 1)</summary>
    public int Line => _line;

    /// <summary>Текущая колонка (с 1)</summary>
    public int Column => _column;

    /// <summary>Текущий символ</summary>
    public char Current => IsAtEnd() ? '\0' : _text[_position];

    /// <summary>Следующий символ (не перемещая курсор)</summary>
    public char Peek() => _position + 1 >= _text.Length ? '\0' : _text[_position + 1];

    /// <summary>Достигнут ли конец текста</summary>
    public bool IsAtEnd() => _position >= _text.Length;

    /// <summary>Перемещает курсор на следующий символ</summary>
    public char Advance()
    {
        var ch = _text[_position];
        _position++;

        if (ch == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }

        return ch;
    }

    /// <summary>Проверяет, совпадает ли текущая позиция с последовательностью</summary>
    public bool MatchSequence(ReadOnlySpan<char> sequence)
    {
        if (_position + sequence.Length > _text.Length)
        {
            return false;
        }

        return _text.AsSpan(_position, sequence.Length).SequenceEqual(sequence);
    }

    /// <summary>Извлекает подстроку как Span (zero-allocation)</summary>
    public ReadOnlySpan<char> GetTextSpan(int start, int length)
    {
        if (start + length > _text.Length)
        {
            length = _text.Length - start;
        }

        return _text.AsSpan(start, length);
    }

    /// <summary>Создаёт снимок текущей позиции</summary>
    public CursorPosition CreateSnapshot() => new(_position, _line, _column);

    /// <summary>Восстанавливает позицию из снимка</summary>
    public void RestoreSnapshot(CursorPosition snapshot)
    {
        _position = snapshot.Position;
        _line = snapshot.Line;
        _column = snapshot.Column;
    }
}