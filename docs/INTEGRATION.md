# Integration Guide: CodeGenerationPipeline

## Overview

This guide explains how to integrate the CLI with the existing `CodeGenerationPipeline.cs`.

## Current Status

✅ **Completed:**
- CLI infrastructure (commands, configuration, output)
- Configuration models that map to pipeline options
- Service layer with placeholder implementations

🔄 **Needs Integration:**
- Connect services to `CodeGenerationPipeline`
- Map YAML configuration to pipeline options
- Implement actual file generation

## Integration Points

### 1. SchemaGenerationService

**Location:** `src/PgCs.Cli/Services/SchemaGenerationService.cs`

**Current State:**
```csharp
public async Task<Result> GenerateAsync(
    PgCsConfiguration config,
    CancellationToken cancellationToken = default)
{
    // TODO: Implement actual integration with CodeGenerationPipeline
    // Placeholder implementation
}
```

**Integration Steps:**

```csharp
public async Task<Result> GenerateAsync(
    PgCsConfiguration config,
    CancellationToken cancellationToken = default)
{
    if (config.Schema is null)
        throw new InvalidOperationException("Schema configuration is missing");

    var warnings = new List<string>();
    var filesCreated = new List<string>();

    // Create pipeline
    var pipeline = new CodeGenerationPipeline();

    // 1. Configure input source
    if (!string.IsNullOrEmpty(config.Schema.Input.File))
    {
        pipeline.FromSchemaFile(config.Schema.Input.File);
    }
    else if (!string.IsNullOrEmpty(config.Schema.Input.Directory))
    {
        pipeline.FromSchemaDirectory(
            config.Schema.Input.Directory,
            config.Schema.Input.Pattern,
            config.Schema.Input.Recursive
        );
    }

    // 2. Apply schema filters
    if (config.Schema.Filter is not null)
    {
        pipeline.FilterSchema(filter =>
        {
            if (config.Schema.Filter.Schemas.Count > 0)
                filter.IncludeSchemas = config.Schema.Filter.Schemas;
            
            if (config.Schema.Filter.ExcludeSchemas.Count > 0)
                filter.ExcludeSchemas = config.Schema.Filter.ExcludeSchemas;
            
            if (config.Schema.Filter.Tables.Count > 0)
                filter.IncludeTables = config.Schema.Filter.Tables;
            
            if (config.Schema.Filter.ExcludeTables.Count > 0)
                filter.ExcludeTables = config.Schema.Filter.ExcludeTables;
        });
    }

    // 3. Configure schema generation
    pipeline.WithSchemaGeneration(options =>
    {
        // Output
        options.OutputDirectory = config.Schema.Output.Directory;
        options.Namespace = config.Schema.Output.Namespace;
        options.FilePerTable = config.Schema.Output.FilePerTable;
        options.SeparateEnums = config.Schema.Output.SeparateEnums;

        // Generation options
        options.GenerateClasses = config.Schema.Generation.GenerateClasses;
        options.GenerateRecords = config.Schema.Generation.GenerateRecords;
        options.GenerateEnums = config.Schema.Generation.GenerateEnums;
        options.GenerateInterfaces = config.Schema.Generation.GenerateInterfaces;
        options.GenerateConstructors = config.Schema.Generation.GenerateConstructors;
        options.GenerateProperties = config.Schema.Generation.GenerateProperties;
        options.GenerateComments = config.Schema.Generation.GenerateComments;
        options.GenerateAttributes = config.Schema.Generation.GenerateAttributes;

        // Naming
        options.NamingConvention = config.Schema.Naming.Convention;
        options.Pluralize = config.Schema.Naming.Pluralize;
        options.Prefix = config.Schema.Naming.Prefix;
        options.Suffix = config.Schema.Naming.Suffix;
        options.RemoveUnderscores = config.Schema.Naming.RemoveUnderscores;

        // Types
        options.UseNullableReferenceTypes = config.Schema.Types.UseNullableReferenceTypes;
        options.UseSystemJsonAttributes = config.Schema.Types.UseSystemJsonAttributes;
        options.CustomTypeMappings = config.Schema.Types.CustomMappings;

        // Formatting
        options.Indentation = config.Formatting.Indentation == "tabs" ? "\t" : new string(' ', config.Formatting.IndentSize);
        options.LineEnding = config.Formatting.LineEndings == "crlf" ? "\r\n" : "\n";
        options.FileScopedNamespaces = config.Formatting.Usings.FileScoped;
        options.SortUsings = config.Formatting.Usings.Sort;

        // Output behavior
        options.OverwriteExisting = config.Output.OverwriteExisting;
        options.CreateBackups = config.Output.CreateBackups;
        options.BackupDirectory = config.Output.BackupDirectory;
        options.DryRun = config.Output.DryRun;
    });

    // 4. Execute pipeline
    var result = await pipeline.ExecuteAsync(cancellationToken);

    // 5. Collect results
    return new Result(
        TablesGenerated: result.TablesGenerated,
        EnumsGenerated: result.EnumsGenerated,
        FilesCreated: result.FilesCreated.ToList(),
        Warnings: result.Warnings.ToList()
    );
}
```

