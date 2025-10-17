using PgCs.Common.SchemaAnalyzer.Models.Tables;
using PgCs.Common.SchemaGenerator.Models;
using PgCs.SchemaGenerator.Formatting;
using PgCs.SchemaGenerator.Mapping;

namespace PgCs.SchemaGenerator.Generation;

/// <summary>
/// Генератор C# моделей для таблиц PostgreSQL
/// </summary>
internal sealed class TableModelGenerator : ITableModelGenerator
{
    /// <inheritdoc />
    public GeneratedModel Generate(TableDefinition table, SchemaGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(options);

        var modelName = NamingHelper.ConvertName(
            table.Name,
            options.NamingStrategy,
            options.ModelPrefix,
            options.ModelSuffix);

        var code = new CodeBuilder(options);

        // Собираем using директивы
        var usings = CollectUsings(table, options);
        code.AppendUsings(usings);

        // Namespace
        code.AppendNamespaceStart(options.Namespace);

        // XML документация для класса
        if (options.GenerateXmlDocumentation)
        {
            var summary = !string.IsNullOrWhiteSpace(table.Comment)
                ? table.Comment
                : $"Модель таблицы {table.Schema ?? "public"}.{table.Name}";

            code.AppendXmlSummary(summary);

            if (table.IsPartitioned && table.PartitionInfo != null)
            {
                code.AppendXmlRemarks($"Партиционированная таблица: {table.PartitionInfo.Strategy}");
            }
        }

        // Атрибуты для маппинга
        if (options.GenerateMappingAttributes)
        {
            var tableName = string.IsNullOrWhiteSpace(table.Schema)
                ? table.Name
                : $"{table.Schema}.{table.Name}";

            code.AppendLine($"[Table(\"{tableName}\")]");
        }

        // Объявление record/class
        code.AppendTypeStart(
            modelName,
            isRecord: options.UseRecords,
            isSealed: true,
            isPartial: options.GeneratePartialClasses);

        // Генерация свойств
        var properties = new List<ModelProperty>();
        var isFirstProperty = true;

        foreach (var column in table.Columns)
        {
            if (!isFirstProperty)
            {
                // Пустая строка между свойствами для читаемости (опционально)
                // code.AppendLine();
            }

            var property = GenerateProperty(column, options, code);
            properties.Add(property);
            isFirstProperty = false;
        }

        code.AppendTypeEnd();

        return new GeneratedModel
        {
            Name = modelName,
            SourceCode = code.ToString(),
            ModelType = ModelType.Table,
            Namespace = options.Namespace,
            SourceObjectName = table.Name,
            SchemaName = table.Schema,
            Properties = properties,
            Documentation = table.Comment
        };
    }

    /// <summary>
    /// Генерирует свойство для колонки
    /// </summary>
    private ModelProperty GenerateProperty(
        ColumnDefinition column,
        SchemaGenerationOptions options,
        CodeBuilder code)
    {
        var propertyName = NamingHelper.ConvertName(column.Name, options.NamingStrategy);
        propertyName = NamingHelper.EscapeIfKeyword(propertyName);

        var csharpType = TypeMapper.MapToCSharpType(
            column.DataType,
            column.IsNullable || !options.UseNullableReferenceTypes,
            column.IsArray);

        // Определяем, требуется ли required modifier
        var isRequired = !column.IsNullable &&
                        !column.IsPrimaryKey && // PK обычно автогенерируемые
                        string.IsNullOrEmpty(column.DefaultValue) &&
                        options.UseInitOnlyProperties;

        // XML документация
        if (options.GenerateXmlDocumentation)
        {
            var summary = !string.IsNullOrWhiteSpace(column.Comment)
                ? column.Comment
                : $"Колонка {column.Name} ({column.DataType})";

            code.AppendXmlSummary(summary);
        }

        // Data Annotations
        if (options.GenerateDataAnnotations)
        {
            GenerateDataAnnotations(column, code);
        }

        // Атрибуты маппинга
        if (options.GenerateMappingAttributes)
        {
            if (propertyName != column.Name)
            {
                code.AppendLine($"[Column(\"{column.Name}\")]");
            }

            if (column.IsPrimaryKey)
            {
                code.AppendLine("[Key]");
            }
        }

        // Определяем значение по умолчанию
        string? defaultValue = null;
        if (!string.IsNullOrEmpty(column.DefaultValue) && !column.IsPrimaryKey)
        {
            defaultValue = ConvertDefaultValue(column.DefaultValue, csharpType);
        }

        // Генерируем свойство
        code.AppendProperty(
            type: csharpType,
            name: propertyName,
            isRequired: isRequired,
            hasInit: options.UseInitOnlyProperties,
            defaultValue: defaultValue,
            comment: null); // Комментарий уже добавлен выше

        return new ModelProperty
        {
            Name = propertyName,
            CSharpType = csharpType,
            IsNullable = column.IsNullable,
            IsRequired = isRequired,
            DefaultValue = defaultValue,
            Documentation = column.Comment,
            SourceColumnName = column.Name,
            PostgresType = column.DataType
        };
    }

