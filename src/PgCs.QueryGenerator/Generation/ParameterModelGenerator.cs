using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryGenerator.Models;
using PgCs.QueryGenerator.Formatting;

namespace PgCs.QueryGenerator.Generation;

/// <summary>
/// Генератор моделей параметров для SQL запросов
/// </summary>
internal sealed class ParameterModelGenerator : IParameterModelGenerator
{
    private const int MinParametersForModel = 3;

    /// <inheritdoc />
    public GeneratedModel? Generate(QueryMetadata query, QueryGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(options);

        // Генерируем модель параметров только если их достаточно много
        if (!options.GenerateParameterModels || query.Parameters.Count < MinParametersForModel)
        {
            return null;
        }

        var modelName = $"{query.MethodName}Parameters";
        var properties = new List<ModelProperty>();

        foreach (var param in query.Parameters)
        {
            var propertyName = ToPascalCase(param.Name);

            properties.Add(new ModelProperty
            {
                Name = propertyName,
                CSharpType = param.CSharpType,
                IsNullable = param.IsNullable,
                IsRequired = !param.IsNullable,
                Documentation = $"Параметр {param.Name} ({param.PostgresType})",
                SourceColumnName = param.Name,
                PostgresType = param.PostgresType
            });
        }

        var code = GenerateModelCode(modelName, properties, options);

        return new GeneratedModel
        {
            Name = modelName,
            SourceCode = code,
            Namespace = options.Namespace,
            ModelType = ModelType.QueryParameters,
            Properties = properties,
            Documentation = $"Модель параметров для запроса {query.MethodName}"
        };
    }

    /// <inheritdoc />
    public IReadOnlyList<GeneratedModel> GenerateBatch(
        IReadOnlyList<QueryMetadata> queries,
        QueryGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(queries);
        ArgumentNullException.ThrowIfNull(options);

        if (!options.GenerateParameterModels)
        {
            return Array.Empty<GeneratedModel>();
        }

        var models = new List<GeneratedModel>();
        var processedModels = new HashSet<string>();

        foreach (var query in queries)
        {
            if (query.Parameters.Count < MinParametersForModel)
            {
                continue;
            }

            var modelName = $"{query.MethodName}Parameters";

            // Избегаем дублирования моделей с одинаковым именем
            if (processedModels.Contains(modelName))
            {
                continue;
            }

            var model = Generate(query, options);
            if (model != null)
            {
                models.Add(model);
                processedModels.Add(modelName);
            }
        }

        return models;
    }

    /// <summary>
    /// Генерирует C# код модели параметров
    /// </summary>
    private static string GenerateModelCode(string modelName, IReadOnlyList<ModelProperty> properties, QueryGenerationOptions options)
    {
        var code = new QueryCodeBuilder(options);

        if (options.GenerateXmlDocumentation)
        {
            code.AppendXmlSummary($"Модель параметров запроса");
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
