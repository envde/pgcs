using PgCs.Common.SchemaAnalyzer.Models.Tables;
using PgCs.Common.SchemaGenerator.Models;

namespace PgCs.SchemaGenerator.Generation;

/// <summary>
/// Интерфейс генератора моделей таблиц
/// </summary>
internal interface ITableModelGenerator
{
    /// <summary>
    /// Генерирует модель для таблицы
    /// </summary>
    GeneratedModel Generate(TableDefinition table, SchemaGenerationOptions options);
}
