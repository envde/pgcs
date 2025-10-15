namespace PgCs.Common.CodeGeneration.Query.Models;

/// <summary>
/// Настройки именования методов
/// </summary>
public sealed record MethodNamingOptions
{
    /// <summary>
    /// Суффикс для асинхронных методов
    /// </summary>
    public string AsyncSuffix { get; init; } = "Async";

    /// <summary>
    /// Префикс для методов выборки SELECT
    /// </summary>
    public string SelectPrefix { get; init; } = "Get";

    /// <summary>
    /// Префикс для методов вставки INSERT
    /// </summary>
    public string InsertPrefix { get; init; } = "Create";

    /// <summary>
    /// Префикс для методов обновления UPDATE
    /// </summary>
    public string UpdatePrefix { get; init; } = "Update";

    /// <summary>
    /// Префикс для методов удаления DELETE
    /// </summary>
    public string DeletePrefix { get; init; } = "Delete";

    /// <summary>
    /// Использовать имя из аннотации как есть (без префиксов)
    /// </summary>
    public bool UseAnnotationNameAsIs { get; init; } = true;

    /// <summary>
    /// Переопределения имен конкретных методов
    /// </summary>
    public IReadOnlyDictionary<string, string> MethodNameOverrides { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    /// Преобразовать snake_case имена в PascalCase
    /// </summary>
    public bool ConvertSnakeCaseToPascalCase { get; init; } = true;
}