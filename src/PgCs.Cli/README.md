# PgCs CLI - PostgreSQL C# Code Generator

Professional command-line tool for generating type-safe C# code from PostgreSQL schemas and SQL queries.

## Features

- ✅ Generate C# classes from PostgreSQL DDL schema files
- ✅ Generate repository pattern code from annotated SQL queries  
- ✅ Type-safe query parameters and result models
- ✅ YAML-based configuration with intelligent defaults
- ✅ Modern C# 14 and .NET 9
- ✅ Clean, readable output with color support
- ✅ Dry-run mode for previewing changes
- ✅ Comprehensive validation and error reporting

## Installation

### From Source

```bash
dotnet build src/PgCs.Cli/PgCs.Cli.csproj -c Release
dotnet pack src/PgCs.Cli/PgCs.Cli.csproj -c Release
dotnet tool install --global --add-source ./src/PgCs.Cli/bin/Release pgcs
```

### Run Directly

```bash
dotnet run --project src/PgCs.Cli/PgCs.Cli.csproj -- [command] [options]
```

## Quick Start

### 1. Initialize Configuration

Create a new configuration file:

```bash
pgcs init --minimal
```

This creates a `config.yml` file with minimal settings:

```yaml
schema:
  input:
    file: "./schema.sql"
  output:
    directory: "./Generated/Schema"
    namespace: "Generated.Schema"

queries:
  input:
    file: "./queries.sql"
  output:
    directory: "./Generated/Queries"
    namespace: "Generated.Queries"
```

### 2. Validate Configuration

```bash
pgcs validate
```

### 3. Generate Code

Generate both schema and queries:

```bash
pgcs generate
```

Or generate separately:

```bash
pgcs generate schema
pgcs generate queries
```

## Commands

### `init` - Initialize Configuration

Create a new configuration file with sensible defaults.

```bash
pgcs init [options]
```

**Options:**
- `-o, --output <path>` - Output configuration file path (default: `config.yml`)
- `-m, --minimal` - Create minimal configuration
- `-f, --force` - Overwrite existing file without confirmation
- `--schema-input <path>` - Path to schema SQL file
- `--schema-output <path>` - Output directory for schema classes
- `--queries-input <path>` - Path to queries SQL file
- `--queries-output <path>` - Output directory for query repositories

**Examples:**

```bash
# Create minimal config
pgcs init --minimal

# Create config with custom paths
pgcs init --schema-input ./db/schema.sql --schema-output ./src/Models

# Overwrite existing config
pgcs init --force
```

### `validate` - Validate Configuration

Validate your configuration file for errors and warnings.

```bash
pgcs validate [options]
```

**Options:**
- `-c, --config <path>` - Path to configuration file (default: `config.yml`)
- `--strict` - Treat warnings as errors

**Examples:**

```bash
# Validate default config
pgcs validate

# Validate custom config
pgcs validate --config my-config.yml

# Strict validation
pgcs validate --strict
```

### `generate` - Generate Code

Generate C# code from PostgreSQL schema and queries.

#### `generate schema` - Generate Schema Classes

Generate C# classes from PostgreSQL DDL schema files.

```bash
pgcs generate schema [options]
```

**Options:**
- `-c, --config <path>` - Configuration file path (default: `config.yml`)
- `-i, --input <path>` - Schema SQL file or directory (overrides config)
- `-o, --output <path>` - Output directory (overrides config)
- `--dry-run` - Show what would be generated without writing files
- `-f, --force` - Overwrite existing files without confirmation
- `-v, --verbose` - Enable verbose output

**Examples:**

```bash
# Generate from config
pgcs generate schema

# Override input
pgcs generate schema --input ./db/schema.sql

# Dry run to preview
pgcs generate schema --dry-run

# Force overwrite
pgcs generate schema --force
```

#### `generate queries` - Generate Query Repositories

Generate C# repository code from annotated SQL queries.

```bash
pgcs generate queries [options]
```

**Options:**
- `-c, --config <path>` - Configuration file path (default: `config.yml`)
- `-i, --input <path>` - Query SQL file or directory (overrides config)
- `-o, --output <path>` - Output directory (overrides config)
- `--dry-run` - Show what would be generated without writing files
- `-f, --force` - Overwrite existing files without confirmation
- `-v, --verbose` - Enable verbose output

**Examples:**

```bash
# Generate from config
pgcs generate queries

# Override input and output
pgcs generate queries --input ./sql/queries.sql --output ./src/Data

# Preview without writing
pgcs generate queries --dry-run
```

