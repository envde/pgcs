using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Humanizer;
using PgCs.Common.Utils;

namespace PgCs.Common.Services;

/// <summary>
/// Fluent API builder для кастомизации правил конвертации имён
/// Позволяет настраивать преобразование имён из PostgreSQL конвенций в C# конвенции
/// </summary>
public sealed class NameConversionStrategyBuilder
{
    private readonly List<Func<string, string>> _transformations = [];
    private readonly List<(Regex Pattern, string Replacement)> _customRules = [];
    private readonly HashSet<string> _prefixesToRemove = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _suffixesToRemove = new(StringComparer.OrdinalIgnoreCase);
    private CaseStyle _classNameCase = CaseStyle.PascalCase;
    private CaseStyle _propertyNameCase = CaseStyle.PascalCase;
    private CaseStyle _methodNameCase = CaseStyle.PascalCase;
    private CaseStyle _parameterNameCase = CaseStyle.CamelCase;
    private bool _singularizeClassNames = true;
    private bool _pluralizeCollections = true;
    private CultureInfo _humanizerCulture = CultureInfo.InvariantCulture;

    private NameConversionStrategyBuilder()
    {
    }

    /// <summary>
    /// Создаёт новый экземпляр NameConversionStrategyBuilder
    /// </summary>
    public static NameConversionStrategyBuilder Create() => new();

    #region Case Style Configuration

    /// <summary>
    /// Использовать PascalCase для имён классов (по умолчанию)
    /// </summary>
    /// <example>user_profile → UserProfile</example>
    public NameConversionStrategyBuilder UsePascalCaseForClasses()
    {
        _classNameCase = CaseStyle.PascalCase;
        return this;
    }

    /// <summary>
    /// Использовать camelCase для имён классов
    /// </summary>
    /// <example>user_profile → userProfile</example>
    public NameConversionStrategyBuilder UseCamelCaseForClasses()
    {
        _classNameCase = CaseStyle.CamelCase;
        return this;
    }

    /// <summary>
    /// Использовать PascalCase для имён свойств (по умолчанию)
    /// </summary>
    /// <example>user_name → UserName</example>
    public NameConversionStrategyBuilder UsePascalCaseForProperties()
    {
        _propertyNameCase = CaseStyle.PascalCase;
        return this;
    }

    /// <summary>
    /// Использовать camelCase для имён свойств
    /// </summary>
    /// <example>user_name → userName</example>
    public NameConversionStrategyBuilder UseCamelCaseForProperties()
    {
        _propertyNameCase = CaseStyle.CamelCase;
        return this;
    }

    /// <summary>
    /// Использовать PascalCase для имён методов (по умолчанию)
    /// </summary>
    /// <example>get_user_by_id → GetUserById</example>
    public NameConversionStrategyBuilder UsePascalCaseForMethods()
    {
        _methodNameCase = CaseStyle.PascalCase;
        return this;
    }

    /// <summary>
    /// Использовать camelCase для имён методов
    /// </summary>
    /// <example>get_user_by_id → getUserById</example>
    public NameConversionStrategyBuilder UseCamelCaseForMethods()
    {
        _methodNameCase = CaseStyle.CamelCase;
        return this;
    }

    /// <summary>
    /// Использовать camelCase для имён параметров (по умолчанию)
    /// </summary>
    /// <example>user_id → userId</example>
    public NameConversionStrategyBuilder UseCamelCaseForParameters()
    {
        _parameterNameCase = CaseStyle.CamelCase;
        return this;
    }

    /// <summary>
    /// Использовать PascalCase для имён параметров
    /// </summary>
    /// <example>user_id → UserId</example>
    public NameConversionStrategyBuilder UsePascalCaseForParameters()
    {
        _parameterNameCase = CaseStyle.PascalCase;
        return this;
    }

    #endregion

    #region Singularization / Pluralization

    /// <summary>
    /// Преобразовывать имена таблиц в единственное число (по умолчанию включено)
    /// </summary>
    /// <example>users → User, categories → Category</example>
    public NameConversionStrategyBuilder SingularizeClassNames()
    {
        _singularizeClassNames = true;
        return this;
    }

    /// <summary>
    /// НЕ преобразовывать имена таблиц в единственное число
    /// </summary>
    /// <example>users → Users, categories → Categories</example>
    public NameConversionStrategyBuilder DoNotSingularizeClassNames()
    {
        _singularizeClassNames = false;
        return this;
    }

    /// <summary>
    /// Преобразовывать имена коллекций во множественное число (по умолчанию включено)
    /// </summary>
    /// <example>User → Users, Category → Categories</example>
    public NameConversionStrategyBuilder PluralizeCollections()
    {
        _pluralizeCollections = true;
        return this;
    }

    /// <summary>
    /// НЕ преобразовывать имена коллекций во множественное число
    /// </summary>
    public NameConversionStrategyBuilder DoNotPluralizeCollections()
    {
        _pluralizeCollections = false;
        return this;
    }