### 2. QueryGenerationService

**Location:** `src/PgCs.Cli/Services/QueryGenerationService.cs`

**Integration Steps:**

```csharp
public async Task<Result> GenerateAsync(
    PgCsConfiguration config,
    CancellationToken cancellationToken = default)
{
    if (config.Queries is null)
        throw new InvalidOperationException("Queries configuration is missing");

    var warnings = new List<string>();
    var filesCreated = new List<string>();

    // Create pipeline
    var pipeline = new CodeGenerationPipeline();

    // 1. Configure input source
    if (!string.IsNullOrEmpty(config.Queries.Input.File))
    {
        pipeline.FromQueryFile(config.Queries.Input.File);
    }
    else if (!string.IsNullOrEmpty(config.Queries.Input.Directory))
    {
        pipeline.FromQueryDirectory(
            config.Queries.Input.Directory,
            config.Queries.Input.Pattern,
            config.Queries.Input.Recursive
        );
    }

    // 2. Configure query generation
    pipeline.WithQueryGeneration(options =>
    {
        // Output
        options.OutputDirectory = config.Queries.Output.Directory;
        options.Namespace = config.Queries.Output.Namespace;
        options.RepositoryPerFile = config.Queries.Output.RepositoryPerFile;
        options.SeparateModels = config.Queries.Output.SeparateModels;
        options.ModelsDirectory = config.Queries.Output.ModelsDirectory;

        // Repositories
        options.GenerateInterfaces = config.Queries.Repositories.GenerateInterfaces;
        options.GenerateImplementations = config.Queries.Repositories.GenerateImplementations;
        options.BaseClass = config.Queries.Repositories.BaseClass;
        options.RepositoryNameSuffix = config.Queries.Repositories.NameSuffix;

        // Methods
        options.AsyncMethods = config.Queries.Methods.Async;
        options.CancellationToken = config.Queries.Methods.CancellationToken;
        options.GenerateMethodComments = config.Queries.Methods.GenerateComments;
        options.MethodNamingConvention = config.Queries.Methods.NamingConvention;

        // Models
        options.GenerateRecords = config.Queries.Models.GenerateRecords;
        options.GenerateClasses = config.Queries.Models.GenerateClasses;
        options.UseNullableReferenceTypes = config.Queries.Models.NullableReferenceTypes;
        options.GenerateModelComments = config.Queries.Models.GenerateComments;

        // Connection
        options.ConnectionType = config.Queries.Connection.Type;
        options.InjectConnection = config.Queries.Connection.InjectConnection;
        options.ConnectionParameterName = config.Queries.Connection.ConnectionParameterName;

        // Error handling
        options.WrapExceptions = config.Queries.ErrorHandling.WrapExceptions;
        options.LogErrors = config.Queries.ErrorHandling.LogErrors;
        options.GenerateTryCatch = config.Queries.ErrorHandling.GenerateTryCatch;

        // Formatting (reuse from schema)
        options.Indentation = config.Formatting.Indentation == "tabs" ? "\t" : new string(' ', config.Formatting.IndentSize);
        options.LineEnding = config.Formatting.LineEndings == "crlf" ? "\r\n" : "\n";
        options.FileScopedNamespaces = config.Formatting.Usings.FileScoped;
        options.SortUsings = config.Formatting.Usings.Sort;

        // Output behavior
        options.OverwriteExisting = config.Output.OverwriteExisting;
        options.CreateBackups = config.Output.CreateBackups;
        options.BackupDirectory = config.Output.BackupDirectory;
        options.DryRun = config.Output.DryRun;
    });

    // 3. Execute pipeline
    var result = await pipeline.ExecuteAsync(cancellationToken);

    // 4. Collect results
    return new Result(
        RepositoriesGenerated: result.RepositoriesGenerated,
        MethodsGenerated: result.MethodsGenerated,
        ModelsGenerated: result.ModelsGenerated,
        FilesCreated: result.FilesCreated.ToList(),
        Warnings: result.Warnings.ToList()
    );
}
```

### 3. Update Commands to Use Services

#### GenerateSchemaCommand

**Location:** `src/PgCs.Cli/Commands/GenerateSchemaCommand.cs`

Replace the placeholder section (lines with `await Task.Delay(100)`) with:

