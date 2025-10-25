using PgCs.Common.CodeGeneration;
using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.SchemaAnalyzer.Models;

namespace PgCs.QueryAnalyzer;

/// <summary>
/// Fluent API builder для анализа SQL запросов
/// </summary>
public sealed class QueryAnalyzerBuilder
{
    private readonly List<string> _files = [];
    private SchemaMetadata? _schemaMetadata;

    /// <summary>
    /// ValidationIssues собранные во время последнего анализа
    /// </summary>
    public List<ValidationIssue> Issues { get; } = new();

    private QueryAnalyzerBuilder()
    {
    }

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
        _files.Add(filePath);
        return this;
    }

    /// <summary>
    /// Анализировать несколько SQL файлов
    /// </summary>
    public QueryAnalyzerBuilder FromFiles(params string[] filePaths)
    {
        ArgumentNullException.ThrowIfNull(filePaths);
        _files.AddRange(filePaths);
        return this;
    }

    /// <summary>
    /// Анализировать все SQL файлы в директории (рекурсивно)
    /// </summary>
    public QueryAnalyzerBuilder FromDirectory(string directoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        var sqlFiles = Directory.GetFiles(directoryPath, "*.sql", SearchOption.AllDirectories);
        _files.AddRange(sqlFiles);
        
        return this;
    }

    /// <summary>
    /// Использовать метаданные схемы для определения типов колонок
    /// </summary>
    public QueryAnalyzerBuilder WithSchema(SchemaMetadata schemaMetadata)
    {
        _schemaMetadata = schemaMetadata;
        return this;
    }

    /// <summary>
    /// Выполнить анализ запросов
    /// </summary>
    public async ValueTask<IReadOnlyList<QueryMetadata>> AnalyzeAsync()
    {
        if (_files.Count == 0)
        {
            throw new InvalidOperationException(
                "No source specified. Use FromFile() or FromDirectory().");
        }

        Issues.Clear(); // Очищаем перед новым анализом
        var analyzer = new QueryAnalyzer(_schemaMetadata);
        var allQueries = new List<QueryMetadata>();

        // Анализ файлов
        foreach (var filePath in _files)
        {
            var queries = await analyzer.AnalyzeFileAsync(filePath);
            allQueries.AddRange(queries);
            
            // Собираем issues из analyzer
            Issues.AddRange(analyzer.Issues);
        }

        return allQueries;
    }
}
