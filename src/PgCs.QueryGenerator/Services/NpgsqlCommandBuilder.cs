using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryAnalyzer.Models.Parameters;
using PgCs.Common.QueryAnalyzer.Models.Results;
using PgCs.Common.Services;

namespace PgCs.QueryGenerator.Services;

/// <summary>
/// Строитель Npgsql команд для выполнения SQL запросов
/// </summary>
public sealed class NpgsqlCommandBuilder : INpgsqlCommandBuilder
{
    private readonly INameConverter _nameConverter;

    public NpgsqlCommandBuilder(INameConverter nameConverter)
    {
        _nameConverter = nameConverter;
    }

    public NpgsqlCommandBuilder() : this(new NameConverter())
    {
    }

    public string BuildCommandCreationCode(QueryMetadata queryMetadata, string connectionVariableName)
    {
        return $@"await using var command = {connectionVariableName}.CreateCommand();
command.CommandText = @""{queryMetadata.SqlQuery.Replace("\"", "\"\"")}"";";
    }

    public string BuildParameterCode(QueryParameter parameter, string parameterSourceName)
    {
        var paramName = _nameConverter.ToParameterName(parameter.Name);
        
        return parameter.IsNullable
            ? $"command.Parameters.Add(new NpgsqlParameter(\"{parameter.Name}\", {parameterSourceName} ?? (object)DBNull.Value));"
            : $"command.Parameters.AddWithValue(\"{parameter.Name}\", {parameterSourceName});";
    }

    public string BuildReaderMappingCode(ReturnColumn column, string readerVariableName, int ordinal)
    {
        var propertyName = _nameConverter.ToPropertyName(column.Name);

        if (column.IsNullable)
        {
            // Для nullable типов проверяем DBNull
            if (IsReferenceType(column.CSharpType))
            {
                return $"{propertyName} = {readerVariableName}.IsDBNull({ordinal}) ? null : {readerVariableName}.Get{GetReaderMethod(column.CSharpType)}({ordinal})";
            }
            else
            {
                return $"{propertyName} = {readerVariableName}.IsDBNull({ordinal}) ? null : ({column.CSharpType.TrimEnd('?')}?){readerVariableName}.Get{GetReaderMethod(column.CSharpType)}({ordinal})";
            }
        }
        else
        {
            return $"{propertyName} = {readerVariableName}.Get{GetReaderMethod(column.CSharpType)}({ordinal})";
        }
    }

    /// <summary>
    /// Получает имя метода NpgsqlDataReader для чтения типа
    /// </summary>
    private static string GetReaderMethod(string csharpType)
    {
        var cleanType = csharpType.TrimEnd('?', '[', ']');

        return cleanType switch
        {
            "bool" => "Boolean",
            "byte" => "Byte",
            "short" => "Int16",
            "int" => "Int32",
            "long" => "Int64",
            "float" => "Float",
            "double" => "Double",
            "decimal" => "Decimal",
            "string" => "String",
            "DateTime" => "DateTime",
            "DateTimeOffset" => "DateTimeOffset",
            "DateOnly" => "FieldValue<DateOnly>",
            "TimeOnly" => "FieldValue<TimeOnly>",
            "TimeSpan" => "TimeSpan",
            "Guid" => "Guid",
            "byte[]" => "FieldValue<byte[]>",
            _ when cleanType.Contains("IPAddress") => "FieldValue<System.Net.IPAddress>",
            _ when cleanType.Contains("PhysicalAddress") => "FieldValue<System.Net.NetworkInformation.PhysicalAddress>",
            _ => $"FieldValue<{cleanType}>"
        };
    }

    /// <summary>
    /// Проверяет, является ли тип reference type
    /// </summary>
    private static bool IsReferenceType(string csharpType)
    {
        var cleanType = csharpType.TrimEnd('?', '[', ']');
        return cleanType switch
        {
            "string" => true,
            "byte[]" => true,
            "object" => true,
            _ when cleanType.Contains('.') => true, // Предполагаем что типы с namespace - reference types
            _ => false
        };
    }
}
