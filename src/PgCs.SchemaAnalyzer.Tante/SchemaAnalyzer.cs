using PgCs.Core.Extraction;
using PgCs.Core.Extraction.Block;
using PgCs.Core.Schema.Analyzer;
using PgCs.Core.Schema.Common;
using PgCs.Core.Schema.Definitions;
using PgCs.SchemaAnalyzer.Tante.Extractors;

namespace PgCs.SchemaAnalyzer.Tante;

/// <summary>
/// Анализатор схемы PostgreSQL базы данных.
/// Извлекает определения объектов из SQL файлов и скриптов.
/// </summary>
public sealed class SchemaAnalyzer : ISchemaAnalyzer
{
    private readonly IBlockExtractor _blockExtractor;
    private readonly IExtractor<EnumTypeDefinition> _enumExtractor;
    private readonly ICompositeExtractor _compositeExtractor;
    private readonly IExtractor<DomainTypeDefinition> _domainExtractor;
    private readonly ITableExtractor _tableExtractor;
    private readonly IViewExtractor _viewExtractor;
    private readonly IExtractor<FunctionDefinition> _functionExtractor;
    private readonly IIndexExtractor _indexExtractor;
    private readonly ITriggerExtractor _triggerExtractor;
    private readonly IConstraintExtractor _constraintExtractor;
    private readonly IExtractor<CommentDefinition> _commentExtractor;

    /// <summary>
    /// Создает новый экземпляр анализатора схемы с инжекцией зависимостей
    /// </summary>
    public SchemaAnalyzer(
        IBlockExtractor blockExtractor,
        IExtractor<EnumTypeDefinition> enumExtractor,
        ICompositeExtractor compositeExtractor,
        IExtractor<DomainTypeDefinition> domainExtractor,
        ITableExtractor tableExtractor,
        IViewExtractor viewExtractor,
        IExtractor<FunctionDefinition> functionExtractor,
        IIndexExtractor indexExtractor,
        ITriggerExtractor triggerExtractor,
        IConstraintExtractor constraintExtractor,
        IExtractor<CommentDefinition> commentExtractor)
    {
        ArgumentNullException.ThrowIfNull(blockExtractor);
        ArgumentNullException.ThrowIfNull(enumExtractor);
        ArgumentNullException.ThrowIfNull(compositeExtractor);
        ArgumentNullException.ThrowIfNull(domainExtractor);
        ArgumentNullException.ThrowIfNull(tableExtractor);
        ArgumentNullException.ThrowIfNull(viewExtractor);
        ArgumentNullException.ThrowIfNull(functionExtractor);
        ArgumentNullException.ThrowIfNull(indexExtractor);
        ArgumentNullException.ThrowIfNull(triggerExtractor);
        ArgumentNullException.ThrowIfNull(constraintExtractor);
        ArgumentNullException.ThrowIfNull(commentExtractor);
        
        _blockExtractor = blockExtractor;
        _enumExtractor = enumExtractor;
        _compositeExtractor = compositeExtractor;
        _domainExtractor = domainExtractor;
        _tableExtractor = tableExtractor;
        _viewExtractor = viewExtractor;
        _functionExtractor = functionExtractor;
        _indexExtractor = indexExtractor;
        _triggerExtractor = triggerExtractor;
        _constraintExtractor = constraintExtractor;
        _commentExtractor = commentExtractor;
    }

    /// <summary>
    /// Создает новый экземпляр анализатора схемы с реализациями по умолчанию
    /// </summary>
    public SchemaAnalyzer() : this(
        new BlockExtractor(),
        new EnumExtractor(),
        new CompositeExtractor(),
        new DomainExtractor(),
        new TableExtractor(),
        new ViewExtractor(),
        new FunctionExtractor(),
        new IndexExtractor(),
        new TriggerExtractor(),
        new ConstraintExtractor(),
        new CommentExtractor())
    {
    }

    /// <inheritdoc />
    public async ValueTask<SchemaMetadata> AnalyzeFileAsync(string schemaFilePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaFilePath);
        
        if (!File.Exists(schemaFilePath))
        {
            throw new FileNotFoundException($"Schema file not found: {schemaFilePath}", schemaFilePath);
        }

        var sqlContent = await File.ReadAllTextAsync(schemaFilePath, cancellationToken);
        var blocks = _blockExtractor.Extract(sqlContent);
        
