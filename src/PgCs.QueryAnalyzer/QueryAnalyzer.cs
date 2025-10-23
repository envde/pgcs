using PgCs.Common.QueryAnalyzer;
using PgCs.Common.QueryAnalyzer.Models.Annotations;
using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryAnalyzer.Models.Parameters;
using PgCs.Common.QueryAnalyzer.Models.Results;
using PgCs.QueryAnalyzer.Parsing;

namespace PgCs.QueryAnalyzer;

/// <summary>
/// Анализатор SQL запросов в стиле sqlc для извлечения метаданных и генерации типобезопасного кода
/// </summary>
public sealed class QueryAnalyzer : IQueryAnalyzer
{
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

        var content = await File.ReadAllTextAsync(sqlFilePath);
        var queries = new List<QueryMetadata>();

        // Разбиваем файл на блоки запросов
        foreach (var block in SqlQueryParser.SplitIntoQueryBlocks(content))
        {
            // Обрабатываем только блоки с аннотациями
            if (!AnnotationParser.HasAnnotation(block))
                continue;

            try
            {
                queries.Add(AnalyzeQuery(block));
            }
            catch
            {
                // Пропускаем некорректные запросы (опционально можно логировать)
            }
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
            ReturnType = returnType
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

        var columns = ColumnExtractor.Extract(sqlQuery);
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