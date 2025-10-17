using PgCs.Common.Formatting;
using PgCs.Common.Generation.Models;
using PgCs.Common.Mapping;
using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryAnalyzer.Models.Results;
using PgCs.Common.QueryGenerator.Models;

namespace PgCs.QueryGenerator.Generation;

/// <summary>
/// Генератор C# методов для выполнения SQL запросов
/// </summary>
internal sealed class MethodGenerator : IMethodGenerator
{
    /// <inheritdoc />
    public GeneratedMethod Generate(QueryMetadata query, QueryGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(options);

        var code = new CodeBuilder(options.IndentationStyle, options.IndentationSize);
        var methodName = query.MethodName + (options.GenerateAsyncMethods ? options.MethodSuffix : "");

        // Определяем возвращаемый тип
        var returnType = DetermineReturnType(query, options);

        // Определяем параметры метода
        var parameters = BuildMethodParameters(query, options);

        // Генерируем метод
        GenerateMethodSignature(code, methodName, returnType, parameters, query, options);
        code.AppendLine("{");
        code.Indent();

        GenerateMethodBody(code, query, options);

        code.Outdent();
        code.AppendLine("}");

        return new GeneratedMethod
        {
            Name = methodName,
            SourceCode = code.ToString(),
            ReturnType = returnType,
            Parameters = parameters.Select(p => new MethodParameter
            {
                Name = p.name,
                CSharpType = p.type,
                IsNullable = p.type.EndsWith("?"),
                Documentation = p.doc
            }).ToList(),
            IsAsync = options.GenerateAsyncMethods,
            SqlQuery = query.SqlQuery,
            Documentation = $"Выполняет SQL запрос: {query.MethodName}"
        };
    }

    /// <summary>
    /// Определяет возвращаемый тип метода
    /// </summary>
    private static string DetermineReturnType(QueryMetadata query, QueryGenerationOptions options)
    {
        if (!options.GenerateAsyncMethods)
        {
            return GetSyncReturnType(query);
        }

        var asyncType = options.UseValueTask ? "ValueTask" : "Task";

        return query.ReturnCardinality switch
        {
            ReturnCardinality.One => query.ReturnType != null
                ? $"{asyncType}<{GetModelName(query)}?>"
                : $"{asyncType}<object?>",

            ReturnCardinality.Many => query.ReturnType != null
                ? $"{asyncType}<IReadOnlyList<{GetModelName(query)}>>"
                : $"{asyncType}<IReadOnlyList<object>>",

            ReturnCardinality.Exec => asyncType,

            ReturnCardinality.ExecRows => $"{asyncType}<int>",

            _ => asyncType
        };
    }

    /// <summary>
    /// Получает синхронный возвращаемый тип
    /// </summary>
    private static string GetSyncReturnType(QueryMetadata query)
    {
        return query.ReturnCardinality switch
        {
            ReturnCardinality.One => query.ReturnType != null ? $"{GetModelName(query)}?" : "object?",
            ReturnCardinality.Many => query.ReturnType != null 
                ? $"IReadOnlyList<{GetModelName(query)}>" 
                : "IReadOnlyList<object>",
            ReturnCardinality.Exec => "void",
            ReturnCardinality.ExecRows => "int",
            _ => "void"
        };
    }

    /// <summary>
    /// Получает имя модели для запроса
    /// </summary>
    private static string GetModelName(QueryMetadata query)
    {
        return query.ExplicitModelName ?? $"{query.MethodName}Result";
    }

    /// <summary>
    /// Строит список параметров метода
    /// </summary>
    private static List<(string name, string type, string doc)> BuildMethodParameters(
        QueryMetadata query,
        QueryGenerationOptions options)
    {
        var parameters = new List<(string, string, string)>
        {
            ("connection", "NpgsqlConnection", "Подключение к базе данных"),
        };

        // Добавляем параметры запроса
        foreach (var param in query.Parameters)
        {
            parameters.Add((param.Name, param.CSharpType, $"Параметр {param.Name}"));
        }

        // Добавляем опциональную транзакцию
        parameters.Add(("transaction", "NpgsqlTransaction?", "Опциональная транзакция"));

        // Добавляем CancellationToken если нужно
        if (options.SupportCancellation && options.GenerateAsyncMethods)
        {
            parameters.Add(("cancellationToken", "CancellationToken", "Токен отмены"));
        }

        return parameters;
    }

    /// <summary>
    /// Генерирует сигнатуру метода
    /// </summary>
    private static void GenerateMethodSignature(
        CodeBuilder code,
        string methodName,
        string returnType,
        List<(string name, string type, string doc)> parameters,
        QueryMetadata query,
        QueryGenerationOptions options)
    {
        // XML документация
        if (options.GenerateXmlDocumentation)
        {
            code.AppendXmlSummary($"Выполняет SQL запрос: {query.MethodName}");

            foreach (var (name, _, doc) in parameters)
            {
                code.AppendXmlParam(name, doc);
            }

            code.AppendXmlReturns(GetReturnDescription(query));
        }

        // Сигнатура
        var asyncKeyword = options.GenerateAsyncMethods ? "async " : "";
        code.AppendLine($"public {asyncKeyword}{returnType} {methodName}(");
        code.Indent();

        for (int i = 0; i < parameters.Count; i++)
        {
            var (name, type, _) = parameters[i];
            var defaultValue = GetDefaultValue(name, type);
            var comma = i < parameters.Count - 1 ? "," : "";

            code.AppendLine($"{type} {name}{defaultValue}{comma}");
        }

        code.Outdent();
        code.AppendLine(")");
    }