```csharp
// Create service
var service = new SchemaGenerationService();

// Generate
var result = await service.GenerateAsync(config, cancellationToken);

// Handle warnings
if (result.Warnings.Count > 0)
{
    foreach (var warning in result.Warnings)
    {
        Writer.Warning(warning);
    }
}

// Print results
if (dryRun)
{
    var resultPrinter = new ResultPrinter(Writer);
    resultPrinter.PrintDryRunResult(
        result.FilesCreated.Count,
        0, // files that would be overwritten (calculate from existing files)
        config.Schema.Output.Directory
    );
}
else
{
    progress.Complete("Schema generation");
    
    var resultPrinter = new ResultPrinter(Writer);
    resultPrinter.PrintSchemaResult(
        result.TablesGenerated,
        result.EnumsGenerated,
        config.Schema.Output.Directory,
        stopwatch.Elapsed
    );
}
```

#### GenerateQueriesCommand

**Location:** `src/PgCs.Cli/Commands/GenerateQueriesCommand.cs`

Replace the placeholder section with:

```csharp
// Create service
var service = new QueryGenerationService();

// Generate
var result = await service.GenerateAsync(config, cancellationToken);

// Handle warnings
if (result.Warnings.Count > 0)
{
    foreach (var warning in result.Warnings)
    {
        Writer.Warning(warning);
    }
}

// Print results
if (dryRun)
{
    var resultPrinter = new ResultPrinter(Writer);
    resultPrinter.PrintDryRunResult(
        result.FilesCreated.Count,
        0,
        config.Queries.Output.Directory
    );
}
else
{
    progress.Complete("Query generation");
    
    var resultPrinter = new ResultPrinter(Writer);
    resultPrinter.PrintQueryResult(
        result.RepositoriesGenerated,
        result.MethodsGenerated,
        result.ModelsGenerated,
        config.Queries.Output.Directory,
        stopwatch.Elapsed
    );
}
```

#### GenerateAllCommand

**Location:** `src/PgCs.Cli/Commands/GenerateAllCommand.cs`

Replace both schema and queries placeholder sections with the service calls above.

## Required Changes to CodeGenerationPipeline

If `CodeGenerationPipeline` doesn't have all these methods/options, you'll need to add:

### 1. Add CancellationToken Support

```csharp
public async Task<PipelineResult> ExecuteAsync(CancellationToken cancellationToken = default)
{
    // Pass cancellation token to all async operations
}
```

### 2. Add Result Model

```csharp
public record PipelineResult(
    int TablesGenerated,
    int EnumsGenerated,
    int RepositoriesGenerated,
    int MethodsGenerated,
    int ModelsGenerated,
    IReadOnlyList<string> FilesCreated,
    IReadOnlyList<string> Warnings
);
```

### 3. Add Configuration Delegates

```csharp
public CodeGenerationPipeline WithSchemaGeneration(Action<SchemaGenerationOptions> configure)
{
    var options = new SchemaGenerationOptions();
    configure(options);
    // Apply options
    return this;
}

public CodeGenerationPipeline WithQueryGeneration(Action<QueryGenerationOptions> configure)
{
    var options = new QueryGenerationOptions();
    configure(options);
    // Apply options
    return this;
}
```

## Testing Integration

### 1. Create Test SQL Files

**schema.sql:**
```sql
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL,
    email VARCHAR(255) NOT NULL
);

CREATE TYPE user_role AS ENUM ('admin', 'user');
```

**queries.sql:**
```sql
-- name: GetUserById
-- returns: single
SELECT id, username, email FROM users WHERE id = @id;

-- name: ListUsers
-- returns: list
SELECT id, username, email FROM users;
```

### 2. Test Commands

```bash
# Initialize
pgcs init --minimal

# Edit config.yml to point to test files
# schema.input.file: ./schema.sql
# queries.input.file: ./queries.sql

# Validate
pgcs validate

# Generate (dry run first)
pgcs generate all --dry-run --verbose

# Generate for real
pgcs generate all
```

### 3. Verify Output

```bash
# Check generated files
ls -R ./Generated/

# Verify C# compilation
dotnet build ./Generated/
```

## Checklist

- [ ] Update `SchemaGenerationService.GenerateAsync()`
- [ ] Update `QueryGenerationService.GenerateAsync()`
- [ ] Update `GenerateSchemaCommand` to use service
- [ ] Update `GenerateQueriesCommand` to use service
- [ ] Update `GenerateAllCommand` to use services
- [ ] Add `CancellationToken` support to `CodeGenerationPipeline`
- [ ] Add `PipelineResult` record to `CodeGenerationPipeline`
- [ ] Add configuration delegates to `CodeGenerationPipeline`
- [ ] Test with real SQL files
- [ ] Verify generated C# compiles
- [ ] Update tests to cover integrated functionality

## Notes

- Keep the placeholder implementations as fallback/example
- Add proper error handling for file I/O operations
- Consider adding progress callbacks to pipeline for real-time updates
- Ensure proper disposal of resources (file handles, etc.)
- Add integration tests for end-to-end scenarios
