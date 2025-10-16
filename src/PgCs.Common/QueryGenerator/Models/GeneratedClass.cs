namespace PgCs.Common.QueryGenerator.Models;

/// <summary>
/// Сгенерированный класс с методами запросов
/// </summary>
public sealed record GeneratedClass
{
    /// <summary>
    /// Имя класса
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Сгенерированный C# код класса
    /// </summary>
    public required string SourceCode { get; init; }

    /// <summary>
    /// Путь к файлу (если уже сохранён)
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// Namespace класса
    /// </summary>
    public required string Namespace { get; init; }

    /// <summary>
    /// Методы, входящие в класс
    /// </summary>
    public required IReadOnlyList<GeneratedMethod> Methods { get; init; }

    /// <summary>
    /// Имя интерфейса (если генерируется)
    /// </summary>
    public string? InterfaceName { get; init; }

    /// <summary>
    /// Исходный код интерфейса (если генерируется)
    /// </summary>
    public string? InterfaceSourceCode { get; init; }

    /// <summary>
    /// Зависимости (другие классы/интерфейсы)
    /// </summary>
    public IReadOnlyList<string> Dependencies { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Документация класса
    /// </summary>
    public string? Documentation { get; init; }

    /// <summary>
    /// Размер кода в байтах
    /// </summary>
    public int SizeInBytes => System.Text.Encoding.UTF8.GetByteCount(SourceCode);
}

/// <summary>
/// Сгенерированный метод
/// </summary>
public sealed record GeneratedMethod
{
    /// <summary>
    /// Имя метода
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Сгенерированный C# код метода
    /// </summary>
    public required string SourceCode { get; init; }

    /// <summary>
    /// Тип возвращаемого значения
    /// </summary>
    public required string ReturnType { get; init; }

    /// <summary>
    /// Параметры метода
    /// </summary>
    public required IReadOnlyList<MethodParameter> Parameters { get; init; }

    /// <summary>
    /// Является асинхронным
    /// </summary>
    public bool IsAsync { get; init; }

    /// <summary>
    /// SQL запрос, который выполняет метод
    /// </summary>
    public required string SqlQuery { get; init; }

    /// <summary>
    /// Документация метода
    /// </summary>
    public string? Documentation { get; init; }

    /// <summary>
    /// Атрибуты метода
    /// </summary>
    public IReadOnlyList<string> Attributes { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Параметр метода
/// </summary>
public sealed record MethodParameter
{
    /// <summary>
    /// Имя параметра
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
    /// Значение по умолчанию
    /// </summary>
    public string? DefaultValue { get; init; }

    /// <summary>
    /// Документация параметра
    /// </summary>
    public string? Documentation { get; init; }

    /// <summary>
    /// Тип PostgreSQL
    /// </summary>
    public string? PostgresType { get; init; }
}

/// <summary>
/// Сгенерированная модель (для результатов или параметров)
/// </summary>
public sealed record GeneratedModel
{
    /// <summary>
    /// Имя модели
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
    /// Свойства модели
    /// </summary>
    public IReadOnlyList<ModelProperty> Properties { get; init; } = Array.Empty<ModelProperty>();

    /// <summary>
    /// Документация модели
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
    /// Модель результата запроса
    /// </summary>
    QueryResult,

    /// <summary>
    /// Модель параметров запроса
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
    /// Документация свойства
    /// </summary>
    public string? Documentation { get; init; }

    /// <summary>
    /// Исходная колонка из результата запроса
    /// </summary>
    public string? SourceColumnName { get; init; }

    /// <summary>
    /// Тип PostgreSQL
    /// </summary>
    public string? PostgresType { get; init; }
}