    /// <summary>
    /// Генерирует тело метода
    /// </summary>
    private static void GenerateMethodBody(CodeBuilder code, QueryMetadata query, QueryGenerationOptions options)
    {
        // SQL запрос как константа
        code.AppendLine("const string sql = @\"");
        code.Indent();
        code.AppendLine(query.SqlQuery.Trim());
        code.Outdent();
        code.AppendLine("\";");
        code.AppendLine();

        // Создание команды
        code.AppendLine($"await using var cmd = new NpgsqlCommand(sql, connection, transaction);");
        code.AppendLine();

        // Добавление параметров
        if (query.Parameters.Count > 0)
        {
            code.AppendLine("// Параметры запроса");
            foreach (var param in query.Parameters)
            {
                var dbType = TypeMapper.MapToNpgsqlDbType(param.PostgresType);
                if (dbType.HasValue)
                {
                    code.AppendLine($"cmd.Parameters.Add(new NpgsqlParameter<{param.CSharpType}>(\"{param.Name}\", {param.Name}));");
                }
                else
                {
                    code.AppendLine($"cmd.Parameters.AddWithValue(\"{param.Name}\", {param.Name});");
                }
            }
            code.AppendLine();
        }

        // Выполнение в зависимости от типа
        GenerateExecutionCode(code, query, options);
    }

    /// <summary>
    /// Генерирует код выполнения запроса
    /// </summary>
    private static void GenerateExecutionCode(CodeBuilder code, QueryMetadata query, QueryGenerationOptions options)
    {
        var cancellationToken = options.SupportCancellation ? "cancellationToken" : "default";

        switch (query.ReturnCardinality)
        {
            case ReturnCardinality.One:
                GenerateOneResultExecution(code, query, cancellationToken);
                break;

            case ReturnCardinality.Many:
                GenerateManyResultsExecution(code, query, cancellationToken);
                break;

            case ReturnCardinality.Exec:
                code.AppendLine($"await cmd.ExecuteNonQueryAsync({cancellationToken});");
                break;

            case ReturnCardinality.ExecRows:
                code.AppendLine($"return await cmd.ExecuteNonQueryAsync({cancellationToken});");
                break;
        }
    }

    /// <summary>
    /// Генерирует код для возврата одного результата
    /// </summary>
    private static void GenerateOneResultExecution(CodeBuilder code, QueryMetadata query, string cancellationToken)
    {
        code.AppendLine($"await using var reader = await cmd.ExecuteReaderAsync({cancellationToken});");
        code.AppendLine();
        code.AppendLine("if (!await reader.ReadAsync())");
        code.AppendLine("{");
        code.Indent();
        code.AppendLine("return null;");
        code.Outdent();
        code.AppendLine("}");
        code.AppendLine();

        if (query.ReturnType != null)
        {
            var modelName = GetModelName(query);
            code.AppendLine($"return new {modelName}");
            code.AppendLine("{");
            code.Indent();

            for (int i = 0; i < query.ReturnType.Columns.Count; i++)
            {
                var col = query.ReturnType.Columns[i];
                var propName = ToPascalCase(col.Name);
                var readerMethod = TypeMapper.GetReaderMethod(col.CSharpType);
                var comma = i < query.ReturnType.Columns.Count - 1 ? "," : "";

                if (col.IsNullable)
                {
                    code.AppendLine($"{propName} = reader.IsDBNull({i}) ? null : reader.{readerMethod}({i}){comma}");
                }
                else
                {
                    code.AppendLine($"{propName} = reader.{readerMethod}({i}){comma}");
                }
            }

            code.Outdent();
            code.AppendLine("};");
        }
    }

    /// <summary>
    /// Генерирует код для возврата множества результатов
    /// </summary>
    private static void GenerateManyResultsExecution(CodeBuilder code, QueryMetadata query, string cancellationToken)
    {
        var modelName = GetModelName(query);
        code.AppendLine($"await using var reader = await cmd.ExecuteReaderAsync({cancellationToken});");
        code.AppendLine($"var results = new List<{modelName}>();");
        code.AppendLine();
        code.AppendLine("while (await reader.ReadAsync())");
        code.AppendLine("{");
        code.Indent();

        if (query.ReturnType != null)
        {
            code.AppendLine($"results.Add(new {modelName}");
            code.AppendLine("{");
            code.Indent();

            for (int i = 0; i < query.ReturnType.Columns.Count; i++)
            {
                var col = query.ReturnType.Columns[i];
                var propName = ToPascalCase(col.Name);
                var readerMethod = TypeMapper.GetReaderMethod(col.CSharpType);
                var comma = i < query.ReturnType.Columns.Count - 1 ? "," : "";

                if (col.IsNullable)
                {
                    code.AppendLine($"{propName} = reader.IsDBNull({i}) ? null : reader.{readerMethod}({i}){comma}");
                }
                else
                {
                    code.AppendLine($"{propName} = reader.{readerMethod}({i}){comma}");
                }
            }

            code.Outdent();
            code.AppendLine("});");
        }

        code.Outdent();
        code.AppendLine("}");
        code.AppendLine();
        code.AppendLine("return results;");
    }

    private static string GetDefaultValue(string paramName, string paramType)
    {
        return paramName switch
        {
            "transaction" => " = null",
            "cancellationToken" => " = default",
            _ => ""
        };
    }

    private static string GetReturnDescription(QueryMetadata query)
    {
        return query.ReturnCardinality switch
        {
            ReturnCardinality.One => "Один результат или null",
            ReturnCardinality.Many => "Список результатов",
            ReturnCardinality.Exec => "Задача выполнения",
            ReturnCardinality.ExecRows => "Количество затронутых строк",
            _ => "Результат выполнения"
        };
    }

    private static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;

        var words = name.Split('_', StringSplitOptions.RemoveEmptyEntries);
        return string.Join("", words.Select(w => char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant()));
    }
}
