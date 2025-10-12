
using PgCs.Common.QueryAnalyzer;
using PgCs.QueryAnalyzer.Parsing;

namespace PgCs.QueryAnalyzer;

public sealed class QueryAnalyzer : IQueryAnalyzer
{
    public async ValueTask<IReadOnlyList<QueryMetadata>> AnalyzeFileAsync(string sqlFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlFilePath);

        if (!File.Exists(sqlFilePath))
            throw new FileNotFoundException($"SQL файл не найден: {sqlFilePath}");

        var content = await File.ReadAllTextAsync(sqlFilePath);
        var queries = new List<QueryMetadata>();

        foreach (var block in SqlQueryParser.SplitIntoQueryBlocks(content))
        {
            if (!AnnotationParser.HasAnnotation(block))
                continue;

            try
            {
                queries.Add(AnalyzeQuery(block));
            }
            catch
            {
                // Пропускаем некорректные запросы
            }
        }

        return queries;
    }

    public QueryMetadata AnalyzeQuery(string sqlQuery)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlQuery);

        var (comments, query) = SqlQueryParser.SplitCommentsAndQuery(sqlQuery.AsSpan());
        var annotation = ParseAnnotations(comments);
        var queryType = SqlQueryParser.DetermineQueryType(query);
        var parameters = ExtractParameters(query);
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

    public IReadOnlyList<QueryParameter> ExtractParameters(string sqlQuery)
    {
        return ParameterExtractor.Extract(sqlQuery);
    }

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

    public QueryAnnotation ParseAnnotations(string comments)
    {
        return AnnotationParser.Parse(comments);
    }
}