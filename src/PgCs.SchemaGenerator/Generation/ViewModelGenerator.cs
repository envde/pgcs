using PgCs.Common.SchemaAnalyzer.Models.Views;
using PgCs.Common.SchemaGenerator.Models;
using PgCs.SchemaGenerator.Formatting;
using PgCs.SchemaGenerator.Mapping;

namespace PgCs.SchemaGenerator.Generation;

/// <summary>
/// Генератор C# моделей для представлений (views) PostgreSQL
/// </summary>
internal sealed class ViewModelGenerator : IViewModelGenerator
{
    /// <inheritdoc />
    public GeneratedModel Generate(ViewDefinition view, SchemaGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(options);

        var modelName = NamingHelper.ConvertName(
            view.Name,
            options.NamingStrategy,
            options.ModelPrefix,
            options.ModelSuffix);

        var code = new CodeBuilder(options);

        // Собираем using директивы
        var usings = CollectUsings(view, options);
        code.AppendUsings(usings);

        // Namespace
        code.AppendNamespaceStart(options.Namespace);

        // XML документация
        if (options.GenerateXmlDocumentation)
        {
            var summary = !string.IsNullOrWhiteSpace(view.Comment)
                ? view.Comment
                : $"Модель представления {view.Schema ?? "public"}.{view.Name}";

            code.AppendXmlSummary(summary);

            if (view.IsMaterialized)
            {
                code.AppendXmlRemarks("Материализованное представление (требует REFRESH для обновления данных)");
            }
        }

        // Атрибуты маппинга
        if (options.GenerateMappingAttributes)
        {
            var viewName = string.IsNullOrWhiteSpace(view.Schema)
                ? view.Name
                : $"{view.Schema}.{view.Name}";

            code.AppendLine($"[Table(\"{viewName}\")]");
        }

        // Объявление record/class
        code.AppendTypeStart(
            modelName,
            isRecord: options.UseRecords,
            isSealed: true,
            isPartial: options.GeneratePartialClasses);

        // Генерация свойств из колонок
        var properties = new List<ModelProperty>();

        foreach (var column in view.Columns)
        {
            var propertyName = NamingHelper.ConvertName(column.Name, options.NamingStrategy);
            propertyName = NamingHelper.EscapeIfKeyword(propertyName);

            var csharpType = PostgresTypeMapper.MapToCSharpType(
                column.DataType,
                column.IsNullable || !options.UseNullableReferenceTypes,
                column.IsArray);

            // Views обычно read-only, поэтому required не нужен
            var isRequired = false;

            // XML документация
            if (options.GenerateXmlDocumentation)
            {
                var summary = !string.IsNullOrWhiteSpace(column.Comment)
                    ? column.Comment
                    : $"Колонка {column.Name} ({column.DataType})";

                code.AppendXmlSummary(summary);
            }

            // Атрибуты маппинга
            if (options.GenerateMappingAttributes && propertyName != column.Name)
            {
                code.AppendLine($"[Column(\"{column.Name}\")]");
            }

            // Генерируем свойство
            code.AppendProperty(
                type: csharpType,
                name: propertyName,
                isRequired: isRequired,
                hasInit: options.UseInitOnlyProperties,
                defaultValue: null,
                comment: null);

            properties.Add(new ModelProperty
            {
                Name = propertyName,
                CSharpType = csharpType,
                IsNullable = column.IsNullable,
                IsRequired = isRequired,
                Documentation = column.Comment,
                SourceColumnName = column.Name,
                PostgresType = column.DataType
            });
        }

        code.AppendTypeEnd();

        return new GeneratedModel
        {
            Name = modelName,
            SourceCode = code.ToString(),
            ModelType = ModelType.View,
            Namespace = options.Namespace,
            SourceObjectName = view.Name,
            SchemaName = view.Schema,
            Properties = properties,
            Documentation = view.Comment
        };
    }

    /// <summary>
    /// Собирает необходимые using директивы
    /// </summary>
    private static List<string> CollectUsings(ViewDefinition view, SchemaGenerationOptions options)
    {
        var usings = new List<string>();

        if (options.GenerateMappingAttributes)
        {
            usings.Add("System.ComponentModel.DataAnnotations.Schema");
        }

        // Проверяем специальные типы в колонках
        foreach (var column in view.Columns)
        {
            var requiredNamespace = PostgresTypeMapper.GetRequiredNamespace(column.DataType);
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
