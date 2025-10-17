using PgCs.Common.Formatting;
using PgCs.Common.Generation.Models;
using PgCs.Common.Mapping;
using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaAnalyzer.Models.Functions;
using PgCs.Common.SchemaGenerator.Models;

namespace PgCs.SchemaGenerator.Generation;

/// <summary>
/// Генератор C# моделей для параметров функций PostgreSQL
/// </summary>
internal sealed class FunctionModelGenerator : IFunctionModelGenerator
{
    /// <inheritdoc />
    public GeneratedModel? Generate(FunctionDefinition function, SchemaGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(function);
        ArgumentNullException.ThrowIfNull(options);

        // Если нет параметров, не генерируем модель
        if (function.Parameters.Count == 0)
        {
            return null;
        }

        // Генерируем только для функций с входными параметрами
        var inputParameters = function.Parameters
            .Where(p => p.Mode is ParameterMode.In or ParameterMode.InOut)
            .ToList();

        if (inputParameters.Count == 0)
        {
            return null;
        }

        var modelName = NamingHelper.ConvertName(
            $"{function.Name}Parameters",
            options.NamingStrategy,
            options.ModelPrefix,
            $"{options.ModelSuffix}Parameters");

        var code = new CodeBuilder(options.IndentationStyle, options.IndentationSize);

        // Собираем using директивы
        var usings = CollectUsings(inputParameters, options);
        code.AppendUsings(usings);

        code.AppendNamespaceStart(options.Namespace);

        // XML документация
        if (options.GenerateXmlDocumentation)
        {
            var summary = !string.IsNullOrWhiteSpace(function.Comment)
                ? $"Параметры функции {function.Name}: {function.Comment}"
                : $"Параметры функции {function.Schema ?? "public"}.{function.Name}";

            code.AppendXmlSummary(summary);
        }

        code.AppendTypeStart(
            modelName,
            isRecord: options.UseRecords,
            isSealed: true,
            isPartial: options.GeneratePartialClasses);

        // Генерируем свойства для параметров
        var properties = new List<ModelProperty>();

        foreach (var parameter in inputParameters)
        {
            var propertyName = NamingHelper.ConvertName(parameter.Name, options.NamingStrategy);
            propertyName = NamingHelper.EscapeIfKeyword(propertyName);

            // Параметр считается обязательным, если у него нет значения по умолчанию
            var isParameterRequired = string.IsNullOrEmpty(parameter.DefaultValue);

            var csharpType = TypeMapper.MapToCSharpType(
                parameter.DataType,
                isNullable: !isParameterRequired || !options.UseNullableReferenceTypes);

            // XML документация
            if (options.GenerateXmlDocumentation)
            {
                var summary = $"Параметр {parameter.Name} ({parameter.DataType})";
                if (parameter.Mode == ParameterMode.InOut)
                {
                    summary += " [IN/OUT]";
                }
                code.AppendXmlSummary(summary);
            }

            // Определяем значение по умолчанию
            string? defaultValue = null;
            if (!string.IsNullOrEmpty(parameter.DefaultValue))
            {
                defaultValue = ConvertDefaultValue(parameter.DefaultValue, csharpType);
            }

            code.AppendProperty(
                type: csharpType,
                name: propertyName,
                isRequired: isParameterRequired,
                hasInit: options.UseInitOnlyProperties,
                defaultValue: defaultValue,
                comment: null);

            properties.Add(new ModelProperty
            {
                Name = propertyName,
                CSharpType = csharpType,
                IsNullable = !isParameterRequired,
                IsRequired = isParameterRequired,
                DefaultValue = defaultValue,
                SourceColumnName = parameter.Name,
                PostgresType = parameter.DataType
            });
        }

        code.AppendTypeEnd();

        return new GeneratedModel
        {
            Name = modelName,
            SourceCode = code.ToString(),
            ModelType = ModelType.FunctionParameters,
            Namespace = options.Namespace,
            SourceObjectName = function.Name,
            SchemaName = function.Schema,
            Properties = properties,
            Documentation = function.Comment
        };
    }

    /// <summary>
    /// Преобразует значение по умолчанию PostgreSQL в C#
    /// </summary>
    private static string? ConvertDefaultValue(string postgresDefault, string csharpType)
    {
        var cleanDefault = postgresDefault.Trim().Trim('\'', '"');

        return cleanDefault switch
        {
            "NULL" => "null",
            "true" or "TRUE" => "true",
            "false" or "FALSE" => "false",
            _ when decimal.TryParse(cleanDefault, out _) => cleanDefault,
            _ when cleanDefault.StartsWith("'") && cleanDefault.EndsWith("'") => 
                $"\"{cleanDefault.Trim('\'')}\"",
            _ => null
        };
    }

    /// <summary>
    /// Собирает необходимые using директивы
    /// </summary>
    private static List<string> CollectUsings(
        IEnumerable<FunctionParameter> parameters,
        SchemaGenerationOptions options)
    {
        var usings = new List<string>();

        foreach (var parameter in parameters)
        {
            var requiredNamespace = TypeMapper.GetRequiredNamespace(parameter.DataType);
            if (requiredNamespace != null && !usings.Contains(requiredNamespace))
            {
                usings.Add(requiredNamespace);
            }
        }

        if (options.AdditionalUsings?.Count > 0)
        {
            usings.AddRange(options.AdditionalUsings);
        }

        return usings;
    }
}
