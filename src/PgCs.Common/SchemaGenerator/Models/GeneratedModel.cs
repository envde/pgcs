namespace PgCs.Common.SchemaGenerator.Models;

/// <summary>
/// Сгенерированная модель
/// </summary>
public sealed record GeneratedModel
{
    /// <summary>
    /// Имя модели (класса/record/enum)
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Сгенерированный C# код
    /// </summary>
    public required string SourceCode { get; init; }

    /// <summary>
    /// Путь к файлу (если уже сохранён)
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// Тип модели
    /// </summary>
    public ModelType ModelType { get; init; }

    /// <summary>
    /// Namespace модели
    /// </summary>
    public required string Namespace { get; init; }

    /// <summary>
    /// Исходное имя объекта базы данных
    /// </summary>
    public string? SourceObjectName { get; init; }

    /// <summary>
    /// Схема базы данных
    /// </summary>
    public string? SchemaName { get; init; }

    /// <summary>
    /// Зависимости (другие модели, которые используются в этой модели)
    /// </summary>
    public IReadOnlyList<string> Dependencies { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Свойства модели
    /// </summary>
    public IReadOnlyList<ModelProperty> Properties { get; init; } = Array.Empty<ModelProperty>();

    /// <summary>
    /// Комментарии и документация
    /// </summary>
    public string? Documentation { get; init; }

    /// <summary>
    /// Размер кода в байтах
    /// </summary>
    public int SizeInBytes => System.Text.Encoding.UTF8.GetByteCount(SourceCode);
}

/// <summary>
/// Тип модели
/// </summary>
public enum ModelType
{
    /// <summary>
    /// Модель таблицы
    /// </summary>
    Table,

    /// <summary>
    /// Модель представления
    /// </summary>
    View,

    /// <summary>
    /// Перечисление (ENUM)
    /// </summary>
    Enum,

    /// <summary>
    /// Пользовательский тип (DOMAIN, COMPOSITE)
    /// </summary>
    CustomType,

    /// <summary>
    /// Параметры функции/процедуры
    /// </summary>
    FunctionParameters,

    /// <summary>
    /// Результат запроса
    /// </summary>
    QueryResult,

    /// <summary>
    /// Параметры запроса
    /// </summary>
    QueryParameters
}

/// <summary>
/// Свойство модели
/// </summary>
public sealed record ModelProperty
{
    /// <summary>
    /// Имя свойства
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Тип C#
    /// </summary>
    public required string CSharpType { get; init; }

    /// <summary>
    /// Является nullable
    /// </summary>
    public bool IsNullable { get; init; }

    /// <summary>
    /// Является обязательным (required)
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Значение по умолчанию
    /// </summary>
    public string? DefaultValue { get; init; }

    /// <summary>
    /// Документация свойства
    /// </summary>
    public string? Documentation { get; init; }

    /// <summary>
    /// Атрибуты свойства
    /// </summary>
    public IReadOnlyList<string> Attributes { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Исходная колонка базы данных
    /// </summary>
    public string? SourceColumnName { get; init; }

    /// <summary>
    /// Тип PostgreSQL
    /// </summary>
    public string? PostgresType { get; init; }
}
