using YamlDotNet.Serialization;

namespace PgCs.Cli.Configuration;

/// <summary>
/// Root configuration model for PgCs CLI
/// </summary>
public sealed class PgCsConfiguration
{
    [YamlMember(Alias = "schema")]
    public SchemaConfiguration? Schema { get; set; }

    [YamlMember(Alias = "queries")]
    public QueriesConfiguration? Queries { get; set; }

    [YamlMember(Alias = "formatting")]
    public FormattingConfiguration Formatting { get; set; } = new();

    [YamlMember(Alias = "output")]
    public OutputConfiguration Output { get; set; } = new();

    [YamlMember(Alias = "logging")]
    public LoggingConfiguration Logging { get; set; } = new();

    [YamlMember(Alias = "advanced")]
    public AdvancedConfiguration Advanced { get; set; } = new();
}

/// <summary>
/// Schema generation configuration
/// </summary>
public sealed class SchemaConfiguration
{
    [YamlMember(Alias = "input")]
    public InputConfiguration Input { get; set; } = new();

    [YamlMember(Alias = "output")]
    public SchemaOutputConfiguration Output { get; set; } = new();

    [YamlMember(Alias = "filter")]
    public FilterConfiguration? Filter { get; set; }

    [YamlMember(Alias = "generation")]
    public SchemaGenerationConfiguration Generation { get; set; } = new();

    [YamlMember(Alias = "naming")]
    public NamingConfiguration Naming { get; set; } = new();

    [YamlMember(Alias = "types")]
    public TypeConfiguration Types { get; set; } = new();

    [YamlMember(Alias = "validation")]
    public ValidationConfiguration Validation { get; set; } = new();
}

/// <summary>
/// Query generation configuration
/// </summary>
public sealed class QueriesConfiguration
{
    [YamlMember(Alias = "input")]
    public InputConfiguration Input { get; set; } = new();

    [YamlMember(Alias = "output")]
    public QueryOutputConfiguration Output { get; set; } = new();

    [YamlMember(Alias = "repositories")]
    public RepositoryConfiguration Repositories { get; set; } = new();

    [YamlMember(Alias = "methods")]
    public MethodConfiguration Methods { get; set; } = new();

    [YamlMember(Alias = "models")]
    public ModelConfiguration Models { get; set; } = new();

    [YamlMember(Alias = "connection")]
    public ConnectionConfiguration Connection { get; set; } = new();

    [YamlMember(Alias = "errorHandling")]
    public ErrorHandlingConfiguration ErrorHandling { get; set; } = new();

    [YamlMember(Alias = "validation")]
    public QueryValidationConfiguration Validation { get; set; } = new();
}

/// <summary>
/// Input source configuration
/// </summary>
public sealed class InputConfiguration
{
    [YamlMember(Alias = "file")]
    public string? File { get; set; }

    [YamlMember(Alias = "directory")]
    public string? Directory { get; set; }

    [YamlMember(Alias = "pattern")]
    public string Pattern { get; set; } = "*.sql";

    [YamlMember(Alias = "recursive")]
    public bool Recursive { get; set; } = true;

    [YamlMember(Alias = "encoding")]
    public string Encoding { get; set; } = "UTF-8";
}

/// <summary>
/// Schema output configuration
/// </summary>
public sealed class SchemaOutputConfiguration
{
    [YamlMember(Alias = "directory")]
    public string Directory { get; set; } = "./Generated/Schema";

    [YamlMember(Alias = "namespace")]
    public string Namespace { get; set; } = "Generated.Schema";

    [YamlMember(Alias = "filePerTable")]
    public bool FilePerTable { get; set; } = true;

    [YamlMember(Alias = "separateEnums")]
    public bool SeparateEnums { get; set; } = false;
}

/// <summary>
/// Query output configuration
/// </summary>
public sealed class QueryOutputConfiguration
{
    [YamlMember(Alias = "directory")]
    public string Directory { get; set; } = "./Generated/Queries";

    [YamlMember(Alias = "namespace")]
    public string Namespace { get; set; } = "Generated.Queries";

    [YamlMember(Alias = "repositoryPerFile")]
    public bool RepositoryPerFile { get; set; } = true;

    [YamlMember(Alias = "separateModels")]
    public bool SeparateModels { get; set; } = true;

