using System.Text.RegularExpressions;
using PgCs.Common.SchemaAnalyzer.Models.Functions;
using PgCs.Common.SchemaAnalyzer.Models.Tables;
using PgCs.Common.Utils;

namespace PgCs.SchemaAnalyzer.Extractors;

/// <summary>
/// Извлекает определения функций и процедур
/// </summary>
internal sealed partial class FunctionExtractor : BaseExtractor<FunctionDefinition>
{
    [GeneratedRegex(@"CREATE\s+(?:OR\s+REPLACE\s+)?FUNCTION\s+([a-zA-Z_][a-zA-Z0-9_.]*)\s*\((.*?)\)\s+RETURNS\s+(.*?)\s+AS\s+(\$\$|\$[a-zA-Z_][a-zA-Z0-9_]*\$)(.*?)(\$\$|\$[a-zA-Z_][a-zA-Z0-9_]*\$)\s+LANGUAGE\s+([a-zA-Z]+)(?:\s+(IMMUTABLE|STABLE|VOLATILE))?", 
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex FunctionPatternRegex();

    protected override Regex Pattern => FunctionPatternRegex();

    protected override FunctionDefinition? ParseMatch(Match match, string statement)
    {
        var fullFunctionName = match.Groups[1].Value.Trim();
        var parametersText = match.Groups[2].Value.Trim();
        var returnType = match.Groups[3].Value.Trim();
        var body = match.Groups[5].Value.Trim();
        var language = match.Groups[7].Value.Trim();
        var volatility = match.Groups[8].Value.Trim();

        var schema = ExtractSchemaName(fullFunctionName);
        var functionName = ExtractTableName(fullFunctionName);

        var parameters = ParseParameters(parametersText);
        var (returnsTable, returnTableColumns) = ParseReturnType(returnType);

        var functionVolatility = ParseVolatility(volatility);
        var isTrigger = returnType.Equals("TRIGGER", StringComparison.OrdinalIgnoreCase);

        return new FunctionDefinition
        {
            Name = functionName,
            Schema = schema,
            Parameters = parameters,
            ReturnType = returnType,
            ReturnsTable = returnsTable,
            ReturnTableColumns = returnTableColumns,
            Language = language,
            Body = body,
            Volatility = functionVolatility,
            IsTrigger = isTrigger,
            RawSql = statement
        };
    }

    private IReadOnlyList<FunctionParameter> ParseParameters(string parametersText)
    {
        if (string.IsNullOrWhiteSpace(parametersText))
            return [];

        var parameters = new List<FunctionParameter>();
        var parts = SplitParameters(parametersText);

        foreach (var part in parts)
        {
            var parameter = ParseParameter(part);
            if (parameter is not null)
            {
                parameters.Add(parameter);
            }
        }

        return parameters;
    }

    private FunctionParameter? ParseParameter(string parameterText)
    {
        var tokens = parameterText.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length < 2)
            return null;

        var mode = ParameterMode.In;
        var startIndex = 0;

        // Проверяем режим параметра
        if (tokens[0].Equals("IN", StringComparison.OrdinalIgnoreCase))
        {
            mode = ParameterMode.In;
            startIndex = 1;
        }
        else if (tokens[0].Equals("OUT", StringComparison.OrdinalIgnoreCase))
        {
            mode = ParameterMode.Out;
            startIndex = 1;
        }
        else if (tokens[0].Equals("INOUT", StringComparison.OrdinalIgnoreCase))
        {
            mode = ParameterMode.InOut;
            startIndex = 1;
        }
        else if (tokens[0].Equals("VARIADIC", StringComparison.OrdinalIgnoreCase))
        {
            mode = ParameterMode.Variadic;
            startIndex = 1;
        }

        if (startIndex >= tokens.Length - 1)
            return null;

        var paramName = tokens[startIndex];
        var dataType = string.Join(" ", tokens.Skip(startIndex + 1));

        // Извлекаем DEFAULT значение
        string? defaultValue = null;
        var defaultMatch = Regex.Match(dataType, @"DEFAULT\s+(.+)", RegexOptions.IgnoreCase);
        if (defaultMatch.Success)
        {
            defaultValue = defaultMatch.Groups[1].Value.Trim();
            dataType = dataType[..defaultMatch.Index].Trim();
        }

        return new FunctionParameter
        {
            Name = paramName,
            DataType = dataType,
            Mode = mode,
            DefaultValue = defaultValue
        };
    }

    private (bool ReturnsTable, IReadOnlyList<ColumnDefinition>? Columns) ParseReturnType(string returnType)
    {
        if (returnType.StartsWith("TABLE", StringComparison.OrdinalIgnoreCase))
        {
            var match = Regex.Match(returnType, @"TABLE\s*\((.*?)\)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var columnsText = match.Groups[1].Value;
                var columns = ParseTableColumns(columnsText);
                return (true, columns);
            }
        }

        return (false, null);
    }

    private IReadOnlyList<ColumnDefinition> ParseTableColumns(string columnsText)
    {
        var columns = new List<ColumnDefinition>();
        var parts = SplitParameters(columnsText);

        foreach (var part in parts)
        {
            var tokens = part.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length >= 2)
            {
                columns.Add(new ColumnDefinition
                {
                    Name = tokens[0],
                    DataType = string.Join(" ", tokens.Skip(1)),
                    IsNullable = true
                });
            }
        }

        return columns;
    }

    private static FunctionVolatility ParseVolatility(string volatility)
    {
        if (string.IsNullOrWhiteSpace(volatility))
            return FunctionVolatility.Volatile;

        return volatility.ToUpperInvariant() switch
        {
            "IMMUTABLE" => FunctionVolatility.Immutable,
            "STABLE" => FunctionVolatility.Stable,
            "VOLATILE" => FunctionVolatility.Volatile,
            _ => FunctionVolatility.Volatile
        };
    }

    private static List<string> SplitParameters(string parametersText)
    {
        // Используем общий метод без учета кавычек (параметры не содержат строковых литералов)
        var parts = StringParsingHelpers.SplitByCommaRespectingDepth(parametersText, respectQuotes: false);
        return parts.ToList();
    }
}