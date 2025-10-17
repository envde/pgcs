using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryAnalyzer.Models.Parameters;
using PgCs.Common.QueryAnalyzer.Models.Results;

namespace PgCs.QueryGenerator.Services;

/// <summary>
/// Строитель Npgsql команд для выполнения SQL запросов
/// </summary>
public interface INpgsqlCommandBuilder
{
    /// <summary>
    /// Генерирует код для создания NpgsqlCommand
    /// </summary>
    string BuildCommandCreationCode(QueryMetadata queryMetadata, string connectionVariableName);

    /// <summary>
    /// Генерирует код для добавления параметра в команду
    /// </summary>
    string BuildParameterCode(QueryParameter parameter, string parameterSourceName);

    /// <summary>
    /// Генерирует код для чтения результата из NpgsqlDataReader
    /// </summary>
    string BuildReaderMappingCode(ReturnColumn column, string readerVariableName, int ordinal);
}
