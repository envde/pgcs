using PgCs.Common.SchemaAnalyzer.Models.Types;
using PgCs.Common.SchemaGenerator.Models;
using PgCs.SchemaGenerator.Formatting;
using PgCs.SchemaGenerator.Mapping;

namespace PgCs.SchemaGenerator.Generation;

/// <summary>
/// Генератор C# моделей для пользовательских типов PostgreSQL
/// </summary>
internal sealed class TypeModelGenerator : ITypeModelGenerator
{
    /// <inheritdoc />
    public GeneratedModel Generate(TypeDefinition type, SchemaGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(options);

        return type.Kind switch
        {
            TypeKind.Enum => GenerateEnum(type, options),
            TypeKind.Composite => GenerateComposite(type, options),
            TypeKind.Domain => GenerateDomain(type, options),
            TypeKind.Range => GenerateRange(type, options),
            _ => throw new NotSupportedException($"Тип {type.Kind} не поддерживается")
        };
    }

    /// <summary>
    /// Генерирует C# enum для PostgreSQL ENUM
    /// </summary>
    private GeneratedModel GenerateEnum(TypeDefinition type, SchemaGenerationOptions options)
    {
        var enumName = NamingHelper.ConvertName(
            type.Name,
            options.NamingStrategy,
            options.ModelPrefix,
            options.ModelSuffix);

        var code = new CodeBuilder(options);

        code.AppendNamespaceStart(options.Namespace);

        // XML документация
        if (options.GenerateXmlDocumentation)
        {
            var summary = !string.IsNullOrWhiteSpace(type.Comment)
                ? type.Comment
                : $"Перечисление {type.Schema ?? "public"}.{type.Name}";

            code.AppendXmlSummary(summary);
        }

        code.AppendEnumStart(enumName);

        // Генерируем значения
        for (int i = 0; i < type.EnumValues.Count; i++)
        {
            var value = type.EnumValues[i];
            var valueName = NamingHelper.ConvertName(value, NamingStrategy.PascalCase);
            var isLast = i == type.EnumValues.Count - 1;

            var comment = $"Значение '{value}'";
            code.AppendEnumValue(valueName, comment, isLast);
        }

        code.AppendTypeEnd();

        return new GeneratedModel
        {
            Name = enumName,
            SourceCode = code.ToString(),
            ModelType = ModelType.Enum,
            Namespace = options.Namespace,
            SourceObjectName = type.Name,
            SchemaName = type.Schema,
            Documentation = type.Comment
        };
    }

    /// <summary>
    /// Генерирует C# class/record для PostgreSQL COMPOSITE TYPE
    /// </summary>
    private GeneratedModel GenerateComposite(TypeDefinition type, SchemaGenerationOptions options)
    {
        var typeName = NamingHelper.ConvertName(
            type.Name,
            options.NamingStrategy,
            options.ModelPrefix,
            options.ModelSuffix);

        var code = new CodeBuilder(options);

        // Собираем using директивы
        var usings = new List<string>();
        foreach (var attr in type.CompositeAttributes)
        {
            var ns = PostgresTypeMapper.GetRequiredNamespace(attr.DataType);
            if (ns != null && !usings.Contains(ns))
            {
                usings.Add(ns);
            }
        }

        code.AppendUsings(usings);
        code.AppendNamespaceStart(options.Namespace);

        // XML документация
        if (options.GenerateXmlDocumentation)
        {
            var summary = !string.IsNullOrWhiteSpace(type.Comment)
                ? type.Comment
                : $"Композитный тип {type.Schema ?? "public"}.{type.Name}";

            code.AppendXmlSummary(summary);
        }

        code.AppendTypeStart(
            typeName,
            isRecord: options.UseRecords,
            isSealed: true,
            isPartial: options.GeneratePartialClasses);

        // Генерируем свойства
        var properties = new List<ModelProperty>();

        foreach (var attr in type.CompositeAttributes)
        {
            var propertyName = NamingHelper.ConvertName(attr.Name, options.NamingStrategy);
            propertyName = NamingHelper.EscapeIfKeyword(propertyName);

            var csharpType = PostgresTypeMapper.MapToCSharpType(
                attr.DataType,
                isNullable: true, // Композитные типы обычно nullable
                isArray: false);

            if (options.GenerateXmlDocumentation)
            {
                code.AppendXmlSummary($"Атрибут {attr.Name} ({attr.DataType})");
            }

            code.AppendProperty(
                type: csharpType,
                name: propertyName,
                isRequired: false,
                hasInit: options.UseInitOnlyProperties,
                defaultValue: null,
                comment: null);

            properties.Add(new ModelProperty
            {
                Name = propertyName,
                CSharpType = csharpType,
                IsNullable = true,
                IsRequired = false,
                SourceColumnName = attr.Name,
                PostgresType = attr.DataType
            });
        }

        code.AppendTypeEnd();

        return new GeneratedModel
        {
            Name = typeName,
            SourceCode = code.ToString(),
            ModelType = ModelType.CustomType,
            Namespace = options.Namespace,
            SourceObjectName = type.Name,
            SchemaName = type.Schema,
            Properties = properties,
            Documentation = type.Comment
        };
    }

