using Microsoft.CodeAnalysis;

namespace PgCs.Core.SchemaAnalyzer.Options;

/// <summary>
/// Настройки для анализа схемы базы данных
/// </summary>
public sealed record SchemaAnalysisOptions
{
    /// <summary>
    /// Схемы для включения (null = все схемы)
    /// </summary>
    public IReadOnlySet<string>? IncludedSchemas { get; init; }
    
    /// <summary>
    /// Схемы для исключения
    /// </summary>
    public IReadOnlySet<string>? ExcludedSchemas { get; init; }
    
    /// <summary>
    /// Regex паттерны для включения таблиц
    /// </summary>
    public IReadOnlyList<string>? IncludeTablePatterns { get; init; }
    
    /// <summary>
    /// Regex паттерны для исключения таблиц
    /// </summary>
    public IReadOnlyList<string>? ExcludeTablePatterns { get; init; }
    
    /// <summary>
    /// Regex паттерны для включения представлений
    /// </summary>
    public IReadOnlyList<string>? IncludeViewPatterns { get; init; }
    
    /// <summary>
    /// Regex паттерны для исключения представлений
    /// </summary>
    public IReadOnlyList<string>? ExcludeViewPatterns { get; init; }
    
    /// <summary>
    /// Типы для включения (null = все типы)
    /// </summary>
    public IReadOnlySet<TypeKind>? IncludedTypeKinds { get; init; }
    
    /// <summary>
    /// Какие объекты анализировать (пустой список = все объекты)
    /// </summary>
    public IReadOnlySet<SchemaObjectType>? ObjectsToAnalyze { get; init; }
    
    /// <summary>
    /// Исключить системные объекты (pg_catalog, information_schema и т.д.)
    /// </summary>
    public bool ExcludeSystemObjects { get; init; }
    
    /// <summary>
    /// Максимальная глубина анализа зависимостей (0 = не анализировать)
    /// </summary>
    public int DependencyDepth { get; init; }
    
    /// <summary>
    /// Настройки по умолчанию (все объекты, все схемы)
    /// </summary>
    public static SchemaAnalysisOptions Default => new();

    /// <summary>
    /// Типы объектов базы данных для анализа
    /// </summary>
    public enum SchemaObjectType
    {
        /// <summary>
        /// Не анализировать объекты
        /// </summary>
        None,
    
        /// <summary>
        /// Таблицы
        /// </summary>
        Tables,
    
        /// <summary>
        /// Представления (VIEW)
        /// </summary>
        Views,
    
        /// <summary>
        /// Пользовательские типы данных
        /// </summary>
        Types,
    
        /// <summary>
        /// Функции и процедуры
        /// </summary>
        Functions,
    
        /// <summary>
        /// Индексы
        /// </summary>
        Indexes,
    
        /// <summary>
        /// Триггеры
        /// </summary>
        Triggers,
    
        /// <summary>
        /// Ограничения целостности (constraints)
        /// </summary>
        Constraints
    }
}