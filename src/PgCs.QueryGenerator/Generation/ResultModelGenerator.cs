using PgCs.Common.Formatting;
using PgCs.Common.Generation.Models;
using PgCs.Common.Mapping;
using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryAnalyzer.Models.Results;
using PgCs.Common.QueryGenerator.Models;

namespace PgCs.QueryGenerator.Generation;

/// <summary>
/// Генератор моделей результатов для SQL запросов
/// </summary>
internal sealed class ResultModelGenerator : IResultModelGenerator
{
    /// <inheritdoc />
    public GeneratedModel Generate(QueryMetadata query, QueryGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(options);

        if (query.ReturnType == null)
        {
            throw new InvalidOperationException($"Запрос {query.MethodName} не имеет типа возврата");
        }

        var modelName = query.ExplicitModelName ?? $"{query.MethodName}Result";
        var properties = new List<ModelProperty>();

        foreach (var column in query.ReturnType.Columns)
        {
            var propertyName = ToPascalCase(column.Name);

            properties.Add(new ModelProperty
            {
                Name = propertyName,
                CSharpType = column.CSharpType,
                IsNullable = column.IsNullable,
                IsRequired = !column.IsNullable,
                Documentation = $"Колонка {column.Name} ({column.PostgresType})",
                SourceColumnName = column.Name,
                PostgresType = column.PostgresType
            });
        }

        var code = GenerateModelCode(modelName, properties, options);

        return new GeneratedModel
        {
            Name = modelName,
            SourceCode = code,
            Namespace = options.Namespace,
            ModelType = ModelType.QueryResult,
            Properties = properties,
            Documentation = $"Модель результата для запроса {query.MethodName}"
        };
    }

    /// <inheritdoc />
    public IReadOnlyList<GeneratedModel> GenerateBatch(
        IReadOnlyList<QueryMetadata> queries,
        QueryGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(queries);
        ArgumentNullException.ThrowIfNull(options);

        var models = new List<GeneratedModel>();
        var processedModels = new HashSet<string>();

        foreach (var query in queries)
        {
            if (query.ReturnType == null || query.ReturnCardinality == ReturnCardinality.Exec || query.ReturnCardinality == ReturnCardinality.ExecRows)
            {
                continue;
            }

            var modelName = query.ExplicitModelName ?? $"{query.MethodName}Result";

            // Избегаем дублирования моделей с одинаковым именем
            if (processedModels.Contains(modelName))
            {
                continue;
            }

            var model = Generate(query, options);
            models.Add(model);
            processedModels.Add(modelName);
        }

        return models;
    }

    /// <summary>
    /// Генерирует C# код модели
    /// </summary>
    private static string GenerateModelCode(string modelName, IReadOnlyList<ModelProperty> properties, QueryGenerationOptions options)
    {
        var code = new CodeBuilder(options.IndentationStyle, options.IndentationSize);

        if (options.GenerateXmlDocumentation)
        {
            code.AppendXmlSummary($"Модель результата запроса");
        }

        code.AppendLine($"public sealed record {modelName}");
        code.AppendLine("{");
        code.Indent();

        foreach (var prop in properties)
        {
            if (options.GenerateXmlDocumentation)
            {
                code.AppendXmlSummary(prop.Documentation ?? prop.Name);
            }

            var requiredKeyword = prop.IsRequired ? "required " : "";
            code.AppendLine($"public {requiredKeyword}{prop.CSharpType} {prop.Name} {{ get; init; }}");
        }

        code.Outdent();
        code.AppendLine("}");

        return code.ToString();
    }

    /// <summary>
    /// Преобразует snake_case в PascalCase
    /// </summary>
    private static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;

        var words = name.Split('_', StringSplitOptions.RemoveEmptyEntries);
        return string.Join("", words.Select(w => char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant()));
    }
}
