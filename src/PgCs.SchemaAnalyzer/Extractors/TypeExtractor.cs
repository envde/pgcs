using System.Text.RegularExpressions;
using PgCs.Common.SchemaAnalyzer;

namespace PgCs.SchemaAnalyzer.Extractors;

/// <summary>
/// Извлекает определения пользовательских типов
/// </summary>
internal sealed partial class TypeExtractor : BaseExtractor<TypeDefinition>
{
    [GeneratedRegex(@"CREATE\s+TYPE\s+([a-zA-Z_][a-zA-Z0-9_.]*)\s+AS\s+(ENUM|RANGE)?\s*\((.*?)\)|CREATE\s+DOMAIN\s+([a-zA-Z_][a-zA-Z0-9_.]*)\s+AS\s+([a-zA-Z0-9_]+(?:\([^)]+\))?)(.*)", 
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex TypePatternRegex();

    protected override Regex Pattern => TypePatternRegex();

    protected override TypeDefinition? ParseMatch(Match match, string statement)
    {
        // DOMAIN type
        if (match.Groups[4].Success)
        {
            return ParseDomainType(match, statement);
        }

        // ENUM or COMPOSITE type
        var fullTypeName = match.Groups[1].Value.Trim();
        var typeKind = match.Groups[2].Value.Trim();
        var typeBody = match.Groups[3].Value.Trim();

        var schema = ExtractSchemaName(fullTypeName);
        var typeName = ExtractTableName(fullTypeName);

        if (typeKind.Equals("ENUM", StringComparison.OrdinalIgnoreCase))
        {
            return ParseEnumType(typeName, schema, typeBody, statement);
        }

        return ParseCompositeType(typeName, schema, typeBody, statement);
    }

    private TypeDefinition ParseEnumType(string name, string? schema, string enumValues, string rawSql)
    {
        var values = enumValues
            .Split(',')
            .Select(v => v.Trim().Trim('\'', '"'))
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToArray();

        return new TypeDefinition
        {
            Name = name,
            Schema = schema,
            Kind = TypeKind.Enum,
            EnumValues = values,
            RawSql = rawSql
        };
    }

    private TypeDefinition ParseCompositeType(string name, string? schema, string attributes, string rawSql)
    {
        var attributeList = new List<CompositeTypeAttribute>();
        var parts = attributes.Split(',');

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            var tokens = trimmed.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length >= 2)
            {
                var attrName = tokens[0].Trim();
                var dataType = string.Join(" ", tokens.Skip(1));

                attributeList.Add(new CompositeTypeAttribute
                {
                    Name = attrName,
                    DataType = dataType
                });
            }
        }

        return new TypeDefinition
        {
            Name = name,
            Schema = schema,
            Kind = TypeKind.Composite,
            CompositeAttributes = attributeList,
            RawSql = rawSql
        };
    }

    private TypeDefinition ParseDomainType(Match match, string statement)
    {
        var domainName = match.Groups[4].Value.Trim();
        var baseType = match.Groups[5].Value.Trim();
        var constraints = match.Groups[6].Value.Trim();

        var checkConstraints = new List<string>();
        var checkMatches = Regex.Matches(constraints, @"CHECK\s*\((.*?)\)", RegexOptions.IgnoreCase);

        foreach (Match checkMatch in checkMatches)
        {
            checkConstraints.Add(checkMatch.Groups[1].Value);
        }

        var isNotNull = constraints.Contains("NOT NULL", StringComparison.OrdinalIgnoreCase);
        var defaultMatch = Regex.Match(constraints, @"DEFAULT\s+([^,\s]+)", RegexOptions.IgnoreCase);
        var defaultValue = defaultMatch.Success ? defaultMatch.Groups[1].Value : null;

        return new TypeDefinition
        {
            Name = domainName,
            Kind = TypeKind.Domain,
            DomainInfo = new DomainTypeInfo
            {
                BaseType = baseType,
                IsNotNull = isNotNull,
                DefaultValue = defaultValue,
                CheckConstraints = checkConstraints
            },
            RawSql = statement
        };
    }
}