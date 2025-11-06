namespace PgCs.Core.Tokenization;

/// <summary>
/// Сканер числовых литералов PostgreSQL
/// Поддерживает целые, десятичные, экспоненциальные числа
/// </summary>
public sealed class SqlNumberScanner
{
    /// <summary>
    /// Сканирует числовой литерал
    /// Форматы: 123, 123.456, 1.23e10, 0x1A (hex), 0b101 (binary), 0o17 (octal)
    /// </summary>
    public static ScanResult ScanNumber(TextCursor cursor)
    {
        var start = cursor.Position;
        
        // Проверяем специальные префиксы (PostgreSQL расширения)
        if (cursor.Current == '0' && !cursor.IsAtEnd())
        {
            var next = cursor.Peek();
            
            // Hexadecimal: 0x1A
            if (next is 'x' or 'X')
            {
                cursor.Advance(); // 0
                cursor.Advance(); // x
                
                while (!cursor.IsAtEnd() && IsHexDigit(cursor.Current))
                {
                    cursor.Advance();
                }
                
                return new ScanResult(TokenType.NumericLiteral, cursor.Position - start);
            }
            
            // Binary: 0b101
            if (next is 'b' or 'B')
            {
                cursor.Advance(); // 0
                cursor.Advance(); // b
                
                while (!cursor.IsAtEnd() && IsBinaryDigit(cursor.Current))
                {
                    cursor.Advance();
                }
                
                return new ScanResult(TokenType.NumericLiteral, cursor.Position - start);
            }
            
            // Octal: 0o17
            if (next is 'o' or 'O')
            {
                cursor.Advance(); // 0
                cursor.Advance(); // o
                
                while (!cursor.IsAtEnd() && IsOctalDigit(cursor.Current))
                {
                    cursor.Advance();
                }
                
                return new ScanResult(TokenType.NumericLiteral, cursor.Position - start);
            }
        }
        
        // Целая часть
        while (!cursor.IsAtEnd() && SqlCharClassifier.IsDigit(cursor.Current))
        {
            cursor.Advance();
        }
        
        // Десятичная часть
        if (cursor.Current == '.' && !cursor.IsAtEnd() && SqlCharClassifier.IsDigit(cursor.Peek()))
        {
            cursor.Advance(); // .
            
            while (!cursor.IsAtEnd() && SqlCharClassifier.IsDigit(cursor.Current))
            {
                cursor.Advance();
            }
        }
        
        // Экспонента
        if (cursor.Current is 'e' or 'E')
        {
            cursor.Advance(); // e
            
            // Опциональный знак
            if (cursor.Current is '+' or '-')
            {
                cursor.Advance();
            }
            
            while (!cursor.IsAtEnd() && SqlCharClassifier.IsDigit(cursor.Current))
            {
                cursor.Advance();
            }
        }
        
        var length = cursor.Position - start;
        return new ScanResult(TokenType.NumericLiteral, length);
    }

    private static bool IsHexDigit(char ch) => 
        SqlCharClassifier.IsDigit(ch) || ch is >= 'A' and <= 'F' or >= 'a' and <= 'f';

    private static bool IsBinaryDigit(char ch) => 
        ch is '0' or '1';

    private static bool IsOctalDigit(char ch) => 
        ch is >= '0' and <= '7';
}