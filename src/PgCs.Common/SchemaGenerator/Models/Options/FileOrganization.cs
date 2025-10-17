namespace PgCs.Common.SchemaGenerator.Models.Options;

/// <summary>
/// Стратегия организации сгенерированных файлов
/// </summary>
public enum FileOrganization
{
    /// <summary>
    /// Все файлы в одной директории
    /// </summary>
    Flat,

    /// <summary>
    /// Группировка по типу объекта (Tables, Views, Types и т.д.)
    /// </summary>
    ByType,

    /// <summary>
    /// Группировка по схеме базы данных
    /// </summary>
    BySchema,

    /// <summary>
    /// Комбинированная: схема -> тип объекта
    /// </summary>
    BySchemaAndType
}
