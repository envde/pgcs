using PgCs.Common.QueryAnalyzer;
using PgCs.Common.QueryAnalyzer.Models.Metadata;

namespace PgCs.QueryAnalyzer;

/// <summary>
/// Fluent API builder для анализа SQL запросов
/// </summary>
public class QueryAnalyzerBuilder
{
    private readonly List<string> _filePaths = new();
    private readonly List<string> _sqlQueries = new();
    private bool _extractParameters = true;
    private bool _inferReturnTypes = true;
    private bool _parseAnnotations = true;
    private bool _validateSyntax = false;
    private bool _skipInvalidQueries = true;
    private readonly List<string> _includeOnlyQueryNames = new();
    private readonly List<string> _excludeQueryNames = new();
    private readonly List<string> _includeOnlyQueryTypes = new();

    private QueryAnalyzerBuilder() { }

    /// <summary>
    /// Создать новый builder для анализа запросов
    /// </summary>
    public static QueryAnalyzerBuilder Create() => new();

    /// <summary>
    /// Анализировать SQL файл с запросами
    /// </summary>
    public QueryAnalyzerBuilder FromFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePaths.Add(filePath);
        return this;
    }

    /// <summary>
    /// Анализировать несколько SQL файлов
    /// </summary>
    public QueryAnalyzerBuilder FromFiles(params string[] filePaths)
    {
        ArgumentNullException.ThrowIfNull(filePaths);
        _filePaths.AddRange(filePaths);
        return this;
    }

    /// <summary>
    /// Анализировать SQL запрос напрямую
    /// </summary>
    public QueryAnalyzerBuilder FromQuery(string sqlQuery)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlQuery);
        _sqlQueries.Add(sqlQuery);
        return this;
    }

    /// <summary>
    /// Анализировать несколько SQL запросов
    /// </summary>
    public QueryAnalyzerBuilder FromQueries(params string[] sqlQueries)
    {
        ArgumentNullException.ThrowIfNull(sqlQueries);
        _sqlQueries.AddRange(sqlQueries);
        return this;
    }

    /// <summary>
    /// Извлекать параметры из запросов (по умолчанию: true)
    /// </summary>
    public QueryAnalyzerBuilder WithParameterExtraction(bool extract = true)
    {
        _extractParameters = extract;
        return this;
    }

    /// <summary>
    /// НЕ извлекать параметры
    /// </summary>
    public QueryAnalyzerBuilder WithoutParameterExtraction()
    {
        _extractParameters = false;
        return this;
    }

    /// <summary>
    /// Определять типы возвращаемых значений (по умолчанию: true)
    /// </summary>
    public QueryAnalyzerBuilder WithTypeInference(bool infer = true)
    {
        _inferReturnTypes = infer;
        return this;
    }

    /// <summary>
    /// НЕ определять типы возвращаемых значений
    /// </summary>
    public QueryAnalyzerBuilder WithoutTypeInference()
    {
        _inferReturnTypes = false;
        return this;
    }

    /// <summary>
    /// Парсить аннотации sqlc (-- name: GetUser :one)
    /// </summary>
    public QueryAnalyzerBuilder WithAnnotationParsing(bool parse = true)
    {
        _parseAnnotations = parse;
        return this;
    }

    /// <summary>
    /// НЕ парсить аннотации
    /// </summary>
    public QueryAnalyzerBuilder WithoutAnnotationParsing()
    {
        _parseAnnotations = false;
        return this;
    }

    /// <summary>
    /// Валидировать синтаксис SQL запросов
    /// </summary>
    public QueryAnalyzerBuilder ValidateSyntax(bool validate = true)
    {
        _validateSyntax = validate;
        return this;
    }

    /// <summary>
    /// Пропускать невалидные запросы вместо выброса исключений
    /// </summary>
    public QueryAnalyzerBuilder SkipInvalidQueries(bool skip = true)
    {
        _skipInvalidQueries = skip;
        return this;
    }

    /// <summary>
    /// Бросать исключение при невалидном запросе
    /// </summary>
    public QueryAnalyzerBuilder FailOnInvalidQuery()
    {
        _skipInvalidQueries = false;
        return this;
    }

    /// <summary>
    /// Анализировать только запросы с указанными именами
    /// </summary>
    public QueryAnalyzerBuilder IncludeOnlyQueries(params string[] queryNames)
    {
        ArgumentNullException.ThrowIfNull(queryNames);
        _includeOnlyQueryNames.AddRange(queryNames);
        return this;
    }

    /// <summary>
    /// Исключить запросы с указанными именами
    /// </summary>
    public QueryAnalyzerBuilder ExcludeQueries(params string[] queryNames)
    {
        ArgumentNullException.ThrowIfNull(queryNames);
        _excludeQueryNames.AddRange(queryNames);
        return this;
    }

    /// <summary>
    /// Анализировать только запросы определённого типа (SELECT, INSERT, UPDATE, DELETE)
    /// </summary>
    public QueryAnalyzerBuilder IncludeOnlyQueryTypes(params string[] queryTypes)
    {
        ArgumentNullException.ThrowIfNull(queryTypes);
        _includeOnlyQueryTypes.AddRange(queryTypes);
        return this;
    }

    /// <summary>
    /// Анализировать только SELECT запросы
    /// </summary>
    public QueryAnalyzerBuilder OnlySelects()
    {
        _includeOnlyQueryTypes.Clear();
        _includeOnlyQueryTypes.Add("SELECT");
        return this;
    }

    /// <summary>
    /// Анализировать только INSERT запросы
    /// </summary>
    public QueryAnalyzerBuilder OnlyInserts()
    {
        _includeOnlyQueryTypes.Clear();
        _includeOnlyQueryTypes.Add("INSERT");
        return this;
    }

    /// <summary>
    /// Анализировать только UPDATE запросы
    /// </summary>
    public QueryAnalyzerBuilder OnlyUpdates()
    {
        _includeOnlyQueryTypes.Clear();
        _includeOnlyQueryTypes.Add("UPDATE");
        return this;
    }

    /// <summary>
    /// Анализировать только DELETE запросы
    /// </summary>
    public QueryAnalyzerBuilder OnlyDeletes()
    {
        _includeOnlyQueryTypes.Clear();
        _includeOnlyQueryTypes.Add("DELETE");
        return this;
    }

    /// <summary>
    /// Выполнить анализ запросов
    /// </summary>
    public async ValueTask<IReadOnlyList<QueryMetadata>> AnalyzeAsync()
    {
        if (_filePaths.Count == 0 && _sqlQueries.Count == 0)
        {
            throw new InvalidOperationException(
                "No source specified. Use FromFile() or FromQuery().");
        }

        var analyzer = new QueryAnalyzer();
        var allQueries = new List<QueryMetadata>();

        // Анализ файлов
        foreach (var filePath in _filePaths)
        {
            try
            {
                var queries = await analyzer.AnalyzeFileAsync(filePath);
                allQueries.AddRange(queries);
            }
            catch
            {
                if (!_skipInvalidQueries)
                    throw;
            }
        }

        // Анализ отдельных запросов
        foreach (var query in _sqlQueries)
        {
            try
            {
                var metadata = analyzer.AnalyzeQuery(query);
                allQueries.Add(metadata);
            }
            catch
            {
                if (!_skipInvalidQueries)
                    throw;
            }
        }

        // Применить фильтры
        var filtered = ApplyFilters(allQueries);

        return filtered;
    }

    private IReadOnlyList<QueryMetadata> ApplyFilters(List<QueryMetadata> queries)
    {
        var filtered = queries.AsEnumerable();

        // Фильтрация по именам (включить только)
        if (_includeOnlyQueryNames.Count > 0)
        {
            filtered = filtered.Where(q => _includeOnlyQueryNames.Contains(q.MethodName));
        }

        // Фильтрация по именам (исключить)
        if (_excludeQueryNames.Count > 0)
        {
            filtered = filtered.Where(q => !_excludeQueryNames.Contains(q.MethodName));
        }

        // Фильтрация по типам запросов
        if (_includeOnlyQueryTypes.Count > 0)
        {
            filtered = filtered.Where(q => 
                _includeOnlyQueryTypes.Contains(q.QueryType.ToString().ToUpperInvariant()));
        }

        return filtered.ToList();
    }
}
