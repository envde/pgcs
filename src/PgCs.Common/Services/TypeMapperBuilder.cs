namespace PgCs.Common.Services;

/// <summary>
/// Fluent API builder для кастомизации маппинга PostgreSQL типов в C# типы
/// Позволяет переопределять стандартный маппинг и добавлять собственные правила
/// </summary>
public sealed class TypeMapperBuilder
{
    private readonly Dictionary<string, string> _customTypeMappings = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _customNamespaces = new(StringComparer.OrdinalIgnoreCase);
    private string _defaultTypeForUnknown = "object";
    private bool _useStandardMappings = true;

    private TypeMapperBuilder()
    {
    }

    /// <summary>
    /// Создаёт новый экземпляр TypeMapperBuilder
    /// </summary>
    public static TypeMapperBuilder Create() => new();

    /// <summary>
    /// Добавляет кастомный маппинг PostgreSQL типа в C# тип
    /// </summary>
    /// <param name="postgresType">PostgreSQL тип (например: 'uuid', 'jsonb', 'inet')</param>
    /// <param name="csharpType">C# тип (например: 'Guid', 'JsonDocument', 'IPAddress')</param>
    /// <example>
    /// builder.MapType("uuid", "Guid")
    ///        .MapType("jsonb", "JsonDocument")
    ///        .MapType("inet", "System.Net.IPAddress");
    /// </example>
    public TypeMapperBuilder MapType(string postgresType, string csharpType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(postgresType);
        ArgumentException.ThrowIfNullOrWhiteSpace(csharpType);

        _customTypeMappings[postgresType] = csharpType;
        return this;
    }

    /// <summary>
    /// Добавляет несколько маппингов одновременно
    /// </summary>
    /// <param name="mappings">Словарь PostgreSQL тип → C# тип</param>
    /// <example>
    /// builder.MapTypes(new Dictionary&lt;string, string&gt;
    /// {
    ///     ["uuid"] = "Guid",
    ///     ["jsonb"] = "JsonDocument",
    ///     ["inet"] = "IPAddress"
    /// });
    /// </example>
    public TypeMapperBuilder MapTypes(IDictionary<string, string> mappings)
    {
        ArgumentNullException.ThrowIfNull(mappings);

        foreach (var (postgresType, csharpType) in mappings)
        {
            MapType(postgresType, csharpType);
        }
        return this;
    }

    /// <summary>
    /// Добавляет namespace для конкретного PostgreSQL типа
    /// </summary>
    /// <param name="postgresType">PostgreSQL тип</param>
    /// <param name="namespace">Namespace для using директивы</param>
    /// <example>
    /// builder.AddNamespace("uuid", "System")
    ///        .AddNamespace("inet", "System.Net")
    ///        .AddNamespace("jsonb", "System.Text.Json");
    /// </example>
    public TypeMapperBuilder AddNamespace(string postgresType, string @namespace)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(postgresType);
        ArgumentException.ThrowIfNullOrWhiteSpace(@namespace);

