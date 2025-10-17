namespace PgCs.SchemaGenerator.Mapping;

/// <summary>
/// Маппер типов данных PostgreSQL в C# типы
/// </summary>
internal static class TypeMapper
{
    /// <summary>
    /// Маппинг типов PostgreSQL на C# типы
    /// </summary>
    private static readonly Dictionary<string, string> TypeMapping = new(StringComparer.OrdinalIgnoreCase)
    {
        // Целочисленные типы
        ["smallint"] = "short",
        ["int2"] = "short",
        ["integer"] = "int",
        ["int"] = "int",
        ["int4"] = "int",
        ["bigint"] = "long",
        ["int8"] = "long",
        ["smallserial"] = "short",
        ["serial"] = "int",
        ["serial4"] = "int",
        ["bigserial"] = "long",
        ["serial8"] = "long",

        // Числа с плавающей точкой
        ["real"] = "float",
        ["float4"] = "float",
        ["double precision"] = "double",
        ["float8"] = "double",
        ["numeric"] = "decimal",
        ["decimal"] = "decimal",
        ["money"] = "decimal",

        // Строковые типы
        ["character varying"] = "string",
        ["varchar"] = "string",
        ["character"] = "string",
        ["char"] = "string",
        ["text"] = "string",
        ["citext"] = "string",
        ["name"] = "string",

        // Булевы типы
        ["boolean"] = "bool",
        ["bool"] = "bool",

        // Дата и время
        ["timestamp"] = "DateTime",
        ["timestamp without time zone"] = "DateTime",
        ["timestamp with time zone"] = "DateTimeOffset",
        ["timestamptz"] = "DateTimeOffset",
        ["date"] = "DateOnly",
        ["time"] = "TimeOnly",
        ["time without time zone"] = "TimeOnly",
        ["time with time zone"] = "DateTimeOffset",
        ["timetz"] = "DateTimeOffset",
        ["interval"] = "TimeSpan",

        // UUID
        ["uuid"] = "Guid",

        // JSON
        ["json"] = "string",
        ["jsonb"] = "string",

        // Бинарные данные
        ["bytea"] = "byte[]",

        // Геометрические типы (базовая поддержка)
        ["point"] = "string",
        ["line"] = "string",
        ["lseg"] = "string",
        ["box"] = "string",
        ["path"] = "string",
        ["polygon"] = "string",
        ["circle"] = "string",

        // Сетевые типы
        ["inet"] = "string",
        ["cidr"] = "string",
        ["macaddr"] = "string",
        ["macaddr8"] = "string",

        // Битовые строки
        ["bit"] = "bool",
        ["bit varying"] = "string",
        ["varbit"] = "string",

        // Полнотекстовый поиск
        ["tsvector"] = "string",
        ["tsquery"] = "string",

        // XML
        ["xml"] = "string",

        // Диапазоны
        ["int4range"] = "string",
        ["int8range"] = "string",
        ["numrange"] = "string",
        ["tsrange"] = "string",
        ["tstzrange"] = "string",
        ["daterange"] = "string",

        // Прочие
        ["oid"] = "uint",
        ["regclass"] = "string",
        ["regtype"] = "string"
    };

    /// <summary>
    /// Преобразует тип PostgreSQL в соответствующий C# тип
    /// </summary>
    /// <param name="postgresType">Тип PostgreSQL</param>
    /// <param name="isNullable">Является ли тип nullable</param>
    /// <param name="isArray">Является ли тип массивом</param>
    /// <returns>C# тип</returns>
    public static string MapToCSharpType(string postgresType, bool isNullable, bool isArray = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(postgresType);

        // Удаляем [] для массивов, если есть
        var cleanType = postgresType.Replace("[]", "").Trim();

        // Извлекаем базовый тип (без размера и параметров)
        var baseType = ExtractBaseType(cleanType);

        // Получаем соответствующий C# тип
        var csharpType = TypeMapping.TryGetValue(baseType, out var mapped)
            ? mapped
            : "string"; // По умолчанию используем string для неизвестных типов

        // Обрабатываем массивы
        if (isArray)
        {
            return $"{csharpType}[]";
        }

        // Добавляем nullable для value types
        if (isNullable && IsValueType(csharpType))
        {
            return $"{csharpType}?";
        }

        // Для reference types nullable добавляется через аннотацию в генераторе
        return csharpType;
    }

    /// <summary>
    /// Извлекает базовый тип без параметров (например, varchar(255) → varchar)
    /// </summary>
    private static string ExtractBaseType(string postgresType)
    {
        var parenIndex = postgresType.IndexOf('(');
        return parenIndex > 0 ? postgresType[..parenIndex].Trim() : postgresType;
    }

    /// <summary>
    /// Проверяет, является ли C# тип value type
    /// </summary>
    private static bool IsValueType(string csharpType)
    {
        return csharpType switch
        {
            "string" => false,
            "byte[]" => false,
            "object" => false,
            _ when csharpType.EndsWith("[]") => false,
            _ => true
        };
    }

    /// <summary>
    /// Получает namespace для специальных типов
    /// </summary>
    public static string? GetRequiredNamespace(string postgresType)
    {
        var baseType = ExtractBaseType(postgresType.Replace("[]", "").Trim());

        return baseType.ToLowerInvariant() switch
        {
            "json" or "jsonb" => null, // Используем string
            "inet" or "cidr" or "macaddr" => "System.Net",
            _ => null
        };
    }
}