#### `generate` - Generate Everything

Generate both schema classes and query repositories in one command.

```bash
pgcs generate [options]
```

**Options:**
- `-c, --config <path>` - Configuration file path (default: `config.yml`)
- `--schema-only` - Generate only schema (skip queries)
- `--queries-only` - Generate only queries (skip schema)
- `--dry-run` - Show what would be generated without writing files
- `-f, --force` - Overwrite existing files without confirmation
- `-v, --verbose` - Enable verbose output

**Examples:**

```bash
# Generate everything
pgcs generate

# Generate only schema
pgcs generate --schema-only

# Generate only queries
pgcs generate --queries-only

# Preview all changes
pgcs generate --dry-run
```

## Configuration

The configuration file (`config.yml`) controls all aspects of code generation.

### Minimal Configuration

```yaml
schema:
  input:
    file: "./schema.sql"
  output:
    directory: "./Generated/Schema"
    namespace: "Generated.Schema"

queries:
  input:
    file: "./queries.sql"
  output:
    directory: "./Generated/Queries"
    namespace: "Generated.Queries"
```

### Full Configuration

See `Examples/Configurations/full_config.yml` for all available options including:

- **Schema Generation**: Table filtering, naming conventions, type mappings
- **Query Generation**: Repository patterns, method naming, async/await
- **Formatting**: Indentation, line endings, using directives
- **Output**: Backup creation, dry-run mode, overwrite behavior
- **Logging**: Console and file logging with configurable levels
- **Advanced**: Parallel processing, caching, memory optimization

## Examples

### Generate Schema from PostgreSQL DDL

**Input** (`schema.sql`):
```sql
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE TYPE user_role AS ENUM ('admin', 'user', 'guest');
```

**Output** (`Users.cs`):
```csharp
namespace Generated.Schema;

public class Users
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
}

public enum UserRole
{
    Admin,
    User,
    Guest
}
```

### Generate Repository from SQL Queries

**Input** (`queries.sql`):
```sql
-- name: GetUserById
-- returns: single
SELECT id, username, email, created_at
FROM users
WHERE id = @id;

-- name: SearchUsers
-- returns: list
SELECT id, username, email
FROM users
WHERE username LIKE @searchTerm || '%'
ORDER BY username;
```

**Output** (`IUserRepository.cs` and `UserRepository.cs`):
```csharp
public interface IUserRepository
{
    Task<GetUserByIdResult?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<SearchUsersResult>> SearchUsersAsync(string searchTerm, CancellationToken cancellationToken = default);
}

public record GetUserByIdResult(int Id, string Username, string Email, DateTime? CreatedAt);
public record SearchUsersResult(int Id, string Username, string Email);
```

## Environment Variables

- `PGCS_DEBUG=1` - Show detailed stack traces on errors

## Common Options

All commands support these common options:

- `-c, --config <path>` - Path to configuration file (default: `config.yml`)
- `-v, --verbose` - Enable verbose output
- `--no-color` - Disable colored output

## Error Handling

PgCs provides clear, actionable error messages:

```bash
$ pgcs generate schema --input missing.sql

✗ File not found: missing.sql

Suggestions:
  • Ensure the file exists and is readable
  • Use 'pgcs init' to create a new configuration file
  • Specify a different file with --config
```

## Tips

1. **Use dry-run mode** to preview changes before writing:
   ```bash
   pgcs generate --dry-run
   ```

2. **Validate config** before generating:
   ```bash
   pgcs validate && pgcs generate
   ```

3. **Enable verbose output** for debugging:
   ```bash
   pgcs generate --verbose
   ```

4. **Create backups** automatically by setting in config:
   ```yaml
   output:
     createBackups: true
     backupDirectory: ./.backups
   ```

## Development Status

✅ **Implemented:**
- CLI structure with System.CommandLine
- Configuration loading and validation (YAML)
- Beautiful console output with colors
- All commands: `init`, `validate`, `generate`
- Error handling and reporting
- Help system and documentation

🔄 **In Progress:**
- Integration with `CodeGenerationPipeline`
- Actual schema and query generation
- File writing with backup support

📋 **Planned:**
- Source generator for AOT compilation support
- Performance optimizations
- Additional output formats
- Plugin system

## Contributing

Contributions are welcome! Please ensure:

1. All tests pass: `dotnet test`
2. Code follows C# conventions
3. One feature per pull request
4. Clear commit messages

## License

[Your License Here]

## Support

- GitHub Issues: [link]
- Documentation: [link]
- Examples: `Examples/` directory
