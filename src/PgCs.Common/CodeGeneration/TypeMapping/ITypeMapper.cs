using PgCs.Common.CodeGeneration.Schema.Models;

namespace PgCs.Common.CodeGeneration.TypeMapping;

/// <summary>
/// Маппер типов PostgreSQL -> C#
/// </summary>
public interface ITypeMapper
{
    /// <summary>
    /// Получает C# тип для PostgreSQL типа
    /// </summary>
    /// <param name="postgresType">PostgreSQL тип</param>
    /// <param name="isNullable">Может ли быть null</param>
    /// <param name="isArray">Является ли массивом</param>
    /// <param name="options">Настройки маппинга</param>
    /// <returns>C# тип</returns>
    string GetCSharpType(
        string postgresType,
        bool isNullable,
        bool isArray = false,
        TypeMappingOptions? options = null);

    /// <summary>
    /// Получает NpgsqlDbType для PostgreSQL типа
    /// </summary>
    /// <param name="postgresType">PostgreSQL тип</param>
    /// <returns>Имя NpgsqlDbType</returns>
    string GetNpgsqlDbType(string postgresType);

    /// <summary>
    /// Проверяет, является ли тип пользовательским (ENUM, COMPOSITE, DOMAIN)
    /// </summary>
    /// <param name="postgresType">PostgreSQL тип</param>
    /// <returns>True если пользовательский тип</returns>
    bool IsCustomType(string postgresType);

    /// <summary>
    /// Получает значение по умолчанию для C# типа
    /// </summary>
    /// <param name="csharpType">C# тип</param>
    /// <param name="isNullable">Nullable тип</param>
    /// <returns>Значение по умолчанию или null</returns>
    string? GetDefaultValue(string csharpType, bool isNullable);

    /// <summary>
    /// Регистрирует пользовательский маппинг типов
    /// </summary>
    /// <param name="postgresType">PostgreSQL тип</param>
    /// <param name="csharpType">C# тип</param>
    void RegisterCustomMapping(string postgresType, string csharpType);

    /// <summary>
    /// Получает using директивы необходимые для типа
    /// </summary>
    /// <param name="csharpType">C# тип</param>
    /// <returns>Список using директив</returns>
    IReadOnlyList<string> GetRequiredUsings(string csharpType);

    /// <summary>
    /// Проверяет, требуется ли для типа специальный конвертер
    /// </summary>
    /// <param name="postgresType">PostgreSQL тип</param>
    /// <returns>True если нужен конвертер</returns>
    bool RequiresConverter(string postgresType);
}