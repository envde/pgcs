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
    private readonly IEnumExtractor _enumExtractor;

    /// <summary>
    /// Создает новый экземпляр анализатора схемы с инжекцией зависимостей
    /// </summary>
    public SchemaAnalyzer(IBlockExtractor blockExtractor, IEnumExtractor enumExtractor)
    {
        ArgumentNullException.ThrowIfNull(blockExtractor);
        ArgumentNullException.ThrowIfNull(enumExtractor);
        
        _blockExtractor = blockExtractor;
        _enumExtractor = enumExtractor;
    }

    /// <summary>
    /// Создает новый экземпляр анализатора схемы с реализациями по умолчанию
    /// </summary>
    public SchemaAnalyzer() : this(new BlockExtractor(), new EnumExtractor())
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
            if (!_enumExtractor.CanExtract(block))
            {
                continue;
            }

            var enumDef = _enumExtractor.Extract(block);
            if (enumDef is not null)
            {
                enums.Add(enumDef);
            }
        }

        return enums;
    }

    public IReadOnlyList<TableDefinition> ExtractTables(string sqlScript)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<ViewDefinition> ExtractViews(string sqlScript)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<DomainTypeDefinition> ExtractDomains(string sqlScript)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<CompositeTypeDefinition> ExtractComposites(string sqlScript)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<FunctionDefinition> ExtractFunctions(string sqlScript)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<IndexDefinition> ExtractIndexes(string sqlScript)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<TriggerDefinition> ExtractTriggers(string sqlScript)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<ConstraintDefinition> ExtractConstraints(string sqlScript)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Анализирует SQL блоки и создает метаданные схемы
    /// </summary>
    /// <param name="blocks">Список SQL блоков для анализа</param>
    /// <param name="sourcePaths">Пути к файлам-источникам</param>
    /// <returns>Метаданные схемы базы данных</returns>
    private SchemaMetadata AnalyzeBlocks(IEnumerable<SqlBlock> blocks, IReadOnlyList<string> sourcePaths)
    {
        var enums = new List<EnumTypeDefinition>();

        foreach (var block in blocks)
        {
            // Определяем тип объекта в блоке
            var objectType = SchemaObjectDetector.DetectObjectType(block.Content);

            switch (objectType)
            {
                case SchemaObjectType.Types:
                    // Пытаемся извлечь ENUM
                    if (_enumExtractor.CanExtract(block))
                    {
                        var enumDef = _enumExtractor.Extract(block);
                        if (enumDef is not null)
                        {
                            enums.Add(enumDef);
                        }
                    }
                    // TODO: Добавить поддержку Composite и Domain типов
                    break;

                // TODO: Добавить обработку других типов объектов
                case SchemaObjectType.Tables:
                case SchemaObjectType.Views:
                case SchemaObjectType.Functions:
                case SchemaObjectType.Indexes:
                case SchemaObjectType.Triggers:
                case SchemaObjectType.Constraints:
                case SchemaObjectType.Comments:
                case SchemaObjectType.None:
                default:
                    // Пока не реализовано
                    break;
            }
        }

        return new SchemaMetadata
        {
            Tables = [],
            Views = [],
            Enums = enums,
            Composites = [],
            Domains = [],
            Functions = [],
            Indexes = [],
            Triggers = [],
            Constraints = [],
            Partitions = [],
            CompositeTypeComments = [],
            TableComments = [],
            ColumnComments = [],
            IndexComments = [],
            TriggerComments = [],
            FunctionComments = [],
            ConstraintComments = [],
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
            CompositeTypeComments = [],
            TableComments = [],
            ColumnComments = [],
            IndexComments = [],
            TriggerComments = [],
            FunctionComments = [],
            ConstraintComments = [],
            ValidationIssues = [],
            SourcePaths = paths,
            AnalyzedAt = DateTime.UtcNow
        };
    }
}
