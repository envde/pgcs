using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaGenerator.Models;

namespace PgCs.Common.SchemaGenerator;

/// <summary>
/// Интерфейс генератора моделей на основе схемы базы данных
/// </summary>
public interface ISchemaGenerator
{
    /// <summary>
    /// Генерирует C# модели на основе метаданных схемы
    /// </summary>
    /// <param name="schemaMetadata">Метаданные проанализированной схемы</param>
    /// <param name="options">Опции генерации</param>
    /// <returns>Результат генерации с информацией о созданных файлах</returns>
    ValueTask<SchemaGenerationResult> GenerateAsync(
        SchemaMetadata schemaMetadata,
        SchemaGenerationOptions? options = null);

    /// <summary>
    /// Генерирует модели только для таблиц
    /// </summary>
    /// <param name="schemaMetadata">Метаданные схемы</param>
    /// <param name="options">Опции генерации</param>
    /// <returns>Список сгенерированных моделей таблиц</returns>
    ValueTask<IReadOnlyList<GeneratedModel>> GenerateTableModelsAsync(
        SchemaMetadata schemaMetadata,
        SchemaGenerationOptions? options = null);

    /// <summary>
    /// Генерирует модели только для представлений
    /// </summary>
    /// <param name="schemaMetadata">Метаданные схемы</param>
    /// <param name="options">Опции генерации</param>
    /// <returns>Список сгенерированных моделей представлений</returns>
    ValueTask<IReadOnlyList<GeneratedModel>> GenerateViewModelsAsync(
        SchemaMetadata schemaMetadata,
        SchemaGenerationOptions? options = null);

    /// <summary>
    /// Генерирует модели для пользовательских типов (ENUM, DOMAIN, COMPOSITE)
    /// </summary>
    /// <param name="schemaMetadata">Метаданные схемы</param>
    /// <param name="options">Опции генерации</param>
    /// <returns>Список сгенерированных типов</returns>
    ValueTask<IReadOnlyList<GeneratedModel>> GenerateTypeModelsAsync(
        SchemaMetadata schemaMetadata,
        SchemaGenerationOptions? options = null);

    /// <summary>
    /// Генерирует параметры для функций и процедур
    /// </summary>
    /// <param name="schemaMetadata">Метаданные схемы</param>
    /// <param name="options">Опции генерации</param>
    /// <returns>Список сгенерированных моделей параметров функций</returns>
    ValueTask<IReadOnlyList<GeneratedModel>> GenerateFunctionParameterModelsAsync(
        SchemaMetadata schemaMetadata,
        SchemaGenerationOptions? options = null);

    /// <summary>
    /// Проверяет, нужна ли регенерация на основе изменений схемы
    /// </summary>
    /// <param name="schemaMetadata">Текущие метаданные схемы</param>
    /// <param name="existingFiles">Пути к существующим сгенерированным файлам</param>
    /// <returns>True, если требуется регенерация</returns>
    ValueTask<bool> RequiresRegenerationAsync(
        SchemaMetadata schemaMetadata,
        IReadOnlyList<string> existingFiles);
}
