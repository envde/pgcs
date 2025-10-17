using PgCs.Common.CodeGeneration;
using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaGenerator.Models.Options;
using PgCs.Common.SchemaGenerator.Models.Results;

namespace PgCs.Common.SchemaGenerator;

/// <summary>
/// Генератор C# кода на основе схемы PostgreSQL базы данных
/// </summary>
public interface ISchemaGenerator
{
    /// <summary>
    /// Генерирует все C# модели на основе метаданных схемы базы данных
    /// </summary>
    /// <param name="schemaMetadata">Метаданные схемы БД</param>
    /// <param name="options">Опции генерации</param>
    /// <returns>Результаты генерации со списком сгенерированных файлов (код)</returns>
    ValueTask<SchemaGenerationResult> GenerateAsync(
        SchemaMetadata schemaMetadata, 
        SchemaGenerationOptions options);

    /// <summary>
    /// Генерирует модели для таблиц базы данных
    /// </summary>
    /// <param name="schemaMetadata">Метаданные схемы БД</param>
    /// <param name="options">Опции генерации</param>
    /// <returns>Список сгенерированного кода моделей таблиц</returns>
    ValueTask<IReadOnlyList<GeneratedCode>> GenerateTableModelsAsync(
        SchemaMetadata schemaMetadata,
        SchemaGenerationOptions options);

    /// <summary>
    /// Генерирует модели для представлений (views) базы данных
    /// </summary>
    /// <param name="schemaMetadata">Метаданные схемы БД</param>
    /// <param name="options">Опции генерации</param>
    /// <returns>Список сгенерированного кода моделей представлений</returns>
    ValueTask<IReadOnlyList<GeneratedCode>> GenerateViewModelsAsync(
        SchemaMetadata schemaMetadata,
        SchemaGenerationOptions options);

    /// <summary>
    /// Генерирует C# типы для пользовательских типов PostgreSQL (ENUM, DOMAIN, COMPOSITE)
    /// </summary>
    /// <param name="schemaMetadata">Метаданные схемы БД</param>
    /// <param name="options">Опции генерации</param>
    /// <returns>Результаты генерации пользовательских типов с статистикой</returns>
    ValueTask<IReadOnlyList<GeneratedCode>> GenerateCustomTypesAsync(
        SchemaMetadata schemaMetadata,
        SchemaGenerationOptions options);

    /// <summary>
    /// Генерирует методы для функций и процедур базы данных
    /// </summary>
    /// <param name="schemaMetadata">Метаданные схемы БД</param>
    /// <param name="options">Опции генерации</param>
    /// <returns>Список сгенерированного кода методов для функций</returns>
    ValueTask<IReadOnlyList<GeneratedCode>> GenerateFunctionMethodsAsync(
        SchemaMetadata schemaMetadata,
        SchemaGenerationOptions options);

    /// <summary>
    /// Проверяет корректность метаданных схемы перед генерацией
    /// </summary>
    /// <param name="schemaMetadata">Метаданные схемы БД</param>
    /// <returns>Список предупреждений и ошибок валидации</returns>
    IReadOnlyList<ValidationIssue> ValidateSchema(SchemaMetadata schemaMetadata);

    /// <summary>
    /// Форматирует сгенерированный C# код с использованием Roslyn
    /// </summary>
    /// <param name="sourceCode">Исходный код для форматирования</param>
    /// <returns>Отформатированный код</returns>
    ValueTask<string> FormatCodeAsync(string sourceCode);
}
