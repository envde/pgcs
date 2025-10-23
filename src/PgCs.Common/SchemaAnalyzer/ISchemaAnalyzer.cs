using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaAnalyzer.Models.Functions;
using PgCs.Common.SchemaAnalyzer.Models.Indexes;
using PgCs.Common.SchemaAnalyzer.Models.Tables;
using PgCs.Common.SchemaAnalyzer.Models.Triggers;
using PgCs.Common.SchemaAnalyzer.Models.Types;
using PgCs.Common.SchemaAnalyzer.Models.Views;

namespace PgCs.Common.SchemaAnalyzer;

public interface ISchemaAnalyzer
{
    /// <summary>
    /// Анализирует SQL файл схемы и извлекает все объекты базы данных
    /// </summary>
    /// <param name="schemaFilePath">Путь к SQL файлу со схемой</param>
    /// <returns>Метаданные схемы базы данных</returns>
    ValueTask<SchemaMetadata> AnalyzeFileAsync(string schemaFilePath);

    /// <summary>
    /// Анализирует папку с SQL файлами схемы и объединяет их в единую схему
    /// </summary>
    /// <param name="schemaDirectoryPath">Путь к папке с SQL файлами</param>
    /// <returns>Объединённые метаданные схемы базы данных</returns>
    ValueTask<SchemaMetadata> AnalyzeDirectoryAsync(string schemaDirectoryPath);

    /// <summary>
    /// Извлекает определения таблиц из SQL скрипта
    /// </summary>
    /// <param name="sqlScript">SQL скрипт</param>
    /// <returns>Список определений таблиц</returns>
    IReadOnlyList<TableDefinition> ExtractTables(string sqlScript);

    /// <summary>
    /// Извлекает определения представлений из SQL скрипта
    /// </summary>
    /// <param name="sqlScript">SQL скрипт</param>
    /// <returns>Список определений представлений</returns>
    IReadOnlyList<ViewDefinition> ExtractViews(string sqlScript);

    /// <summary>
    /// Извлекает определения пользовательских типов (ENUM, DOMAIN, COMPOSITE)
    /// </summary>
    /// <param name="sqlScript">SQL скрипт</param>
    /// <returns>Список определений типов</returns>
    IReadOnlyList<TypeDefinition> ExtractTypes(string sqlScript);

    /// <summary>
    /// Извлекает определения функций и процедур
    /// </summary>
    /// <param name="sqlScript">SQL скрипт</param>
    /// <returns>Список определений функций</returns>
    IReadOnlyList<FunctionDefinition> ExtractFunctions(string sqlScript);

    /// <summary>
    /// Извлекает определения индексов
    /// </summary>
    /// <param name="sqlScript">SQL скрипт</param>
    /// <returns>Список определений индексов</returns>
    IReadOnlyList<IndexDefinition> ExtractIndexes(string sqlScript);

    /// <summary>
    /// Извлекает определения триггеров
    /// </summary>
    /// <param name="sqlScript">SQL скрипт</param>
    /// <returns>Список определений триггеров</returns>
    IReadOnlyList<TriggerDefinition> ExtractTriggers(string sqlScript);

    /// <summary>
    /// Извлекает ограничения (constraints) из таблиц
    /// </summary>
    /// <param name="sqlScript">SQL скрипт</param>
    /// <returns>Список определений ограничений</returns>
    IReadOnlyList<ConstraintDefinition> ExtractConstraints(string sqlScript);
}