    /// <summary>
    /// Генерирует Data Annotations для валидации
    /// </summary>
    private static void GenerateDataAnnotations(ColumnDefinition column, CodeBuilder code)
    {
        if (!column.IsNullable)
        {
            code.AppendLine("[Required]");
        }

        if (column.MaxLength.HasValue)
        {
            code.AppendLine($"[MaxLength({column.MaxLength.Value})]");
        }

        if (column.DataType.Contains("varchar", StringComparison.OrdinalIgnoreCase) ||
            column.DataType.Contains("text", StringComparison.OrdinalIgnoreCase))
        {
            if (!column.IsNullable)
            {
                code.AppendLine("[StringLength(int.MaxValue, MinimumLength = 1)]");
            }
        }

        // Email validation для типа email (domain)
        if (column.DataType.Equals("email", StringComparison.OrdinalIgnoreCase))
        {
            code.AppendLine("[EmailAddress]");
        }
    }

    /// <summary>
    /// Преобразует значение по умолчанию PostgreSQL в C#
    /// </summary>
    private static string? ConvertDefaultValue(string postgresDefault, string csharpType)
    {
        // Убираем лишние части (CAST, ::type)
        var cleanDefault = postgresDefault.Trim()
            .Replace("::text", "")
            .Replace("::integer", "")
            .Replace("::bigint", "")
            .Trim('\'', '"');

        return cleanDefault switch
        {
            "true" or "TRUE" => "true",
            "false" or "FALSE" => "false",
            "NOW()" or "CURRENT_TIMESTAMP" => csharpType.Contains("DateTimeOffset") 
                ? "DateTimeOffset.UtcNow" 
                : "DateTime.UtcNow",
            "CURRENT_DATE" => "DateOnly.FromDateTime(DateTime.UtcNow)",
            "gen_random_uuid()" => "Guid.NewGuid()",
            "{}" when csharpType == "string" => "\"{}\"",
            "[]" when csharpType == "string" => "\"[]\"",
            _ when decimal.TryParse(cleanDefault, out _) => cleanDefault,
            _ => null
        };
    }

    /// <summary>
    /// Собирает необходимые using директивы
    /// </summary>
    private static List<string> CollectUsings(TableDefinition table, SchemaGenerationOptions options)
    {
        var usings = new List<string>();

        if (options.GenerateDataAnnotations)
        {
            usings.Add("System.ComponentModel.DataAnnotations");
        }

        if (options.GenerateMappingAttributes)
        {
            usings.Add("System.ComponentModel.DataAnnotations.Schema");
        }

        // Проверяем, используются ли специальные типы
        foreach (var column in table.Columns)
        {
            var requiredNamespace = TypeMapper.GetRequiredNamespace(column.DataType);
            if (requiredNamespace != null && !usings.Contains(requiredNamespace))
            {
                usings.Add(requiredNamespace);
            }
        }

        // Добавляем пользовательские using директивы
        if (options.AdditionalUsings?.Count > 0)
        {
            usings.AddRange(options.AdditionalUsings);
        }

        return usings;
    }
}