    #endregion

    #region Prefix / Suffix Removal

    /// <summary>
    /// Удалить указанные префиксы из имён
    /// </summary>
    /// <param name="prefixes">Префиксы для удаления</param>
    /// <example>
    /// builder.RemovePrefix("tbl_", "v_", "sp_")
    ///        // tbl_users → Users, v_active_users → ActiveUsers
    /// </example>
    public NameConversionStrategyBuilder RemovePrefix(params string[] prefixes)
    {
        foreach (var prefix in prefixes)
        {
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                _prefixesToRemove.Add(prefix);
            }
        }
        return this;
    }

    /// <summary>
    /// Удалить указанные суффиксы из имён
    /// </summary>
    /// <param name="suffixes">Суффиксы для удаления</param>
    /// <example>
    /// builder.RemoveSuffix("_table", "_view", "_tmp")
    ///        // users_table → Users, orders_tmp → Orders
    /// </example>
    public NameConversionStrategyBuilder RemoveSuffix(params string[] suffixes)
    {
        foreach (var suffix in suffixes)
        {
            if (!string.IsNullOrWhiteSpace(suffix))
            {
                _suffixesToRemove.Add(suffix);
            }
        }
        return this;
    }

    #endregion

    #region Custom Rules

    /// <summary>
    /// Добавить кастомное правило замены с использованием regex
    /// </summary>
    /// <param name="pattern">Regex паттерн</param>
    /// <param name="replacement">Строка замены</param>
    /// <example>
    /// builder.AddCustomRule(@"^usr_", "User") // usr_profile → UserProfile
    ///        .AddCustomRule(@"_id$", "Id")    // user_id → UserId
    /// </example>
    public NameConversionStrategyBuilder AddCustomRule(string pattern, string replacement)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);
        ArgumentNullException.ThrowIfNull(replacement);

        _customRules.Add((new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled), replacement));
        return this;
    }

    /// <summary>
    /// Добавить кастомную функцию преобразования
    /// </summary>
    /// <param name="transformation">Функция преобразования</param>
    /// <example>
    /// builder.AddTransformation(name => name.Replace("Usr", "User"))
    ///        .AddTransformation(name => name.StartsWith("Old") ? name[3..] : name)
    /// </example>
    public NameConversionStrategyBuilder AddTransformation(Func<string, string> transformation)
    {
        ArgumentNullException.ThrowIfNull(transformation);

        _transformations.Add(transformation);
        return this;
    }

    #endregion

    #region Humanizer Configuration

    /// <summary>
    /// Установить культуру для Humanizer (для корректной плюрализации/сингуляризации)
    /// </summary>
    /// <param name="culture">Культура (например: "en-US", "ru-RU")</param>
    /// <example>
    /// builder.WithHumanizerCulture("en-US") // категории → категория (English rules)
    ///        .WithHumanizerCulture("ru-RU") // категории → категория (Russian rules)
    /// </example>
    public NameConversionStrategyBuilder WithHumanizerCulture(string culture)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(culture);

        _humanizerCulture = new CultureInfo(culture);
        return this;
    }

    /// <summary>
    /// Использовать инвариантную культуру для Humanizer (по умолчанию)
    /// </summary>
    public NameConversionStrategyBuilder WithInvariantCulture()
    {
        _humanizerCulture = CultureInfo.InvariantCulture;
        return this;
    }

    #endregion

    #region Quick Presets

    /// <summary>
    /// Preset: Стандартные C# конвенции (PascalCase классы/свойства, camelCase параметры)
    /// </summary>
    public NameConversionStrategyBuilder UseStandardCSharpConventions()
    {
        UsePascalCaseForClasses();
        UsePascalCaseForProperties();
        UsePascalCaseForMethods();
        UseCamelCaseForParameters();
        SingularizeClassNames();
        PluralizeCollections();
        return this;
    }

    /// <summary>
    /// Preset: Все в camelCase (для JSON serialization)
    /// </summary>
    public NameConversionStrategyBuilder UseCamelCaseEverywhere()
    {
        UseCamelCaseForClasses();
        UseCamelCaseForProperties();
        UseCamelCaseForMethods();
        UseCamelCaseForParameters();
        return this;
    }

    /// <summary>
    /// Preset: Все в PascalCase
    /// </summary>
    public NameConversionStrategyBuilder UsePascalCaseEverywhere()
    {
        UsePascalCaseForClasses();
        UsePascalCaseForProperties();
        UsePascalCaseForMethods();
        UsePascalCaseForParameters();
        return this;
    }

    /// <summary>
    /// Preset: Удалить стандартные префиксы таблиц
    /// </summary>
    public NameConversionStrategyBuilder RemoveStandardTablePrefixes()
    {
        RemovePrefix("tbl_", "table_", "t_");
        return this;
    }

    /// <summary>
    /// Preset: Удалить стандартные префиксы представлений
    /// </summary>
    public NameConversionStrategyBuilder RemoveStandardViewPrefixes()
    {
        RemovePrefix("v_", "view_", "vw_");
        return this;
    }

    /// <summary>
    /// Preset: Удалить стандартные префиксы хранимых процедур
    /// </summary>
    public NameConversionStrategyBuilder RemoveStandardProcedurePrefixes()
    {
        RemovePrefix("sp_", "proc_", "fn_", "udf_");
        return this;
    }

    #endregion

    /// <summary>
    /// Создаёт экземпляр INameConverter с настроенными правилами
    /// </summary>
    public INameConverter Build()
    {
        return new CustomNameConverter(
            _prefixesToRemove,
            _suffixesToRemove,
            _customRules,
            _transformations,
            _classNameCase,
            _propertyNameCase,
            _methodNameCase,
            _parameterNameCase,
            _singularizeClassNames,
            _pluralizeCollections,
            _humanizerCulture);
    }

    #region Internal Enums

    private enum CaseStyle
    {
        PascalCase,
        CamelCase
    }

    #endregion

    #region Internal Implementation

    /// <summary>
    /// Внутренняя реализация кастомного конвертера имён
    /// </summary>
    private sealed class CustomNameConverter : INameConverter
    {
        private readonly HashSet<string> _prefixesToRemove;
        private readonly HashSet<string> _suffixesToRemove;
        private readonly List<(Regex Pattern, string Replacement)> _customRules;
        private readonly List<Func<string, string>> _transformations;
        private readonly CaseStyle _classNameCase;
        private readonly CaseStyle _propertyNameCase;
        private readonly CaseStyle _methodNameCase;
        private readonly CaseStyle _parameterNameCase;
        private readonly bool _singularizeClassNames;
        private readonly bool _pluralizeCollections;
        private readonly CultureInfo _humanizerCulture;

        public CustomNameConverter(
            HashSet<string> prefixesToRemove,
            HashSet<string> suffixesToRemove,
            List<(Regex Pattern, string Replacement)> customRules,
            List<Func<string, string>> transformations,
            CaseStyle classNameCase,
            CaseStyle propertyNameCase,
            CaseStyle methodNameCase,
            CaseStyle parameterNameCase,
            bool singularizeClassNames,
            bool pluralizeCollections,
            CultureInfo humanizerCulture)
        {
            _prefixesToRemove = prefixesToRemove;
            _suffixesToRemove = suffixesToRemove;
            _customRules = customRules;
            _transformations = transformations;
            _classNameCase = classNameCase;
            _propertyNameCase = propertyNameCase;
            _methodNameCase = methodNameCase;
            _parameterNameCase = parameterNameCase;
            _singularizeClassNames = singularizeClassNames;
            _pluralizeCollections = pluralizeCollections;
            _humanizerCulture = humanizerCulture;
        }

        public string ToClassName(string tableName)
        {
            var name = ApplyCommonTransformations(tableName);
            name = ApplyCase(name, _classNameCase);

            if (_singularizeClassNames)
            {
                name = name.Singularize(inputIsKnownToBePlural: false);
            }

            return name;
        }

        public string ToPropertyName(string columnName)
        {
            var name = ApplyCommonTransformations(columnName);
            return ApplyCase(name, _propertyNameCase);
        }

        public string ToEnumMemberName(string enumValue)
        {
            var name = ApplyCommonTransformations(enumValue);
            return ApplyCase(name, CaseStyle.PascalCase); // Enum members всегда PascalCase
        }

        public string ToMethodName(string functionName)
        {
            var name = ApplyCommonTransformations(functionName);
            return ApplyCase(name, _methodNameCase);
        }

        public string ToParameterName(string parameterName)
        {
            var name = ApplyCommonTransformations(parameterName);
            return ApplyCase(name, _parameterNameCase);
        }

        private string ApplyCommonTransformations(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var result = input;

            // 1. Удаляем префиксы
            foreach (var prefix in _prefixesToRemove)
            {
                if (result.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    result = result[prefix.Length..];
                    break; // Удаляем только первый найденный префикс
                }
            }

            // 2. Удаляем суффиксы
            foreach (var suffix in _suffixesToRemove)
            {
                if (result.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    result = result[..^suffix.Length];
                    break; // Удаляем только первый найденный суффикс
                }
            }

            // 3. Применяем кастомные regex правила
            foreach (var (pattern, replacement) in _customRules)
            {
                result = pattern.Replace(result, replacement);
            }

            // 4. Применяем кастомные функции преобразования
            foreach (var transformation in _transformations)
            {
                result = transformation(result);
            }

            return result;
        }

        private static string ApplyCase(string input, CaseStyle caseStyle)
        {
            return caseStyle switch
            {
                CaseStyle.PascalCase => CaseConverter.ToPascalCase(input),
                CaseStyle.CamelCase => CaseConverter.ToCamelCase(input),
                _ => input
            };
        }
    }

    #endregion
}
