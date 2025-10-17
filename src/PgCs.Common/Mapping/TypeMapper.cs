using System.Collections.Frozen;
using Npgsql;
using NpgsqlTypes;

namespace PgCs.Common.Mapping;

/// <summary>
/// Маппер типов данных PostgreSQL в C# типы и NpgsqlDbType
/// </summary>
public static class TypeMapper
{
    /// <summary>
    /// Маппинг типов PostgreSQL на C# типы
    /// </summary>
    private static readonly FrozenDictionary<string, string> TypeToCSharpMapping =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
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

            // Геометрические типы
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
        }.ToFrozenDictionary();

    /// <summary>
    /// Маппинг типов PostgreSQL на NpgsqlDbType
    /// </summary>
    private static readonly FrozenDictionary<string, NpgsqlDbType> TypeToNpgsqlDbTypeMapping =
        new Dictionary<string, NpgsqlDbType>(StringComparer.OrdinalIgnoreCase)
        {
            ["smallint"] = NpgsqlDbType.Smallint,
            ["int2"] = NpgsqlDbType.Smallint,
            ["integer"] = NpgsqlDbType.Integer,
            ["int"] = NpgsqlDbType.Integer,
            ["int4"] = NpgsqlDbType.Integer,
            ["bigint"] = NpgsqlDbType.Bigint,
            ["int8"] = NpgsqlDbType.Bigint,

            ["real"] = NpgsqlDbType.Real,
            ["float4"] = NpgsqlDbType.Real,
            ["double precision"] = NpgsqlDbType.Double,
            ["float8"] = NpgsqlDbType.Double,
            ["numeric"] = NpgsqlDbType.Numeric,
            ["decimal"] = NpgsqlDbType.Numeric,
            ["money"] = NpgsqlDbType.Money,

            ["varchar"] = NpgsqlDbType.Varchar,
            ["character varying"] = NpgsqlDbType.Varchar,
            ["char"] = NpgsqlDbType.Char,
            ["character"] = NpgsqlDbType.Char,
            ["text"] = NpgsqlDbType.Text,
            ["citext"] = NpgsqlDbType.Citext,

            ["boolean"] = NpgsqlDbType.Boolean,
            ["bool"] = NpgsqlDbType.Boolean,

            ["timestamp"] = NpgsqlDbType.Timestamp,
            ["timestamp without time zone"] = NpgsqlDbType.Timestamp,
            ["timestamp with time zone"] = NpgsqlDbType.TimestampTz,
            ["timestamptz"] = NpgsqlDbType.TimestampTz,
            ["date"] = NpgsqlDbType.Date,
            ["time"] = NpgsqlDbType.Time,
            ["time without time zone"] = NpgsqlDbType.Time,
            ["interval"] = NpgsqlDbType.Interval,

            ["uuid"] = NpgsqlDbType.Uuid,

            ["json"] = NpgsqlDbType.Json,
            ["jsonb"] = NpgsqlDbType.Jsonb,

            ["bytea"] = NpgsqlDbType.Bytea,

            ["inet"] = NpgsqlDbType.Inet,
            ["cidr"] = NpgsqlDbType.Cidr,
            ["macaddr"] = NpgsqlDbType.MacAddr
        }.ToFrozenDictionary();

    /// <summary>
    /// Преобразует тип PostgreSQL в соответствующий C# тип
    /// </summary>
    /// <param name="postgresType">Тип PostgreSQL</param>
    /// <param name="isNullable">Является ли тип nullable</param>
    /// <returns>C# тип</returns>
    public static string MapToCSharpType(string postgresType, bool isNullable = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(postgresType);

        // Проверяем массив
        var isArray = postgresType.EndsWith("[]");
        var cleanType = postgresType.Replace("[]", "").Trim();

        // Извлекаем базовый тип
        var baseType = ExtractBaseType(cleanType);

        // Получаем C# тип
        var csharpType = TypeToCSharpMapping.GetValueOrDefault(baseType, "string");

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

        return csharpType;
    }

    /// <summary>
    /// Преобразует тип PostgreSQL в NpgsqlDbType
    /// </summary>
    /// <param name="postgresType">Тип PostgreSQL</param>
    /// <returns>NpgsqlDbType или null, если маппинг не найден</returns>
    public static NpgsqlDbType? MapToNpgsqlDbType(string postgresType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(postgresType);

        var cleanType = postgresType.Replace("[]", "").Trim();
        var baseType = ExtractBaseType(cleanType);
        var isArray = postgresType.EndsWith("[]");

        if (TypeToNpgsqlDbTypeMapping.TryGetValue(baseType, out var dbType))
        {
            return isArray ? dbType | NpgsqlDbType.Array : dbType;
        }

        return null;
    }

    /// <summary>
    /// Получает метод расширения для чтения значения из NpgsqlDataReader
    /// </summary>
    /// <param name="csharpType">C# тип</param>
    /// <returns>Имя метода для чтения значения</returns>
    public static string GetReaderMethod(string csharpType) =>
        csharpType switch
        {
            "short" or "short?" => "GetInt16",
            "int" or "int?" => "GetInt32",
            "long" or "long?" => "GetInt64",
            "float" or "float?" => "GetFloat",
            "double" or "double?" => "GetDouble",
            "decimal" or "decimal?" => "GetDecimal",
            "bool" or "bool?" => "GetBoolean",
            "string" => "GetString",
            "DateTime" or "DateTime?" => "GetDateTime",
            "DateTimeOffset" or "DateTimeOffset?" => "GetFieldValue<DateTimeOffset>",
            "DateOnly" or "DateOnly?" => "GetFieldValue<DateOnly>",
            "TimeOnly" or "TimeOnly?" => "GetFieldValue<TimeOnly>",
            "TimeSpan" or "TimeSpan?" => "GetTimeSpan",
            "Guid" or "Guid?" => "GetGuid",
            "byte[]" => "GetFieldValue<byte[]>",
            _ when csharpType.EndsWith("[]") => $"GetFieldValue<{csharpType}>",
            _ => $"GetFieldValue<{csharpType}>"
        };

    /// <summary>
    /// Получает namespace для специальных типов (если требуется)
    /// </summary>
    /// <param name="postgresType">Тип PostgreSQL</param>
    /// <returns>Namespace или null</returns>
    public static string? GetRequiredNamespace(string postgresType)
    {
        var baseType = ExtractBaseType(postgresType.Replace("[]", "").Trim());

        return baseType.ToLowerInvariant() switch
        {
            "inet" or "cidr" or "macaddr" or "macaddr8" => "System.Net",
            _ => null
        };
    }

    /// <summary>
    /// Проверяет, является ли тип массивом
    /// </summary>
    /// <param name="postgresType">Тип PostgreSQL</param>
    /// <returns>True, если массив</returns>
    public static bool IsArrayType(string postgresType) =>
        !string.IsNullOrWhiteSpace(postgresType) && postgresType.EndsWith("[]");

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
    private static bool IsValueType(string csharpType) =>
        csharpType switch
        {
            "string" => false,
            "byte[]" => false,
            "object" => false,
            _ when csharpType.EndsWith("[]") => false,
            _ => true
        };
}
