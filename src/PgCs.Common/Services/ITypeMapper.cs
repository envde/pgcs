namespace PgCs.Common.Services;

/// <summary>
/// Маппер типов PostgreSQL в C# типы
/// </summary>
public interface ITypeMapper
{
    /// <summary>
    /// Преобразует тип PostgreSQL в соответствующий C# тип
    /// </summary>
    /// <param name="postgresType">Тип PostgreSQL (например: integer, text, uuid)</param>
    /// <param name="isNullable">Может ли значение быть null</param>
    /// <param name="isArray">Является ли тип массивом</param>
    /// <returns>Полное имя C# типа</returns>
    string MapType(string postgresType, bool isNullable, bool isArray);

    /// <summary>
    /// Получает имя using директивы для типа (если требуется)
    /// Например: для Guid требуется System
    /// </summary>
    string? GetRequiredNamespace(string postgresType);
}
