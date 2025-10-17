namespace PgCs.Common.Services;

/// <summary>
/// Реализация маппера типов PostgreSQL в C# типы
/// </summary>
public sealed class PostgreSqlTypeMapper : ITypeMapper
{
    private static readonly Dictionary<string, string> TypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Числовые типы
        ["smallint"] = "short",
        ["int2"] = "short",
        ["integer"] = "int",
        ["int"] = "int",
        ["int4"] = "int",
        ["bigint"] = "long",
        ["int8"] = "long",
        ["decimal"] = "decimal",
        ["numeric"] = "decimal",
        ["real"] = "float",
        ["float4"] = "float",
        ["double precision"] = "double",
        ["float8"] = "double",
        ["money"] = "decimal",

        // Строковые типы
        ["character varying"] = "string",
        ["varchar"] = "string",
        ["character"] = "string",
        ["char"] = "string",
        ["text"] = "string",
        ["citext"] = "string",

        // Дата и время
        ["timestamp"] = "DateTime",
        ["timestamp without time zone"] = "DateTime",
        ["timestamp with time zone"] = "DateTimeOffset",
        ["timestamptz"] = "DateTimeOffset",
        ["date"] = "DateOnly",
        ["time"] = "TimeOnly",
        ["time without time zone"] = "TimeOnly",
        ["time with time zone"] = "TimeSpan",
        ["timetz"] = "TimeSpan",
        ["interval"] = "TimeSpan",

        // Boolean
        ["boolean"] = "bool",
        ["bool"] = "bool",

        // UUID
        ["uuid"] = "Guid",

        // Binary
        ["bytea"] = "byte[]",

        // JSON
        ["json"] = "string",
        ["jsonb"] = "string",

        // XML
        ["xml"] = "string",

        // Network
        ["inet"] = "System.Net.IPAddress",
        ["cidr"] = "string",
        ["macaddr"] = "System.Net.NetworkInformation.PhysicalAddress",
        ["macaddr8"] = "System.Net.NetworkInformation.PhysicalAddress",

        // Geometry (PostGIS)
        ["geometry"] = "object",
        ["geography"] = "object",
        ["point"] = "object",
        ["line"] = "object",
        ["lseg"] = "object",
        ["box"] = "object",
        ["path"] = "object",
        ["polygon"] = "object",
        ["circle"] = "object",

        // Range types
        ["int4range"] = "NpgsqlRange<int>",
        ["int8range"] = "NpgsqlRange<long>",
        ["numrange"] = "NpgsqlRange<decimal>",
        ["tsrange"] = "NpgsqlRange<DateTime>",
        ["tstzrange"] = "NpgsqlRange<DateTimeOffset>",
        ["daterange"] = "NpgsqlRange<DateOnly>",

        // Специальные типы
        ["bit"] = "System.Collections.BitArray",
        ["bit varying"] = "System.Collections.BitArray",
        ["varbit"] = "System.Collections.BitArray"
    };

    private static readonly Dictionary<string, string> NamespaceMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["uuid"] = "System",
        ["inet"] = "System.Net",
        ["macaddr"] = "System.Net.NetworkInformation",
        ["macaddr8"] = "System.Net.NetworkInformation",
        ["bit"] = "System.Collections",
        ["bit varying"] = "System.Collections",
        ["varbit"] = "System.Collections",
        ["int4range"] = "NpgsqlTypes",
        ["int8range"] = "NpgsqlTypes",
        ["numrange"] = "NpgsqlTypes",
        ["tsrange"] = "NpgsqlTypes",
        ["tstzrange"] = "NpgsqlTypes",
        ["daterange"] = "NpgsqlTypes"
    };

    public string MapType(string postgresType, bool isNullable, bool isArray)
    {
        // Убираем размерности типа (например: varchar(100) -> varchar)
        var cleanType = CleanTypeName(postgresType);

        // Получаем базовый C# тип
        var csharpType = TypeMap.TryGetValue(cleanType, out var mapped)
            ? mapped
            : "object"; // Для неизвестных типов используем object

        // Обрабатываем массивы
        if (isArray)
        {
            return $"{csharpType}[]" + (isNullable ? "?" : "");
        }

        // Добавляем nullable для value types
        if (isNullable && IsValueType(csharpType))
        {
            return $"{csharpType}?";
        }

        return csharpType;
    }

    public string? GetRequiredNamespace(string postgresType)
    {
        var cleanType = CleanTypeName(postgresType);
        return NamespaceMap.TryGetValue(cleanType, out var ns) ? ns : null;
    }

    /// <summary>
    /// Очищает имя типа от размерности и дополнительных параметров
    /// Например: varchar(100) -> varchar, numeric(10,2) -> numeric
    /// </summary>
    private static string CleanTypeName(string postgresType)
    {
        var parenIndex = postgresType.IndexOf('(');
        return parenIndex > 0
            ? postgresType[..parenIndex].Trim()
            : postgresType.Trim();
    }

    /// <summary>
    /// Проверяет, является ли C# тип value type
    /// </summary>
    private static bool IsValueType(string csharpType)
    {
        return csharpType switch
        {
            "string" => false,
            "object" => false,
            "byte[]" => false,
            _ when csharpType.Contains("[]") => false,
            _ when csharpType.Contains('.') => false, // Предполагаем, что типы с namespace - reference types
            _ => true
        };
    }
}