        // Добавляем информацию о файле-источнике к блокам
        var blocksWithSource = blocks.Select(b => b with { SourcePath = schemaFilePath }).ToList();
        
        return AnalyzeBlocks(blocksWithSource, [schemaFilePath]);
    }

    /// <inheritdoc />
    public async ValueTask<SchemaMetadata> AnalyzeDirectoryAsync(string schemaDirectoryPath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaDirectoryPath);
        
        if (!Directory.Exists(schemaDirectoryPath))
        {
            throw new DirectoryNotFoundException($"Schema directory not found: {schemaDirectoryPath}");
        }

        var sqlFiles = Directory.GetFiles(schemaDirectoryPath, "*.sql", SearchOption.AllDirectories)
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToArray();

        if (sqlFiles.Length == 0)
        {
            return CreateEmptyMetadata([], schemaDirectoryPath);
        }

        var allBlocks = new List<SqlBlock>();
        var sourcePaths = new List<string>();

        foreach (var filePath in sqlFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var sqlContent = await File.ReadAllTextAsync(filePath, cancellationToken);
            var blocks = _blockExtractor.Extract(sqlContent);
            
            // Добавляем информацию о файле-источнике
            var blocksWithSource = blocks.Select(b => b with { SourcePath = filePath });
            allBlocks.AddRange(blocksWithSource);
            sourcePaths.Add(filePath);
        }

        return AnalyzeBlocks(allBlocks, sourcePaths);
    }

    /// <inheritdoc />
    public IReadOnlyList<EnumTypeDefinition> ExtractEnums(string sqlScript)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlScript);
        
        var blocks = _blockExtractor.Extract(sqlScript);
        var enums = new List<EnumTypeDefinition>();

        foreach (var block in blocks)
        {
            var blockList = new[] { block };
            
            if (!_enumExtractor.CanExtract(blockList))
            {
                continue;
            }

            var result = _enumExtractor.Extract(blockList);
            if (result.IsSuccess && result.Definition is not null)
            {
                enums.Add(result.Definition);
            }
        }

        return enums;
    }

    public IReadOnlyList<TableDefinition> ExtractTables(string sqlScript)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlScript);
        
        var blocks = _blockExtractor.Extract(sqlScript);
        var tables = new List<TableDefinition>();

        foreach (var block in blocks)
        {
            if (!_tableExtractor.CanExtract(block))
            {
                continue;
            }

            var tableDef = _tableExtractor.Extract(block);
            if (tableDef is not null)
            {
                tables.Add(tableDef);
            }
        }

        return tables;
    }

    public IReadOnlyList<ViewDefinition> ExtractViews(string sqlScript)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlScript);
        
        var blocks = _blockExtractor.Extract(sqlScript).ToList();
        var views = new List<ViewDefinition>();

        for (int i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            
            // Проверяем, содержит ли блок VIEW
            if (!_viewExtractor.CanExtract([block]))
            {
                continue;
            }

            // Передаем все блоки, начиная с текущего
            var blocksToProcess = blocks.Skip(i).ToList();
            var viewDef = _viewExtractor.Extract(blocksToProcess);
            if (viewDef is not null)
            {
                views.Add(viewDef);
            }
        }

        return views;
    }

    public IReadOnlyList<DomainTypeDefinition> ExtractDomains(string sqlScript)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlScript);
        
        var blocks = _blockExtractor.Extract(sqlScript);
        var domains = new List<DomainTypeDefinition>();

        foreach (var block in blocks)
        {
            var blockList = new[] { block };
            var result = _domainExtractor.Extract(blockList);
            
            if (result.IsSuccess && result.Definition is not null)
            {
                domains.Add(result.Definition);
            }
        }

        return domains;
    }

    public IReadOnlyList<CompositeTypeDefinition> ExtractComposites(string sqlScript)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlScript);
        
        var blocks = _blockExtractor.Extract(sqlScript);
        var composites = new List<CompositeTypeDefinition>();

        foreach (var block in blocks)
        {
            if (!_compositeExtractor.CanExtract(block))
            {
                continue;
            }

            var compositeDef = _compositeExtractor.Extract(block);
            if (compositeDef is not null)
            {
                composites.Add(compositeDef);
            }
        }

        return composites;
    }

    public IReadOnlyList<FunctionDefinition> ExtractFunctions(string sqlScript)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlScript);
        
        var blocks = _blockExtractor.Extract(sqlScript);
        var functions = new List<FunctionDefinition>();

        foreach (var block in blocks)
        {
            var blockList = new[] { block };
            var result = _functionExtractor.Extract(blockList);
            
            if (result.IsSuccess && result.Definition is not null)
            {
                functions.Add(result.Definition);
            }
        }

        return functions;
    }

    /// <summary>
    /// Извлекает комментарии из SQL скрипта
    /// </summary>
    public IReadOnlyList<CommentDefinition> ExtractComments(string sqlScript)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlScript);
        
        var blocks = _blockExtractor.Extract(sqlScript);
        var comments = new List<CommentDefinition>();

        foreach (var block in blocks)
        {
            var blockList = new[] { block };
            
            if (!_commentExtractor.CanExtract(blockList))
            {
                continue;
            }

            var result = _commentExtractor.Extract(blockList);
            if (result.IsSuccess && result.Definition is not null)
            {
                comments.Add(result.Definition);
            }
        }

        return comments;
    }

    public IReadOnlyList<IndexDefinition> ExtractIndexes(string sqlScript)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlScript);
        
        var blocks = _blockExtractor.Extract(sqlScript);
        var indexes = new List<IndexDefinition>();

        foreach (var block in blocks)
        {
            if (!_indexExtractor.CanExtract(block))
            {
                continue;
            }

            var indexDef = _indexExtractor.Extract(block);
            if (indexDef is not null)
            {
                indexes.Add(indexDef);
            }
        }

        return indexes;
    }

    public IReadOnlyList<TriggerDefinition> ExtractTriggers(string sqlScript)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlScript);
        
        var blocks = _blockExtractor.Extract(sqlScript);
        var triggers = new List<TriggerDefinition>();

        foreach (var block in blocks)
        {
            if (!_triggerExtractor.CanExtract(block))
            {
                continue;
            }

            var triggerDef = _triggerExtractor.Extract(block);
            if (triggerDef is not null)
            {
                triggers.Add(triggerDef);
            }
        }

        return triggers;
    }

    public IReadOnlyList<ConstraintDefinition> ExtractConstraints(string sqlScript)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sqlScript);
        
        var blocks = _blockExtractor.Extract(sqlScript);
        var constraints = new List<ConstraintDefinition>();

        foreach (var block in blocks)
        {
            if (!_constraintExtractor.CanExtract(block))
            {
                continue;
            }

            var constraintDef = _constraintExtractor.Extract(block);
            if (constraintDef is not null)
            {
                constraints.Add(constraintDef);
            }
        }

        return constraints;
    }

    /// <summary>
    /// Анализирует SQL блоки и создает метаданные схемы
    /// </summary>
    /// <param name="blocks">Список SQL блоков для анализа</param>
    /// <param name="sourcePaths">Пути к файлам-источникам</param>
    /// <returns>Метаданные схемы базы данных</returns>
    private SchemaMetadata AnalyzeBlocks(IEnumerable<SqlBlock> blocks, IReadOnlyList<string> sourcePaths)
    {
        var tables = new List<TableDefinition>();
        var views = new List<ViewDefinition>();
        var enums = new List<EnumTypeDefinition>();
        var composites = new List<CompositeTypeDefinition>();
        var domains = new List<DomainTypeDefinition>();
        var functions = new List<FunctionDefinition>();
        var indexes = new List<IndexDefinition>();
        var triggers = new List<TriggerDefinition>();
        var constraints = new List<ConstraintDefinition>();
        var comments = new List<CommentDefinition>();

        var blocksList = blocks.ToList();

        for (int i = 0; i < blocksList.Count; i++)
        {
            var block = blocksList[i];
            
            // Определяем тип объекта в блоке
            var objectType = SchemaObjectDetector.DetectObjectType(block.Content);

            switch (objectType)
            {
                case SchemaObjectType.Tables:
                    // Извлекаем таблицу
                    if (_tableExtractor.CanExtract(block))
                    {
                        var tableDef = _tableExtractor.Extract(block);
                        if (tableDef is not null)
                        {
                            tables.Add(tableDef);
                        }
                    }
                    break;

                case SchemaObjectType.Views:
                    // Извлекаем представление - передаем все блоки начиная с текущего
                    if (_viewExtractor.CanExtract([block]))
                    {
                        var blocksToProcess = blocksList.Skip(i).ToList();
                        var viewDef = _viewExtractor.Extract(blocksToProcess);
                        if (viewDef is not null)
                        {
                            views.Add(viewDef);
                        }
                    }
                    break;

                case SchemaObjectType.Types:
                    // Пытаемся извлечь ENUM
                    var blockList = new[] { block };
                    if (_enumExtractor.CanExtract(blockList))
                    {
                        var result = _enumExtractor.Extract(blockList);
                        if (result.IsSuccess && result.Definition is not null)
                        {
                            enums.Add(result.Definition);
                        }
                    }
                    // Пытаемся извлечь Composite
                    else if (_compositeExtractor.CanExtract(block))
                    {
                        var compositeDef = _compositeExtractor.Extract(block);
                        if (compositeDef is not null)
                        {
                            composites.Add(compositeDef);
                        }
                    }
                    // Пытаемся извлечь Domain
                    else
                    {
                        var domainBlocks = new[] { block };
                        var domainResult = _domainExtractor.Extract(domainBlocks);
                        if (domainResult.IsSuccess && domainResult.Definition is not null)
                        {
                            domains.Add(domainResult.Definition);
                        }
                    }
                    break;

                case SchemaObjectType.Functions:
                    // Извлекаем функцию или процедуру
                    var functionBlocks = new[] { block };
                    var functionResult = _functionExtractor.Extract(functionBlocks);
                    if (functionResult.IsSuccess && functionResult.Definition is not null)
                    {
                        functions.Add(functionResult.Definition);
                    }
                    break;

                case SchemaObjectType.Indexes:
                    // Извлекаем индекс
                    if (_indexExtractor.CanExtract(block))
                    {
                        var indexDef = _indexExtractor.Extract(block);
                        if (indexDef is not null)
                        {
                            indexes.Add(indexDef);
                        }
                    }
                    break;

                case SchemaObjectType.Comments:
                    // Извлекаем комментарий (универсальный)
                    var commentBlocks = new[] { block };
                    if (_commentExtractor.CanExtract(commentBlocks))
                    {
                        var commentResult = _commentExtractor.Extract(commentBlocks);
                        if (commentResult.IsSuccess && commentResult.Definition is not null)
                        {
                            comments.Add(commentResult.Definition);
                        }
                    }
                    break;

                case SchemaObjectType.Triggers:
                    // Извлекаем триггер
                    if (_triggerExtractor.CanExtract(block))
                    {
                        var triggerDef = _triggerExtractor.Extract(block);
                        if (triggerDef is not null)
                        {
                            triggers.Add(triggerDef);
                        }
                    }
                    break;

                case SchemaObjectType.Constraints:
                    // Извлекаем ограничение целостности
                    if (_constraintExtractor.CanExtract(block))
                    {
                        var constraintDef = _constraintExtractor.Extract(block);
                        if (constraintDef is not null)
                        {
                            constraints.Add(constraintDef);
                        }
                    }
                    break;

                case SchemaObjectType.None:
                default:
                    // Пока не реализовано
                    break;
            }
        }

        return new SchemaMetadata
        {
            Tables = tables,
            Views = views,
            Enums = enums,
            Composites = composites,
            Domains = domains,
            Functions = functions,
            Indexes = indexes,
            Triggers = triggers,
            Constraints = constraints,
            Partitions = [],
            CommentDefinition = comments,
            ValidationIssues = [],
            SourcePaths = sourcePaths,
            AnalyzedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Создает пустую метаданные схемы
    /// </summary>
    private static SchemaMetadata CreateEmptyMetadata(IReadOnlyList<string> sourcePaths, string? directoryPath = null)
    {
        var paths = directoryPath is not null 
            ? new[] { directoryPath } 
            : sourcePaths;

        return new SchemaMetadata
        {
            Tables = [],
            Views = [],
            Enums = [],
            Composites = [],
            Domains = [],
            Functions = [],
            Indexes = [],
            Triggers = [],
            Constraints = [],
            Partitions = [],
            CommentDefinition = [],
            ValidationIssues = [],
            SourcePaths = paths,
            AnalyzedAt = DateTime.UtcNow
        };
    }
}
