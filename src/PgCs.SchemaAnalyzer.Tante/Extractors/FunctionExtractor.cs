using System.Text.RegularExpressions;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;

namespace PgCs.SchemaAnalyzer.Tante.Extractors;

/// <summary>
/// Извлекает определения функций и процедур из SQL блоков
/// <para>
/// Поддерживает:
/// - Функции (CREATE FUNCTION)
/// - Процедуры (CREATE PROCEDURE)
/// - OR REPLACE
/// - Параметры с разными режимами (IN, OUT, INOUT, VARIADIC)
/// - Возвращаемые значения (RETURNS)
/// - Волатильность (VOLATILE, STABLE, IMMUTABLE)
/// </para>
/// </summary>
public sealed partial class FunctionExtractor : IFunctionExtractor
{
    // ============================================================================
    // Regex Patterns
    // ============================================================================

    /// <summary>
    /// Основной паттерн для извлечения CREATE FUNCTION/PROCEDURE
    /// Группы:
    /// - orReplace: OR REPLACE флаг
    /// - type: FUNCTION или PROCEDURE
    /// - schema: имя схемы (опционально)
    /// - name: имя функции
    /// </summary>
    [GeneratedRegex(
        @"^\s*CREATE\s+(?:(OR\s+REPLACE)\s+)?(FUNCTION|PROCEDURE)\s+(?:(\w+)\.)?(\w+)\s*\(",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex CreateFunctionPattern();

    /// <summary>
    /// Паттерн для извлечения RETURNS
    /// Группа: returnType - тип возвращаемого значения
    /// </summary>
    [GeneratedRegex(
        @"RETURNS\s+((?:SETOF\s+)?(?:TABLE\s*\([^)]+\)|\w+(?:\s+PRECISION)?(?:\s*\[\])?))",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex ReturnsPattern();

    /// <summary>
    /// Паттерн для извлечения волатильности
    /// </summary>
    [GeneratedRegex(
        @"\b(VOLATILE|STABLE|IMMUTABLE)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex VolatilityPattern();

    /// <summary>
    /// Паттерн для извлечения языка функции
    /// </summary>
    [GeneratedRegex(
        @"LANGUAGE\s+(\w+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex LanguagePattern();

    /// <summary>
    /// Паттерн для извлечения параметров функции
    /// </summary>
    [GeneratedRegex(
        @"\(\s*([^)]*)\s*\)",
        RegexOptions.Singleline | RegexOptions.Compiled,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex ParametersPattern();

    // ============================================================================
    // Public Methods
    // ============================================================================

    /// <inheritdoc />
    public bool CanExtract(SqlBlock block)
    {
        if (block == null)
        {
            return false;
        }

        return CreateFunctionPattern().IsMatch(block.Content);
    }

    /// <inheritdoc />
    public FunctionDefinition? Extract(SqlBlock block)
    {
        if (block == null)
        {
            return null;
        }

        if (!CanExtract(block))
        {
            return null;
        }

        var match = CreateFunctionPattern().Match(block.Content);
        if (!match.Success)
        {
            return null;
        }

        var isOrReplace = match.Groups[1].Success;
        var type = match.Groups[2].Value.ToUpperInvariant();
        var schema = match.Groups[3].Success ? match.Groups[3].Value : null;
        var name = match.Groups[4].Value;
        var isProcedure = type == "PROCEDURE";

        // Извлечение параметров
        var parameters = ExtractParameters(block.Content);

        // Извлечение возвращаемого типа (только для функций)
        var returnType = isProcedure ? null : ExtractReturnType(block.Content);

        // Извлечение языка
        var language = ExtractLanguage(block.Content);

        // Извлечение волатильности
        var volatility = ExtractVolatility(block.Content);

        // Извлечение тела функции
        var body = ExtractBody(block.Content);
        
        // Если Body null, возвращаем null для всей функции
        if (body is null)
        {
            return null;
        }

        return new FunctionDefinition
        {
            Name = name,
            Schema = schema,
            Parameters = parameters,
            ReturnType = returnType,
            Language = language ?? "sql",
            Volatility = volatility,
            IsProcedure = isProcedure,
            Body = body,
            SqlComment = block.HeaderComment,
            RawSql = block.Content
        };
    }

    // ============================================================================
    // Private Helper Methods
    // ============================================================================

    /// <summary>
    /// Извлекает параметры функции
    /// </summary>
    private static IReadOnlyList<FunctionParameter> ExtractParameters(string sql)
    {
        var parameters = new List<FunctionParameter>();

        // Находим первую открывающую скобку после имени функции
        var startIndex = sql.IndexOf('(');
        if (startIndex == -1)
        {
            return parameters;
        }

        // Находим соответствующую закрывающую скобку
        var depth = 0;
        var endIndex = -1;
        
        for (int i = startIndex; i < sql.Length; i++)
        {
            if (sql[i] == '(')
            {
                depth++;
            }
            else if (sql[i] == ')')
            {
                depth--;
                if (depth == 0)
                {
                    endIndex = i;
                    break;
                }
            }
        }

        if (endIndex == -1)
        {
            return parameters;
        }

        var paramsStr = sql.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();
        
        if (string.IsNullOrWhiteSpace(paramsStr))
        {
            return parameters;
        }

        // Разделяем параметры по запятым (учитывая вложенные скобки)
        var paramParts = SplitParameters(paramsStr);

        foreach (var part in paramParts)
        {
            var param = ParseParameter(part.Trim());
            if (param is not null)
            {
                parameters.Add(param);
            }
        }

        return parameters;
    }

    /// <summary>
    /// Разделяет строку параметров по запятым, учитывая вложенные скобки
    /// </summary>
    private static List<string> SplitParameters(string paramsStr)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        var depth = 0;

        foreach (var ch in paramsStr)
        {
            if (ch == '(')
            {
                depth++;
                current.Append(ch);
            }
            else if (ch == ')')
            {
                depth--;
                current.Append(ch);
            }
            else if (ch == ',' && depth == 0)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(ch);
            }
        }

        if (current.Length > 0)
        {
            result.Add(current.ToString());
        }

        return result;
    }

    /// <summary>
    /// Парсит отдельный параметр функции
    /// </summary>
    private static FunctionParameter? ParseParameter(string paramStr)
    {
        if (string.IsNullOrWhiteSpace(paramStr))
        {
            return null;
        }

        var parts = paramStr.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            return null;
        }

        var mode = ParameterMode.In;
        var startIndex = 0;

        // Проверяем режим параметра
        var firstPart = parts[0].ToUpperInvariant();
        if (firstPart is "IN" or "OUT" or "INOUT" or "VARIADIC")
        {
            mode = firstPart switch
            {
                "OUT" => ParameterMode.Out,
                "INOUT" => ParameterMode.InOut,
                "VARIADIC" => ParameterMode.Variadic,
                _ => ParameterMode.In
            };
            startIndex = 1;
        }

        if (parts.Length <= startIndex + 1)
        {
            return null;
        }

        var name = parts[startIndex];
        var dataType = parts[startIndex + 1];

        // Проверяем на массив
        var isArray = dataType.EndsWith("[]");
        
        // Для VARIADIC параметров оставляем [] в типе
        if (isArray && mode != ParameterMode.Variadic)
        {
            dataType = dataType[..^2];
        }

        // Извлекаем DEFAULT значение (если есть)
        string? defaultValue = null;
        var defaultIndex = Array.FindIndex(parts, p => p.Equals("DEFAULT", StringComparison.OrdinalIgnoreCase));
        if (defaultIndex > 0 && defaultIndex < parts.Length - 1)
        {
            defaultValue = string.Join(" ", parts[(defaultIndex + 1)..]);
        }

        return new FunctionParameter
        {
            Name = name,
            DataType = dataType,
            Mode = mode,
            IsArray = isArray,
            DefaultValue = defaultValue
        };
    }

    /// <summary>
    /// Извлекает тип возвращаемого значения функции
    /// </summary>
    private static string? ExtractReturnType(string sql)
    {
        var match = ReturnsPattern().Match(sql);
        if (!match.Success)
        {
            return null;
        }

        return match.Groups[1].Value.Trim();
    }

    /// <summary>
    /// Извлекает язык функции
    /// </summary>
    private static string? ExtractLanguage(string sql)
    {
        var match = LanguagePattern().Match(sql);
        return match.Success ? match.Groups[1].Value.ToLowerInvariant() : null;
    }

    /// <summary>
    /// Извлекает волатильность функции
    /// </summary>
    private static FunctionVolatility ExtractVolatility(string sql)
    {
        var match = VolatilityPattern().Match(sql);
        if (!match.Success)
        {
            return FunctionVolatility.Volatile; // По умолчанию
        }

        return match.Groups[1].Value.ToUpperInvariant() switch
        {
            "STABLE" => FunctionVolatility.Stable,
            "IMMUTABLE" => FunctionVolatility.Immutable,
            _ => FunctionVolatility.Volatile
        };
    }

    /// <summary>
    /// Извлекает тело функции
    /// </summary>
    private static string? ExtractBody(string sql)
    {
        // Ищем AS или BEGIN
        var asIndex = sql.IndexOf(" AS ", StringComparison.OrdinalIgnoreCase);
        if (asIndex == -1)
        {
            asIndex = sql.IndexOf("\nAS ", StringComparison.OrdinalIgnoreCase);
        }

        if (asIndex == -1)
        {
            return null;
        }

        var bodyStart = asIndex + 4;
        
        // Ищем конец функции (обычно это последняя точка с запятой или конец блока)
        var bodyEnd = sql.Length;
        
        // Убираем завершающую точку с запятой
        var lastSemicolon = sql.LastIndexOf(';');
        if (lastSemicolon > bodyStart)
        {
            bodyEnd = lastSemicolon;
        }

        var body = sql.Substring(bodyStart, bodyEnd - bodyStart).Trim();

        // Убираем внешние кавычки для строковых литералов
        if (body.StartsWith("$$") && body.EndsWith("$$"))
        {
            body = body[2..^2].Trim();
        }
        else if (body.StartsWith("$") && body.Contains("$") && body.EndsWith("$"))
        {
            // Обработка $tag$...$tag$
            var firstDollar = body.IndexOf('$', 1);
            if (firstDollar > 0)
            {
                var tag = body[..(firstDollar + 1)];
                if (body.EndsWith(tag))
                {
                    body = body[tag.Length..^tag.Length].Trim();
                }
            }
        }
        else if (body.StartsWith("'") && body.EndsWith("'"))
        {
            body = body[1..^1];
        }

        return body;
    }
}