        _customNamespaces[postgresType] = @namespace;
        return this;
    }

    /// <summary>
    /// Добавляет несколько namespaces одновременно
    /// </summary>
    /// <param name="namespaces">Словарь PostgreSQL тип → namespace</param>
    public TypeMapperBuilder AddNamespaces(IDictionary<string, string> namespaces)
    {
        ArgumentNullException.ThrowIfNull(namespaces);

        foreach (var (postgresType, @namespace) in namespaces)
        {
            AddNamespace(postgresType, @namespace);
        }
        return this;
    }

    /// <summary>
    /// Устанавливает тип по умолчанию для неизвестных PostgreSQL типов
    /// </summary>
    /// <param name="defaultType">C# тип (по умолчанию: "object")</param>
    /// <example>
    /// builder.WithDefaultTypeForUnknown("string") // Все неизвестные типы → string
    ///        .WithDefaultTypeForUnknown("dynamic") // Все неизвестные типы → dynamic
    /// </example>
    public TypeMapperBuilder WithDefaultTypeForUnknown(string defaultType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultType);

        _defaultTypeForUnknown = defaultType;
        return this;
    }

    /// <summary>
    /// Очищает стандартные маппинги (использовать только кастомные)
    /// </summary>
    /// <example>
    /// // Использовать ТОЛЬКО кастомные маппинги, игнорируя стандартные
    /// builder.ClearStandardMappings()
    ///        .MapType("uuid", "Guid")
    ///        .MapType("text", "string");
    /// </example>
    public TypeMapperBuilder ClearStandardMappings()
    {
        _useStandardMappings = false;
        return this;
    }

    /// <summary>
    /// Включает использование стандартных маппингов (по умолчанию включено)
    /// </summary>
    public TypeMapperBuilder UseStandardMappings()
    {
        _useStandardMappings = true;
        return this;
    }

    /// <summary>
    /// Быстрая настройка для JSON типов с использованием System.Text.Json
    /// </summary>
    /// <example>
    /// builder.UseSystemTextJson(); // jsonb → JsonDocument, json → JsonDocument
    /// </example>
    public TypeMapperBuilder UseSystemTextJson()
    {
        MapType("json", "JsonDocument");
        MapType("jsonb", "JsonDocument");
        AddNamespace("json", "System.Text.Json");
        AddNamespace("jsonb", "System.Text.Json");
        return this;
    }

    /// <summary>
    /// Быстрая настройка для JSON типов с использованием Newtonsoft.Json
    /// </summary>
    /// <example>
    /// builder.UseNewtonsoftJson(); // jsonb → JObject, json → JObject
    /// </example>
    public TypeMapperBuilder UseNewtonsoftJson()
    {
        MapType("json", "JObject");
        MapType("jsonb", "JObject");
        AddNamespace("json", "Newtonsoft.Json.Linq");
        AddNamespace("jsonb", "Newtonsoft.Json.Linq");
        return this;
    }

    /// <summary>
    /// Быстрая настройка для JSON типов как строк
    /// </summary>
    /// <example>
    /// builder.UseStringForJson(); // jsonb → string, json → string
    /// </example>
    public TypeMapperBuilder UseStringForJson()
    {
        MapType("json", "string");
        MapType("jsonb", "string");
        return this;
    }

    /// <summary>
    /// Быстрая настройка для использования NodaTime для дат и времени
    /// </summary>
    /// <example>
    /// builder.UseNodaTime(); // timestamp → Instant, date → LocalDate, etc.
    /// </example>
    public TypeMapperBuilder UseNodaTime()
    {
        MapType("timestamp", "Instant");
        MapType("timestamp without time zone", "Instant");
        MapType("timestamp with time zone", "Instant");
        MapType("timestamptz", "Instant");
        MapType("date", "LocalDate");
        MapType("time", "LocalTime");
        MapType("time without time zone", "LocalTime");
        MapType("interval", "Period");

        AddNamespace("timestamp", "NodaTime");
        AddNamespace("date", "NodaTime");
        AddNamespace("time", "NodaTime");
        AddNamespace("interval", "NodaTime");
        return this;
    }

    /// <summary>
    /// Быстрая настройка для PostGIS типов
    /// </summary>
    /// <example>
    /// builder.UseNetTopologySuite(); // geometry → Geometry, geography → Geometry
    /// </example>
    public TypeMapperBuilder UseNetTopologySuite()
    {
        MapType("geometry", "Geometry");
        MapType("geography", "Geometry");
        MapType("point", "Point");
        MapType("polygon", "Polygon");
        MapType("linestring", "LineString");
        MapType("multipoint", "MultiPoint");
        MapType("multipolygon", "MultiPolygon");
        MapType("multilinestring", "MultiLineString");

        var ns = "NetTopologySuite.Geometries";
        AddNamespace("geometry", ns);
        AddNamespace("geography", ns);
        AddNamespace("point", ns);
        AddNamespace("polygon", ns);
        AddNamespace("linestring", ns);
        AddNamespace("multipoint", ns);
        AddNamespace("multipolygon", ns);
        AddNamespace("multilinestring", ns);
        return this;
    }

    /// <summary>
    /// Создаёт экземпляр CustomTypeMapper с настройками
    /// </summary>
    /// <returns>Сконфигурированный ITypeMapper</returns>
    public ITypeMapper Build()
    {
        return new CustomTypeMapper(
            _customTypeMappings,
            _customNamespaces,
            _defaultTypeForUnknown,
            _useStandardMappings);
    }

    /// <summary>
    /// Внутренняя реализация кастомного маппера типов
    /// </summary>
    private sealed class CustomTypeMapper : ITypeMapper
    {
        private readonly Dictionary<string, string> _customTypeMappings;
        private readonly Dictionary<string, string> _customNamespaces;
        private readonly string _defaultTypeForUnknown;
        private readonly bool _useStandardMappings;
        private readonly PostgreSqlTypeMapper _standardMapper;

        public CustomTypeMapper(
            Dictionary<string, string> customTypeMappings,
            Dictionary<string, string> customNamespaces,
            string defaultTypeForUnknown,
            bool useStandardMappings)
        {
            _customTypeMappings = customTypeMappings;
            _customNamespaces = customNamespaces;
            _defaultTypeForUnknown = defaultTypeForUnknown;
            _useStandardMappings = useStandardMappings;
            _standardMapper = new PostgreSqlTypeMapper();
        }

        public string MapType(string postgresType, bool isNullable, bool isArray)
        {
            var cleanType = CleanTypeName(postgresType);

            // Сначала проверяем кастомные маппинги
            if (_customTypeMappings.TryGetValue(cleanType, out var customType))
            {
                return ApplyModifiers(customType, isNullable, isArray);
            }

            // Если используем стандартные маппинги, пробуем их
            if (_useStandardMappings)
            {
                var standardType = _standardMapper.MapType(postgresType, isNullable: false, isArray: false);
                if (standardType != "object")
                {
                    return ApplyModifiers(standardType, isNullable, isArray);
                }
            }

            // Fallback на тип по умолчанию
            return ApplyModifiers(_defaultTypeForUnknown, isNullable, isArray);
        }

        public string? GetRequiredNamespace(string postgresType)
        {
            var cleanType = CleanTypeName(postgresType);

            // Сначала проверяем кастомные namespaces
            if (_customNamespaces.TryGetValue(cleanType, out var customNamespace))
            {
                return customNamespace;
            }

            // Fallback на стандартный маппер
            if (_useStandardMappings)
            {
                return _standardMapper.GetRequiredNamespace(postgresType);
            }

            return null;
        }

        private static string ApplyModifiers(string csharpType, bool isNullable, bool isArray)
        {
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

        private static string CleanTypeName(string postgresType)
        {
            var parenIndex = postgresType.IndexOf('(');
            return parenIndex > 0
                ? postgresType[..parenIndex].Trim()
                : postgresType.Trim();
        }

        private static bool IsValueType(string csharpType)
        {
            return csharpType switch
            {
                "string" => false,
                "object" => false,
                "dynamic" => false,
                "byte[]" => false,
                _ when csharpType.Contains("[]") => false,
                _ when csharpType.Contains('.') && !csharpType.StartsWith("System.") => false,
                "Guid" => true,
                "DateTime" => true,
                "DateTimeOffset" => true,
                "DateOnly" => true,
                "TimeOnly" => true,
                "TimeSpan" => true,
                "Instant" => true,
                "LocalDate" => true,
                "LocalTime" => true,
                "Period" => true,
                _ => true
            };
        }
    }
}
