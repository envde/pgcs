using PgCs.Common.CodeGeneration;
using PgCs.Common.QueryAnalyzer;
using PgCs.Common.QueryAnalyzer.Models.Annotations;
using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryAnalyzer.Models.Parameters;
using PgCs.Common.QueryAnalyzer.Models.Results;
using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.Services;
using PgCs.Common.Utils;
using PgCs.QueryAnalyzer.Parsing;

namespace PgCs.QueryAnalyzer;

/// <summary>
/// Анализатор SQL запросов в стиле sqlc для извлечения метаданных и генерации типобезопасного кода
/// </summary>
public sealed class QueryAnalyzer : IQueryAnalyzer
{
    private readonly SchemaMetadata? _schemaMetadata;
    private readonly INameConverter _nameConverter;

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
        _nameConverter = new NameConverter();
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
                
                // Собираем issues из QueryMetadata в общий список
                if (queryMetadata.ValidationIssues is not null)
                {
                    Issues.AddRange(queryMetadata.ValidationIssues);
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку парсинга
                var preview = StringParsingHelpers.Truncate(block, 150);
                
                Issues.Add(ValidationIssue.Error(
                    "QUERY_PARSE_ERROR",
                    $"Failed to parse query: {ex.Message}",
                    preview,
                    new Dictionary<string, string>
                    {
                        ["Exception"] = ex.GetType().Name,
                        ["QueryPreview"] = preview
                    }));
            }
        }

        // Проверка: если нет queries с аннотациями, это может быть проблемой
        var blockCount = blocks.Count();
        if (queries.Count == 0 && blockCount > 0)
        {
            Issues.Add(ValidationIssue.Warning(
                "NO_ANNOTATED_QUERIES",
                $"No queries with annotations found in file. Make sure to use '-- name: MethodName :one' annotations.",
                $"File: {sqlFilePath}",
                new Dictionary<string, string>
                {
                    ["BlocksFound"] = blockCount.ToString(),
                    ["FilePath"] = sqlFilePath
                }));
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

        var localIssues = new List<ValidationIssue>();

        try
        {
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
                ? InferReturnType(query, annotation.Name) 
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
                ReturnsDescription = annotation.Returns,
                ValidationIssues = localIssues.Count > 0 ? localIssues : null
            };
        }
        catch (Exception ex)
        {
            // Добавляем ошибку в локальные issues
            var preview = StringParsingHelpers.Truncate(sqlQuery, 150);
            
            localIssues.Add(ValidationIssue.Error(
                "QUERY_PARSE_ERROR",
                $"Failed to parse query: {ex.Message}",
                preview,
                new Dictionary<string, string>
                {
                    ["Exception"] = ex.GetType().Name,
                    ["QueryPreview"] = preview
                }));

            // Возвращаем metadata с ошибкой
            return new QueryMetadata
            {
                MethodName = "InvalidQuery",
                SqlQuery = sqlQuery,
                QueryType = QueryType.Select,
                ReturnCardinality = ReturnCardinality.Exec,
                Parameters = Array.Empty<QueryParameter>(),
                ReturnType = null,
                ValidationIssues = localIssues
            };
        }
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
    /// <param name="sqlQuery">SQL запрос для анализа</param>
    /// <param name="methodName">Имя метода для генерации уникального имени модели</param>
    public ReturnTypeInfo InferReturnType(string sqlQuery, string methodName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlQuery);
        ArgumentException.ThrowIfNullOrWhiteSpace(methodName);

        var columns = ColumnExtractor.Extract(sqlQuery, _schemaMetadata);
        
        // Проверяем является ли это SELECT * от одной таблицы
        var isSelectStar = IsSelectStarQuery(sqlQuery);
        var tableName = ExtractSingleTableName(sqlQuery);
        
        string modelName;
        bool requiresCustomModel;
        
        if (isSelectStar && tableName != null && _schemaMetadata != null)
        {
            // SELECT * - используем модель таблицы
            var table = _schemaMetadata.Tables.FirstOrDefault(t => 
                t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
                
            if (table != null)
            {
                // Используем имя таблицы как в схеме (там уже применён singularize)
                // SchemaGenerator создаёт модели с правильными именами (User, а не Users)
                modelName = _nameConverter.ToClassName(table.Name);
                requiresCustomModel = false; // Используем существующую модель схемы
            }
            else
            {
                // Таблица не найдена - создаем кастомную модель с именем метода
                modelName = methodName + "Result";
                requiresCustomModel = true;
            }
        }
        else
        {
            // Частичная выборка или несколько таблиц - создаем кастомную модель
            // Используем имя метода для уникального имени модели (GetUserById → GetUserByIdResult)
            modelName = methodName + "Result";
            requiresCustomModel = columns.Count > 0;
        }

        return new ReturnTypeInfo
        {
            ModelName = modelName,
            Columns = columns,
            RequiresCustomModel = requiresCustomModel
        };
    }

    /// <summary>
    /// Проверяет является ли запрос SELECT *
    /// </summary>
    private static bool IsSelectStarQuery(string sqlQuery)
    {
        // Простая проверка на SELECT *
        var selectPattern = @"\bSELECT\s+\*\s+FROM\b";
        return System.Text.RegularExpressions.Regex.IsMatch(
            sqlQuery, 
            selectPattern, 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Извлекает имя одной таблицы из FROM clause
    /// </summary>
    private static string? ExtractSingleTableName(string sqlQuery)
    {
        // Простой паттерн для извлечения имени таблицы из FROM clause
        var fromPattern = @"\bFROM\s+([a-zA-Z_][a-zA-Z0-9_]*)\b";
        var match = System.Text.RegularExpressions.Regex.Match(
            sqlQuery, 
            fromPattern, 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// Парсит комментарии sqlc формата для извлечения имени метода и кардинальности
    /// </summary>
    public QueryAnnotation ParseAnnotations(string comments)
    {
        return AnnotationParser.Parse(comments);
    }
}