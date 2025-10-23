using PgCs.Common.Services;
using PgCs.Common.Writer;

namespace PgCs.Common.CodeGeneration;

/// <summary>
/// Fluent API factory для создания генераторов с кастомными зависимостями
/// Упрощает конфигурацию и внедрение зависимостей для SchemaGenerator и QueryGenerator
/// </summary>
public sealed class GeneratorFactory
{
    private ITypeMapper? _typeMapper;
    private INameConverter? _nameConverter;
    private IRoslynFormatter? _formatter;
    private IWriter? _writer;
    private bool _useDefaults = true;

    private GeneratorFactory()
    {
    }

    /// <summary>
    /// Создаёт новый экземпляр GeneratorFactory
    /// </summary>
    public static GeneratorFactory Create() => new();

    #region Type Mapper Configuration

    /// <summary>
    /// Использовать кастомный type mapper
    /// </summary>
    /// <param name="typeMapper">Кастомная реализация ITypeMapper</param>
    /// <example>
    /// factory.WithTypeMapper(TypeMapperBuilder.Create()
    ///     .UseSystemTextJson()
    ///     .UseNodaTime()
    ///     .Build())
    /// </example>
    public GeneratorFactory WithTypeMapper(ITypeMapper typeMapper)
    {
        ArgumentNullException.ThrowIfNull(typeMapper);

        _typeMapper = typeMapper;
        return this;
    }

    /// <summary>
    /// Настроить type mapper используя builder
    /// </summary>
    /// <param name="configure">Функция конфигурации TypeMapperBuilder</param>
    /// <example>
    /// factory.WithTypeMapper(builder => builder
    ///     .UseSystemTextJson()
    ///     .MapType("uuid", "Guid")
    ///     .AddNamespace("uuid", "System"))
    /// </example>
    public GeneratorFactory WithTypeMapper(Func<TypeMapperBuilder, TypeMapperBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = TypeMapperBuilder.Create();
        var configured = configure(builder);
        _typeMapper = configured.Build();
        return this;
    }

    /// <summary>
    /// Использовать стандартный type mapper
    /// </summary>
    public GeneratorFactory WithDefaultTypeMapper()
    {
        _typeMapper = new PostgreSqlTypeMapper();
        return this;
    }

    #endregion

    #region Name Converter Configuration

    /// <summary>
    /// Использовать кастомный name converter
    /// </summary>
    /// <param name="nameConverter">Кастомная реализация INameConverter</param>
    /// <example>
    /// factory.WithNameConverter(NameConversionStrategyBuilder.Create()
    ///     .UsePascalCaseForClasses()
    ///     .RemovePrefix("tbl_")
    ///     .Build())
    /// </example>
    public GeneratorFactory WithNameConverter(INameConverter nameConverter)
    {
        ArgumentNullException.ThrowIfNull(nameConverter);

        _nameConverter = nameConverter;
        return this;
    }

    /// <summary>
    /// Настроить name converter используя builder
    /// </summary>
    /// <param name="configure">Функция конфигурации NameConversionStrategyBuilder</param>
    /// <example>
    /// factory.WithNameConverter(builder => builder
    ///     .UseStandardCSharpConventions()
    ///     .RemovePrefix("tbl_", "v_")
    ///     .SingularizeClassNames())
    /// </example>
    public GeneratorFactory WithNameConverter(Func<NameConversionStrategyBuilder, NameConversionStrategyBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = NameConversionStrategyBuilder.Create();
        var configured = configure(builder);
        _nameConverter = configured.Build();
        return this;
    }

    /// <summary>
    /// Использовать стандартный name converter
    /// </summary>
    public GeneratorFactory WithDefaultNameConverter()
    {
        _nameConverter = new NameConverter();
        return this;
    }

    #endregion

    #region Formatter Configuration

    /// <summary>
    /// Использовать кастомный formatter
    /// </summary>
    /// <param name="formatter">Кастомная реализация IRoslynFormatter</param>
    public GeneratorFactory WithFormatter(IRoslynFormatter formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);

