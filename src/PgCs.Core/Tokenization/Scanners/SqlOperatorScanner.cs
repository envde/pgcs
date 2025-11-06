namespace PgCs.Core.Tokenization.Scanners;

/// <summary>
/// Сканер SQL операторов PostgreSQL
/// Обрабатывает простые и составные операторы
/// </summary>
public sealed class SqlOperatorScanner
{
    /// <summary>
    /// Сканирует оператор (простой или составной)
    /// PostgreSQL поддерживает многосимвольные операторы: <=, >=, <>, !=, ||, &&, etc.
    /// </summary>
    public static ScanResult ScanOperator(TextCursor cursor)
    {
        var start = cursor.Position;
        var first = cursor.Current;

        cursor.Advance();

        if (cursor.IsAtEnd())
        {
            return new ScanResult(TokenType.Operator, 1);
        }

        var second = cursor.Current;

        // Двухсимвольные операторы
        var twoCharOp = $"{first}{second}";
        if (IsTwoCharOperator(twoCharOp))
        {
            cursor.Advance();
            return new ScanResult(TokenType.Operator, 2);
        }

        // Проверяем трёхсимвольные операторы
        if (!cursor.IsAtEnd())
        {
            var third = cursor.Peek();
            var threeCharOp = $"{first}{second}{third}";

            if (IsThreeCharOperator(threeCharOp))
            {
                cursor.Advance();
                cursor.Advance();
                return new ScanResult(TokenType.Operator, 3);
            }
        }

        return new ScanResult(TokenType.Operator, 1);
    }

    private static bool IsTwoCharOperator(string op) => op switch
    {
        "<=" or ">=" or "<>" or "!=" or "||" or "&&" or
            "::" or "->" or "->>" or "#>" or "#>>" or
            "@>" or "<@" or "?|" or "?&" or "~*" or "!~" or
            "!~*" or "@@" or "##" or "<->" or "<<" or ">>" or
            "&<" or "&>" or "<<|" or "|>>" or "&<|" or "|&>" => true,
        _ => false
    };

    private static bool IsThreeCharOperator(string op) => op switch
    {
        "!~~" or "~~*" or "!~~*" => true,
        _ => false
    };
}