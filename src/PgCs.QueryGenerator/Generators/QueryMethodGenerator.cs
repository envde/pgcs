using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using PgCs.Common.CodeGeneration;
using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryAnalyzer.Models.Results;
using PgCs.Common.QueryGenerator.Models.Options;
using PgCs.Common.QueryGenerator.Models.Results;
using PgCs.QueryGenerator.Services;

namespace PgCs.QueryGenerator.Generators;

/// <summary>
/// Генератор C# методов для SQL запросов с использованием Roslyn
/// </summary>
public sealed class QueryMethodGenerator : IQueryMethodGenerator
{
    private readonly QuerySyntaxBuilder _syntaxBuilder;
    private readonly INpgsqlCommandBuilder _commandBuilder;

    public QueryMethodGenerator(
        QuerySyntaxBuilder syntaxBuilder,
        INpgsqlCommandBuilder commandBuilder)
    {
        _syntaxBuilder = syntaxBuilder;
        _commandBuilder = commandBuilder;
    }

    public ValueTask<GeneratedMethodResult> GenerateAsync(
        QueryMetadata queryMetadata,
        QueryGenerationOptions options)
    {
        // Генерируем код метода через string builder (упрощённая версия для компиляции)
        // TODO: Полная реализация через Roslyn SyntaxFactory
        var sourceCode = GenerateMethodCode(queryMetadata, options);

        return ValueTask.FromResult(new GeneratedMethodResult
        {
            IsSuccess = true,
            MethodName = queryMetadata.MethodName,
            MethodSignature = GetMethodSignature(queryMetadata, options),
            SourceCode = sourceCode,
            SqlQuery = queryMetadata.SqlQuery
        });
    }

    /// <summary>
    /// Генерирует код метода (упрощённая версия)
    /// </summary>
    private string GenerateMethodCode(QueryMetadata queryMetadata, QueryGenerationOptions options)
    {
        var sb = new StringBuilder();

        // XML комментарий
        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// Выполняет запрос: {queryMetadata.MethodName}");
        sb.AppendLine($"    /// </summary>");
        if (options.IncludeSqlInDocumentation)
        {
            sb.AppendLine($"    /// <remarks>SQL: {queryMetadata.SqlQuery}</remarks>");
        }

        // Сигнатура метода
        sb.AppendLine($"    {GetMethodSignature(queryMetadata, options)}");
        sb.AppendLine("    {");

        // Получение соединения
        if (options.GenerateTransactionSupport)
        {
            sb.AppendLine("        await using var connection = transaction?.Connection ?? await _dataSource.OpenConnectionAsync(cancellationToken);");
        }
        else
        {
            sb.AppendLine("        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);");
        }

        // Создание команды
        sb.AppendLine($"        await using var cmd = new NpgsqlCommand(@\"{queryMetadata.SqlQuery}\", connection");
        if (options.GenerateTransactionSupport)
        {
            sb.AppendLine(", transaction);");
        }
        else
        {
            sb.AppendLine(");");
        }

        // Добавление параметров
        foreach (var param in queryMetadata.Parameters)
        {
            var paramName = ConvertParameterName(param.Name);
            sb.AppendLine($"        cmd.Parameters.AddWithValue(\"{param.Name}\", {paramName});");
        }

        // Выполнение запроса
        switch (queryMetadata.QueryType)
        {
            case QueryType.Select when queryMetadata.ReturnType != null:
                GenerateSelectCode(sb, queryMetadata, options);
                break;

            case QueryType.Insert:
            case QueryType.Update:
            case QueryType.Delete:
                sb.AppendLine("        return await cmd.ExecuteNonQueryAsync(cancellationToken);");
                break;

            default:
                sb.AppendLine("        await cmd.ExecuteNonQueryAsync(cancellationToken);");
                break;
        }

        sb.AppendLine("    }");

        return sb.ToString();
    }

    /// <summary>
    /// Генерирует код для SELECT запроса
    /// </summary>
    private void GenerateSelectCode(StringBuilder sb, QueryMetadata queryMetadata, QueryGenerationOptions options)
    {
        sb.AppendLine("        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);");

        if (queryMetadata.ReturnCardinality == ReturnCardinality.One)
        {
            // Один результат
            sb.AppendLine("        if (!await reader.ReadAsync(cancellationToken)) return null;");
            sb.AppendLine($"        return MapTo{queryMetadata.ReturnType!.ModelName}(reader);");
        }
        else
        {
            // Множество результатов
            sb.AppendLine($"        var result = new List<{queryMetadata.ReturnType!.ModelName}>();");
            sb.AppendLine("        while (await reader.ReadAsync(cancellationToken))");
            sb.AppendLine("        {");
            sb.AppendLine($"            result.Add(MapTo{queryMetadata.ReturnType.ModelName}(reader));");
            sb.AppendLine("        }");
            sb.AppendLine("        return result;");
        }
    }

    /// <summary>
    /// Получает сигнатуру метода
    /// </summary>
    private string GetMethodSignature(QueryMetadata queryMetadata, QueryGenerationOptions options)
    {
        var returnType = GetReturnTypeString(queryMetadata);
        var parameters = GetParametersString(queryMetadata, options);

        return $"public async {returnType} {queryMetadata.MethodName}Async({parameters})";
    }

    /// <summary>
    /// Получает строку типа возврата
    /// </summary>
    private string GetReturnTypeString(QueryMetadata queryMetadata)
    {
        return queryMetadata.QueryType switch
        {
            QueryType.Select when queryMetadata.ReturnType != null =>
                queryMetadata.ReturnCardinality == ReturnCardinality.One
                    ? $"Task<{queryMetadata.ReturnType.ModelName}?>"
                    : $"Task<List<{queryMetadata.ReturnType.ModelName}>>",
            QueryType.Insert or QueryType.Update or QueryType.Delete => "Task<int>",
            _ => "Task"
        };
    }

    /// <summary>
    /// Получает строку параметров метода
    /// </summary>
    private string GetParametersString(QueryMetadata queryMetadata, QueryGenerationOptions options)
    {
        var parameters = new List<string>();

        // Параметры запроса
        if (options.GenerateParameterModels && 
            queryMetadata.Parameters.Count >= options.ParameterModelThreshold)
        {
            var paramModelName = $"{queryMetadata.MethodName}Params";
            parameters.Add($"{paramModelName} params");
        }
        else
        {
            foreach (var param in queryMetadata.Parameters)
            {
                var paramName = ConvertParameterName(param.Name);
                parameters.Add($"{param.CSharpType} {paramName}");
            }
        }

        // Транзакция
        if (options.GenerateTransactionSupport)
        {
            parameters.Add("NpgsqlTransaction? transaction = null");
        }

        // CancellationToken
        if (options.SupportCancellation)
        {
            parameters.Add("CancellationToken cancellationToken = default");
        }

        return string.Join(", ", parameters);
    }

    /// <summary>
    /// Конвертирует имя параметра из SQL в C# стиль
    /// </summary>
    private string ConvertParameterName(string sqlName)
    {
        // Убираем префиксы @ и $
        var name = sqlName.TrimStart('@', '$');
        
        // Конвертируем snake_case в camelCase
        var parts = name.Split('_');
        if (parts.Length == 1)
            return char.ToLower(name[0]) + name.Substring(1);

        var result = parts[0].ToLower();
        for (int i = 1; i < parts.Length; i++)
        {
            if (parts[i].Length > 0)
            {
                result += char.ToUpper(parts[i][0]) + parts[i].Substring(1).ToLower();
            }
        }

        return result;
    }
}
