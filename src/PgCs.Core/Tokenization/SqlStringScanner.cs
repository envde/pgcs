namespace PgCs.Core.Tokenization;

/// <summary>
/// Сканер строковых литералов PostgreSQL
/// Обрабатывает различные форматы строк
/// </summary>
public sealed class SqlStringScanner
{
    /// <summary>
    /// Сканирует строковый литерал с одинарными кавычками: 'text'
    /// Поддерживает экранирование через двойные кавычки: 'don''t'
    /// </summary>
    public static ScanResult ScanStringLiteral(TextCursor cursor)
    {
        var start = cursor.Position;
        
        // Пропускаем открывающую кавычку
        cursor.Advance();
        
        while (!cursor.IsAtEnd())
        {
            if (cursor.Current == '\'')
            {
                cursor.Advance();
                
                // Проверяем двойную кавычку '' (экранирование)
                if (cursor.Current == '\'')
                {
                    cursor.Advance(); // Пропускаем вторую кавычку
                    continue;
                }
                
                // Конец строки
                break;
            }
            
            cursor.Advance();
        }
        
        var length = cursor.Position - start;
        return new ScanResult(TokenType.StringLiteral, length);
    }

    /// <summary>
    /// Сканирует dollar-quoted строку: $$text$$ или $tag$text$tag$
    /// PostgreSQL поддерживает произвольные теги между $
    /// </summary>
    public static ScanResult ScanDollarQuotedString(TextCursor cursor)
    {
        var start = cursor.Position;
        
        // Читаем открывающий тег: $ + [optional_tag] + $
        cursor.Advance(); // Первый $
        
        var tagStart = cursor.Position;
        
        // Читаем тег (может быть пустым)
        while (!cursor.IsAtEnd() && SqlCharClassifier.IsIdentifierPart(cursor.Current))
        {
            cursor.Advance();
        }
        
        var tagEnd = cursor.Position;
        
        // Должен быть закрывающий $ тега
        if (cursor.Current != '$')
        {
            // Это не dollar-quoted string, а просто оператор $
            return new ScanResult(TokenType.Operator, 1);
        }
        
        cursor.Advance(); // Закрывающий $ тега
        
        var tag = cursor.GetText(tagStart, tagEnd - tagStart);
        var closingTag = $"${tag}$";
        
        // Ищем закрывающий тег
        while (!cursor.IsAtEnd())
        {
            if (cursor.MatchSequence(closingTag))
            {
                // Пропускаем закрывающий тег
                for (var i = 0; i < closingTag.Length; i++)
                {
                    cursor.Advance();
                }
                break;
            }
            
            cursor.Advance();
        }
        
        var length = cursor.Position - start;
        return new ScanResult(TokenType.DollarQuotedString, length);
    }

    /// <summary>
    /// Сканирует quoted идентификатор: "Table Name"
    /// PostgreSQL не поддерживает экранирование внутри quoted identifiers
    /// </summary>
    public static ScanResult ScanQuotedIdentifier(TextCursor cursor)
    {
        var start = cursor.Position;
        
        // Пропускаем открывающую кавычку
        cursor.Advance();
        
        while (!cursor.IsAtEnd() && cursor.Current != '"')
        {
            cursor.Advance();
        }
        
        if (!cursor.IsAtEnd())
        {
            cursor.Advance(); // Закрывающая кавычка
        }
        
        var length = cursor.Position - start;
        return new ScanResult(TokenType.QuotedIdentifier, length);
    }
}