    [YamlMember(Alias = "modelsDirectory")]
    public string ModelsDirectory { get; set; } = "Models";
}

/// <summary>
/// Schema filter configuration
/// </summary>
public sealed class FilterConfiguration
{
    [YamlMember(Alias = "schemas")]
    public List<string> Schemas { get; set; } = new();

    [YamlMember(Alias = "tables")]
    public List<string> Tables { get; set; } = new();

    [YamlMember(Alias = "excludeSchemas")]
    public List<string> ExcludeSchemas { get; set; } = new() { "pg_catalog", "information_schema" };

    [YamlMember(Alias = "excludeTables")]
    public List<string> ExcludeTables { get; set; } = new();
}

/// <summary>
/// Schema generation options
/// </summary>
public sealed class SchemaGenerationConfiguration
{
    [YamlMember(Alias = "generateClasses")]
    public bool GenerateClasses { get; set; } = true;

    [YamlMember(Alias = "generateRecords")]
    public bool GenerateRecords { get; set; } = false;

    [YamlMember(Alias = "generateEnums")]
    public bool GenerateEnums { get; set; } = true;

    [YamlMember(Alias = "generateInterfaces")]
    public bool GenerateInterfaces { get; set; } = false;

    [YamlMember(Alias = "generateConstructors")]
    public bool GenerateConstructors { get; set; } = true;

    [YamlMember(Alias = "generateProperties")]
    public bool GenerateProperties { get; set; } = true;

    [YamlMember(Alias = "generateComments")]
    public bool GenerateComments { get; set; } = true;

    [YamlMember(Alias = "generateAttributes")]
    public bool GenerateAttributes { get; set; } = true;
}

/// <summary>
/// Naming convention configuration
/// </summary>
public sealed class NamingConfiguration
{
    [YamlMember(Alias = "convention")]
    public string Convention { get; set; } = "PascalCase";

    [YamlMember(Alias = "pluralize")]
    public bool Pluralize { get; set; } = false;

    [YamlMember(Alias = "prefix")]
    public string? Prefix { get; set; }

    [YamlMember(Alias = "suffix")]
    public string? Suffix { get; set; }

    [YamlMember(Alias = "removeUnderscores")]
    public bool RemoveUnderscores { get; set; } = true;
}

/// <summary>
/// Type mapping configuration
/// </summary>
public sealed class TypeConfiguration
{
    [YamlMember(Alias = "useNullableReferenceTypes")]
    public bool UseNullableReferenceTypes { get; set; } = true;

    [YamlMember(Alias = "useSystemJsonAttributes")]
    public bool UseSystemJsonAttributes { get; set; } = false;

    [YamlMember(Alias = "customMappings")]
    public Dictionary<string, string> CustomMappings { get; set; } = new();
}

/// <summary>
/// Validation configuration
/// </summary>
public sealed class ValidationConfiguration
{
    [YamlMember(Alias = "enabled")]
    public bool Enabled { get; set; } = true;

    [YamlMember(Alias = "strictMode")]
    public bool StrictMode { get; set; } = false;

    [YamlMember(Alias = "warnOnMissingTables")]
    public bool WarnOnMissingTables { get; set; } = true;
}

/// <summary>
/// Repository generation configuration
/// </summary>
public sealed class RepositoryConfiguration
{
    [YamlMember(Alias = "generateInterfaces")]
    public bool GenerateInterfaces { get; set; } = true;

    [YamlMember(Alias = "generateImplementations")]
    public bool GenerateImplementations { get; set; } = true;

    [YamlMember(Alias = "baseClass")]
    public string? BaseClass { get; set; }

    [YamlMember(Alias = "nameSuffix")]
    public string NameSuffix { get; set; } = "Repository";
}

/// <summary>
/// Method generation configuration
/// </summary>
public sealed class MethodConfiguration
{
    [YamlMember(Alias = "async")]
    public bool Async { get; set; } = true;

    [YamlMember(Alias = "cancellationToken")]
    public bool CancellationToken { get; set; } = true;

    [YamlMember(Alias = "generateComments")]
    public bool GenerateComments { get; set; } = true;

    [YamlMember(Alias = "namingConvention")]
    public string NamingConvention { get; set; } = "PascalCase";
}

/// <summary>
/// Model generation configuration
/// </summary>
public sealed class ModelConfiguration
{
    [YamlMember(Alias = "generateRecords")]
    public bool GenerateRecords { get; set; } = true;

