using PgCs.Core.Definitions.Schema;
using PgCs.Core.Definitions.Schema.Base;

namespace PgCs.Core.SchemaAnalyzer;

/// <summary>
/// Интерфейс для фильтрации элементов схемы базы данных.
/// Используется для определения того, какие объекты должны быть включены в анализ и кодогенерацию.
/// </summary>
public interface ISchemaFilter
{
    /// <summary>
    /// Применяет фильтр ко всей схеме и возвращает отфильтрованные метаданные
    /// </summary>
    /// <param name="metadata">Исходные метаданные схемы</param>
    SchemaMetadata ApplyFilter(SchemaMetadata metadata);
}