        _formatter = formatter;
        return this;
    }

    /// <summary>
    /// Использовать стандартный Roslyn formatter
    /// </summary>
    public GeneratorFactory WithDefaultFormatter()
    {
        _formatter = new RoslynFormatter();
        return this;
    }

    /// <summary>
    /// Отключить форматирование (для faster builds)
    /// </summary>
    public GeneratorFactory WithoutFormatter()
    {
        _formatter = new NoOpFormatter();
        return this;
    }

    #endregion

    #region Writer Configuration

    /// <summary>
    /// Использовать кастомный writer
    /// </summary>
    /// <param name="writer">Кастомная реализация IWriter</param>
    public GeneratorFactory WithWriter(IWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        _writer = writer;
        return this;
    }

    /// <summary>
    /// Использовать стандартный file writer (должен быть передан извне)
    /// </summary>
    public GeneratorFactory WithDefaultWriter()
    {
        // Будет установлен в null, пользователь должен передать writer
        _writer = null;
        return this;
    }

    #endregion

    #region Default Strategy

    /// <summary>
    /// Не использовать defaults - требовать явную конфигурацию всех зависимостей
    /// </summary>
    public GeneratorFactory WithoutDefaults()
    {
        _useDefaults = false;
        return this;
    }

    /// <summary>
    /// Использовать defaults для неуказанных зависимостей (по умолчанию включено)
    /// </summary>
    public GeneratorFactory WithDefaults()
    {
        _useDefaults = true;
        return this;
    }

    #endregion

    #region Quick Presets

    /// <summary>
    /// Preset: Конфигурация для использования System.Text.Json
    /// </summary>
    public GeneratorFactory UseSystemTextJsonPreset()
    {
        return WithTypeMapper(builder => builder.UseSystemTextJson())
               .WithNameConverter(builder => builder.UseStandardCSharpConventions())
               .WithDefaultFormatter();
    }

    /// <summary>
    /// Preset: Конфигурация для использования NodaTime
    /// </summary>
    public GeneratorFactory UseNodaTimePreset()
    {
        return WithTypeMapper(builder => builder.UseNodaTime())
               .WithNameConverter(builder => builder.UseStandardCSharpConventions())
               .WithDefaultFormatter();
    }

    /// <summary>
    /// Preset: Конфигурация для PostGIS / NetTopologySuite
    /// </summary>
    public GeneratorFactory UseNetTopologySuitePreset()
    {
        return WithTypeMapper(builder => builder.UseNetTopologySuite())
               .WithNameConverter(builder => builder.UseStandardCSharpConventions())
               .WithDefaultFormatter();
    }

    /// <summary>
    /// Preset: Минималистичная конфигурация (без форматирования, стандартные маппинги)
    /// </summary>
    public GeneratorFactory UseMinimalPreset()
    {
        return WithDefaultTypeMapper()
               .WithDefaultNameConverter()
               .WithoutFormatter();
    }

    /// <summary>
    /// Preset: Максимальная кастомизация (удалить префиксы, System.Text.Json, форматирование)
    /// </summary>
    public GeneratorFactory UseCustomizationPreset()
    {
        return WithTypeMapper(builder => builder
                   .UseSystemTextJson()
                   .MapType("uuid", "Guid")
                   .AddNamespace("uuid", "System"))
               .WithNameConverter(builder => builder
                   .UseStandardCSharpConventions()
                   .RemoveStandardTablePrefixes()
                   .RemoveStandardViewPrefixes())
               .WithDefaultFormatter();
    }

    #endregion

    #region Build Methods

    /// <summary>
    /// Получить зависимости для ручного создания SchemaGenerator
    /// </summary>
    /// <returns>Tuple с зависимостями (TypeMapper, NameConverter, Formatter)</returns>
    public (ITypeMapper TypeMapper, INameConverter NameConverter, IRoslynFormatter Formatter) GetDependencies()
    {
        EnsureDependencies();

        return (_typeMapper!, _nameConverter!, _formatter!);
    }

    /// <summary>
    /// Получить IWriter (если настроен)
    /// </summary>
    /// <returns>IWriter или null</returns>
    public IWriter? GetWriter()
    {
        return _writer;
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Убеждается что все зависимости настроены (или использует defaults)
    /// </summary>
    private void EnsureDependencies()
    {
        if (_useDefaults)
        {
            _typeMapper ??= new PostgreSqlTypeMapper();
            _nameConverter ??= new NameConverter();
            _formatter ??= new RoslynFormatter();
            // Writer остаётся null - должен быть передан извне
        }
        else
        {
            if (_typeMapper == null)
                throw new InvalidOperationException("TypeMapper not configured. Call WithTypeMapper() or enable defaults.");
            if (_nameConverter == null)
                throw new InvalidOperationException("NameConverter not configured. Call WithNameConverter() or enable defaults.");
            if (_formatter == null)
                throw new InvalidOperationException("Formatter not configured. Call WithFormatter() or enable defaults.");
        }
    }

    #endregion

    #region No-Op Implementations

    /// <summary>
    /// No-op formatter (не выполняет форматирование)
    /// </summary>
    private sealed class NoOpFormatter : IRoslynFormatter
    {
        public string Format(string sourceCode) => sourceCode;
    }

    #endregion
}