    [YamlMember(Alias = "generateClasses")]
    public bool GenerateClasses { get; set; } = false;

    [YamlMember(Alias = "nullableReferenceTypes")]
    public bool NullableReferenceTypes { get; set; } = true;

    [YamlMember(Alias = "generateComments")]
    public bool GenerateComments { get; set; } = true;
}

/// <summary>
/// Connection configuration
/// </summary>
public sealed class ConnectionConfiguration
{
    [YamlMember(Alias = "type")]
    public string Type { get; set; } = "Npgsql";

    [YamlMember(Alias = "injectConnection")]
    public bool InjectConnection { get; set; } = true;

    [YamlMember(Alias = "connectionParameterName")]
    public string ConnectionParameterName { get; set; } = "connection";
}

/// <summary>
/// Error handling configuration
/// </summary>
public sealed class ErrorHandlingConfiguration
{
    [YamlMember(Alias = "wrapExceptions")]
    public bool WrapExceptions { get; set; } = true;

    [YamlMember(Alias = "logErrors")]
    public bool LogErrors { get; set; } = true;

    [YamlMember(Alias = "generateTryCatch")]
    public bool GenerateTryCatch { get; set; } = false;
}

/// <summary>
/// Query validation configuration
/// </summary>
public sealed class QueryValidationConfiguration
{
    [YamlMember(Alias = "enabled")]
    public bool Enabled { get; set; } = true;

    [YamlMember(Alias = "validateSyntax")]
    public bool ValidateSyntax { get; set; } = true;

    [YamlMember(Alias = "warnOnMissingAnnotations")]
    public bool WarnOnMissingAnnotations { get; set; } = true;
}

/// <summary>
/// Code formatting configuration
/// </summary>
public sealed class FormattingConfiguration
{
    [YamlMember(Alias = "indentation")]
    public string Indentation { get; set; } = "spaces";

    [YamlMember(Alias = "indentSize")]
    public int IndentSize { get; set; } = 4;

    [YamlMember(Alias = "lineEndings")]
    public string LineEndings { get; set; } = "lf";

    [YamlMember(Alias = "usings")]
    public UsingsConfiguration Usings { get; set; } = new();
}

/// <summary>
/// Using directives configuration
/// </summary>
public sealed class UsingsConfiguration
{
    [YamlMember(Alias = "fileScoped")]
    public bool FileScoped { get; set; } = true;

    [YamlMember(Alias = "sort")]
    public bool Sort { get; set; } = true;

    [YamlMember(Alias = "removeUnused")]
    public bool RemoveUnused { get; set; } = true;
}

/// <summary>
/// Output behavior configuration
/// </summary>
public sealed class OutputConfiguration
{
    [YamlMember(Alias = "overwriteExisting")]
    public bool OverwriteExisting { get; set; } = false;

    [YamlMember(Alias = "createBackups")]
    public bool CreateBackups { get; set; } = true;

    [YamlMember(Alias = "backupDirectory")]
    public string BackupDirectory { get; set; } = "./.backups";

    [YamlMember(Alias = "dryRun")]
    public bool DryRun { get; set; } = false;
}

/// <summary>
/// Logging configuration
/// </summary>
public sealed class LoggingConfiguration
{
    [YamlMember(Alias = "level")]
    public string Level { get; set; } = "Information";

    [YamlMember(Alias = "console")]
    public bool Console { get; set; } = true;

    [YamlMember(Alias = "file")]
    public string? File { get; set; }

    [YamlMember(Alias = "timestamp")]
    public bool Timestamp { get; set; } = true;

    [YamlMember(Alias = "colors")]
    public bool Colors { get; set; } = true;
}

/// <summary>
/// Advanced options configuration
/// </summary>
public sealed class AdvancedConfiguration
{
    [YamlMember(Alias = "parallelProcessing")]
    public bool ParallelProcessing { get; set; } = true;

    [YamlMember(Alias = "maxDegreeOfParallelism")]
    public int MaxDegreeOfParallelism { get; set; } = -1;

    [YamlMember(Alias = "memoryOptimization")]
    public bool MemoryOptimization { get; set; } = false;

    [YamlMember(Alias = "cacheResults")]
    public bool CacheResults { get; set; } = true;
}
