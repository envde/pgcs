using PgCs.Common.CodeGeneration;
using PgCs.Common.QueryAnalyzer;
using PgCs.Common.QueryAnalyzer.Models.Annotations;
using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryAnalyzer.Models.Parameters;
using PgCs.Common.QueryAnalyzer.Models.Results;
using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.QueryAnalyzer.Parsing;

namespace PgCs.QueryAnalyzer;

/// <summary>
/// Анализатор SQL запросов в стиле sqlc для извлечения метаданных и генерации типобезопасного кода
/// </summary>
public sealed class QueryAnalyzer : IQueryAnalyzer
{
    private readonly SchemaMetadata? _schemaMetadata;

    /// <summary>
    /// Warnings и errors собранные во время parsing queries
    /// </summary>
    public List<ValidationIssue> Issues { get; } = new();

    /// <summary>
    /// Создает новый экземпляр анализатора запросов
    /// </summary>
    /// <param name="schemaMetadata">Метаданные схемы для определения типов колонок (опционально)</param>
    public QueryAnalyzer(SchemaMetadata? schemaMetadata = null)
    {
        _schemaMetadata = schemaMetadata;
    }
    /// <summary>
    /// Анализирует SQL файл и извлекает все запросы с аннотациями sqlc
    /// </summary>
    /// <param name="sqlFilePath">Абсолютный путь к SQL файлу</param>
    /// <returns>Список проанализированных запросов с метаданными</returns>
    /// <exception cref="FileNotFoundException">Файл не найден</exception>
    public async ValueTask<IReadOnlyList<QueryMetadata>> AnalyzeFileAsync(string sqlFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlFilePath);

        if (!File.Exists(sqlFilePath))
            throw new FileNotFoundException($"SQL файл не найден: {sqlFilePath}");

        Issues.Clear(); // Очищаем перед новым анализом
        var content = await File.ReadAllTextAsync(sqlFilePath);
        var queries = new List<QueryMetadata>();
        var blocks = SqlQueryParser.SplitIntoQueryBlocks(content);

        // Разбиваем файл на блоки запросов
        foreach (var block in blocks)
        {
            // Обрабатываем только блоки с аннотациями
            if (!AnnotationParser.HasAnnotation(block))
                continue;

            try
            {
                var queryMetadata = AnalyzeQuery(block);
                queries.Add(queryMetadata);
            }
            catch (Exception ex)
            {
                // Логируем ошибку парсинга
                var preview = block.Length > 150 
                    ? block.Substring(0, 150) + "..." 
                    : block;
                
                Issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Code = "QUERY_PARSE_ERROR",
                    Message = $"Failed to parse query: {ex.Message}",
                    Location = preview,
                    Details = new Dictionary<string, string>
                    {
                        ["Exception"] = ex.GetType().Name,
                        ["QueryPreview"] = preview
                    }
                });
            }
        }

        // Проверка: если нет queries с аннотациями, это может быть проблемой
        var blockCount = blocks.Count();
        if (queries.Count == 0 && blockCount > 0)
        {
            Issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Warning,
                Code = "NO_ANNOTATED_QUERIES",
                Message = $"No queries with annotations found in file. Make sure to use '-- name: MethodName :one' annotations.",
                Location = $"File: {sqlFilePath}",
                Details = new Dictionary<string, string>
                {
                    ["BlocksFound"] = blockCount.ToString(),
                    ["FilePath"] = sqlFilePath
                }
            });
        }

        return queries;
    }

    /// <summary>
    /// Анализирует отдельный SQL запрос с аннотациями и извлекает метаданные
    /// </summary>
    /// <param name="sqlQuery">SQL запрос с комментариями-аннотациями</param>
    /// <returns>Полные метаданные запроса (имя, параметры, возвращаемый тип)</returns>
    public QueryMetadata AnalyzeQuery(string sqlQuery)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlQuery);

        // Разделяем комментарии и сам SQL запрос
        var (comments, query) = SqlQueryParser.SplitCommentsAndQuery(sqlQuery.AsSpan());
        
        // Парсим аннотации (-- name: MethodName :cardinality)
        var annotation = ParseAnnotations(comments);
        
        // Определяем тип запроса (SELECT, INSERT, etc.)
        var queryType = SqlQueryParser.DetermineQueryType(query);
        
        // Извлекаем параметры ($1, @param)
        var parameters = ExtractParameters(query);
        
        // Определяем возвращаемый тип (для SELECT запросов)
        var returnType = annotation.Cardinality is not ReturnCardinality.Exec 
            ? InferReturnType(query) 
            : null;

        return new QueryMetadata
        {
            MethodName = annotation.Name,
            SqlQuery = query,
            QueryType = queryType,
            ReturnCardinality = annotation.Cardinality,
            Parameters = parameters,
            ReturnType = returnType,
            Summary = annotation.Summary,
            ParameterDescriptions = annotation.ParameterDescriptions,
            ReturnsDescription = annotation.Returns
        };
    }

    /// <summary>
    /// Извлекает все параметры из SQL запроса (@param или $param синтаксис)
    /// </summary>
    public IReadOnlyList<QueryParameter> ExtractParameters(string sqlQuery)
    {
        return ParameterExtractor.Extract(sqlQuery);
    }

    /// <summary>
    /// Определяет тип возвращаемого значения на основе SELECT/RETURNING части запроса
    /// </summary>
    public ReturnTypeInfo InferReturnType(string sqlQuery)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlQuery);

        var columns = ColumnExtractor.Extract(sqlQuery, _schemaMetadata);
        var modelName = ModelNameGenerator.Generate(columns);

        return new ReturnTypeInfo
        {
            ModelName = modelName,
            Columns = columns,
            RequiresCustomModel = columns.Count > 0
        };
    }

    /// <summary>
    /// Парсит комментарии sqlc формата для извлечения имени метода и кардинальности
    /// </summary>
    public QueryAnnotation ParseAnnotations(string comments)
    {
        return AnnotationParser.Parse(comments);
    }
}