    /// <summary>
    /// Генерирует C# type alias для PostgreSQL DOMAIN
    /// </summary>
    private GeneratedModel GenerateDomain(TypeDefinition type, SchemaGenerationOptions options)
    {
        var domainName = NamingHelper.ConvertName(
            type.Name,
            options.NamingStrategy,
            options.ModelPrefix,
            options.ModelSuffix);

        var code = new CodeBuilder(options);

        code.AppendNamespaceStart(options.Namespace);

        // XML документация
        if (options.GenerateXmlDocumentation)
        {
            var summary = !string.IsNullOrWhiteSpace(type.Comment)
                ? type.Comment
                : $"Доменный тип {type.Schema ?? "public"}.{type.Name}";

            code.AppendXmlSummary(summary);

            if (type.DomainInfo != null)
            {
                var remarks = $"Базовый тип: {type.DomainInfo.BaseType}";
                if (type.DomainInfo.CheckConstraints.Count > 0)
                {
                    remarks += $"\nОграничения: {string.Join(", ", type.DomainInfo.CheckConstraints)}";
                }
                code.AppendXmlRemarks(remarks);
            }
        }

        // В C# нет прямого аналога DOMAIN, используем record с одним свойством
        code.AppendTypeStart(
            domainName,
            isRecord: true,
            isSealed: true,
            isPartial: false);

        var baseType = type.DomainInfo?.BaseType ?? "text";
        var csharpType = PostgresTypeMapper.MapToCSharpType(baseType, isNullable: false, isArray: false);

        code.AppendXmlSummary("Значение доменного типа");
        code.AppendProperty(
            type: csharpType,
            name: "Value",
            isRequired: true,
            hasInit: true,
            defaultValue: null,
            comment: null);

        code.AppendTypeEnd();

        return new GeneratedModel
        {
            Name = domainName,
            SourceCode = code.ToString(),
            ModelType = ModelType.CustomType,
            Namespace = options.Namespace,
            SourceObjectName = type.Name,
            SchemaName = type.Schema,
            Documentation = type.Comment,
            Properties =
            [
                new ModelProperty
                {
                    Name = "Value",
                    CSharpType = csharpType,
                    IsNullable = false,
                    IsRequired = true,
                    PostgresType = baseType
                }
            ]
        };
    }

    /// <summary>
    /// Генерирует модель для PostgreSQL RANGE типа
    /// </summary>
    private GeneratedModel GenerateRange(TypeDefinition type, SchemaGenerationOptions options)
    {
        // Range типы в C# можно представить как record с Start и End
        var rangeName = NamingHelper.ConvertName(
            type.Name,
            options.NamingStrategy,
            options.ModelPrefix,
            options.ModelSuffix);

        var code = new CodeBuilder(options);

        code.AppendNamespaceStart(options.Namespace);

        if (options.GenerateXmlDocumentation)
        {
            var summary = !string.IsNullOrWhiteSpace(type.Comment)
                ? type.Comment
                : $"Диапазонный тип {type.Schema ?? "public"}.{type.Name}";

            code.AppendXmlSummary(summary);
        }

        code.AppendTypeStart(
            rangeName,
            isRecord: true,
            isSealed: true,
            isPartial: false);

        // Для простоты используем string, в реальности нужно определять тип элемента
        code.AppendXmlSummary("Начало диапазона");
        code.AppendProperty("string?", "Start", isRequired: false, hasInit: true);

        code.AppendXmlSummary("Конец диапазона");
        code.AppendProperty("string?", "End", isRequired: false, hasInit: true);

        code.AppendTypeEnd();

        return new GeneratedModel
        {
            Name = rangeName,
            SourceCode = code.ToString(),
            ModelType = ModelType.CustomType,
            Namespace = options.Namespace,
            SourceObjectName = type.Name,
            SchemaName = type.Schema,
            Documentation = type.Comment
        };
    }
}
