namespace PgCs.Common.CodeGeneration.Models.Enums;

/// <summary>
/// Режим генерации файлов
/// </summary>
public enum FileGenerationMode
{
    /// <summary>
    /// Один файл на таблицу/модель
    /// </summary>
    OnePerTable,

    /// <summary>
    /// Все модели в одном файле
    /// </summary>
    SingleFile,

    /// <summary>
    /// Группировать по схемам БД
    /// </summary>
    GroupBySchema,

    /// <summary>
    /// Группировать по типам (таблицы, представления, енумы отдельно)
    /// </summary>
    GroupByType
}