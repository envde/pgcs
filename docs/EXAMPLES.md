# PgCs CLI Usage Examples

## Basic Workflow

### 1. Initialize a New Project

```bash
# Create minimal configuration
pgcs init --minimal

# Create with custom paths
pgcs init \
  --schema-input ./database/schema.sql \
  --schema-output ./src/Models \
  --queries-input ./database/queries.sql \
  --queries-output ./src/Repositories
```

### 2. Validate Configuration

```bash
# Validate default config.yml
pgcs validate

# Validate with custom config
pgcs validate --config my-config.yml

# Strict validation (warnings = errors)
pgcs validate --strict
```

### 3. Generate Code

```bash
# Generate everything
pgcs generate all

# Generate only schema
pgcs generate schema

# Generate only queries
pgcs generate queries

# Preview without writing files
pgcs generate all --dry-run
```

## Advanced Scenarios

### Custom Configuration Paths

```bash
# Use custom config file for all commands
pgcs validate --config ./configs/production.yml
pgcs generate all --config ./configs/production.yml
```

### Override Input/Output Paths

```bash
# Override schema input
pgcs generate schema \
  --input ./alternative-schema.sql \
  --output ./Generated/CustomModels

# Override queries paths
pgcs generate queries \
  --input ./sql/custom-queries.sql \
  --output ./src/Data/Repositories
```

### Dry Run Mode

```bash
# Preview schema generation
pgcs generate schema --dry-run

# Preview everything
pgcs generate all --dry-run --verbose
```

### Force Overwrite

```bash
# Skip confirmation prompts
pgcs generate all --force

# Force with custom config
pgcs generate schema --config prod.yml --force
```

### Verbose Output

```bash
# Enable verbose logging
pgcs generate all --verbose

# Verbose with custom config
pgcs validate --config dev.yml --verbose
```

## Real-World Examples

### Example 1: E-commerce Database

**Directory Structure:**
```
my-project/
├── database/
│   ├── schema.sql        # Tables, enums, types
│   └── queries.sql       # SQL queries with annotations
├── src/
│   ├── Models/           # Generated schema classes
│   └── Repositories/     # Generated query repositories
└── config.yml
```

**Workflow:**

```bash
# 1. Initialize with custom paths
pgcs init \
  --schema-input ./database/schema.sql \
  --schema-output ./src/Models \
  --queries-input ./database/queries.sql \
  --queries-output ./src/Repositories \
  --output config.yml

# 2. Edit config.yml to customize generation options
# (add filters, naming conventions, etc.)

# 3. Validate configuration
pgcs validate

# 4. Preview changes
pgcs generate all --dry-run

# 5. Generate code
pgcs generate all

# 6. Regenerate after schema changes
pgcs generate schema --force
```

### Example 2: Microservice with Multiple Schemas

**config.yml:**
```yaml
schema:
  input:
    directory: "./database/schemas"
    pattern: "*.sql"
    recursive: true
  output:
    directory: "./src/Domain/Models"
    namespace: "MyService.Domain.Models"
    filePerTable: true
  filter:
    excludeSchemas:
      - "pg_catalog"
      - "information_schema"
      - "audit"

queries:
  input:
    directory: "./database/queries"
    pattern: "*.sql"
    recursive: true
  output:
    directory: "./src/Infrastructure/Data"
    namespace: "MyService.Infrastructure.Data"
    repositoryPerFile: true
    separateModels: true
```

**Commands:**

```bash
# Validate multi-schema config
pgcs validate --config config.yml

# Generate with verbose output
pgcs generate all --verbose

# Regenerate only queries after adding new SQL files
pgcs generate queries --force
```

### Example 3: CI/CD Pipeline

**.github/workflows/generate-code.yml:**
```yaml
name: Generate Database Code

on:
  push:
    paths:
      - 'database/**'
      - 'config.yml'

jobs:
  generate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Install PgCs CLI
        run: dotnet tool install --global pgcs
      
      - name: Validate Configuration
        run: pgcs validate --strict
      
      - name: Generate Code
        run: pgcs generate all --force --verbose
      
      - name: Commit Generated Code
        run: |
          git config user.name "GitHub Actions"
          git config user.email "actions@github.com"
          git add src/
          git commit -m "chore: regenerate database code" || echo "No changes"
          git push
```

**Local Commands:**

```bash
# Before committing changes, validate
pgcs validate --strict

# Generate and review changes
pgcs generate all --dry-run
pgcs generate all

# Commit
git add src/ config.yml
git commit -m "feat: update database schema"
```

### Example 4: Multiple Environments

**Directory Structure:**
```
configs/
├── config.development.yml
├── config.staging.yml
└── config.production.yml
```

**Commands:**

```bash
# Development
pgcs generate all --config configs/config.development.yml

# Staging
pgcs generate all --config configs/config.staging.yml --verbose

# Production (with validation and dry-run first)
pgcs validate --config configs/config.production.yml --strict
pgcs generate all --config configs/config.production.yml --dry-run
pgcs generate all --config configs/config.production.yml --force
```

## Troubleshooting

### Configuration Errors

```bash
# Enable debug mode for detailed errors
PGCS_DEBUG=1 pgcs validate

# Check file paths
ls -la database/*.sql

# Validate YAML syntax
cat config.yml | yaml-lint  # if you have yaml-lint installed
```

### File Not Found

```bash
# Verify paths in config
cat config.yml | grep -A 5 "input:"

# Check current directory
pwd

# Use absolute paths
pgcs generate schema --input /absolute/path/to/schema.sql
```

### Permission Errors

```bash
# Check output directory permissions
ls -ld ./Generated

# Create directory manually
mkdir -p ./Generated/Schema ./Generated/Queries

# Check write permissions
touch ./Generated/test.txt && rm ./Generated/test.txt
```

### Overwrite Protection

```bash
# Preview what will be overwritten
pgcs generate all --dry-run

# Force overwrite if intended
pgcs generate all --force

# Or answer 'yes' to confirmation prompt
pgcs generate all
# Do you want to overwrite existing files? (y/N): y
```

## Tips & Tricks

### 1. Use Dry Run Before Generating

```bash
# Always preview first
pgcs generate all --dry-run
```

### 2. Validate Before Committing

```bash
# Pre-commit validation
pgcs validate --strict && pgcs generate all
```

### 3. Use Verbose Mode for Debugging

```bash
# See detailed output
pgcs generate all --verbose
```

### 4. Create Minimal Config First

```bash
# Start simple, then customize
pgcs init --minimal
# Edit config.yml
pgcs validate
pgcs generate all
```

### 5. Use Environment Variables

```bash
# Debug mode
export PGCS_DEBUG=1
pgcs generate all

# Custom config path
export PGCS_CONFIG=./configs/dev.yml
pgcs validate
pgcs generate all
```

### 6. Separate Schema and Query Generation

```bash
# Generate schema first
pgcs generate schema

# Review generated classes
ls -R ./Generated/Schema

# Then generate queries
pgcs generate queries
```

### 7. Use Different Configs for Different Purposes

```bash
# Minimal config for quick prototypes
pgcs init --minimal --output config.minimal.yml

# Full config for production
pgcs init --output config.production.yml

# Use as needed
pgcs generate all --config config.minimal.yml
pgcs generate all --config config.production.yml
```

## Common Command Patterns

```bash
# Quick start
pgcs init --minimal && pgcs generate all

# Safe generation
pgcs validate && pgcs generate all --dry-run && pgcs generate all

# Force regeneration
pgcs generate all --force --verbose

# Schema only
pgcs generate schema --force

# Queries only
pgcs generate queries --force

# Different configs
pgcs generate all --config dev.yml
pgcs generate all --config prod.yml

# Override paths
pgcs generate schema --input ./db/schema.sql --output ./models

# CI/CD friendly
pgcs validate --strict && pgcs generate all --force --no-color
```

## Integration with Build Tools

### MSBuild (csproj)

```xml
<Target Name="GenerateCode" BeforeTargets="BeforeBuild">
  <Exec Command="pgcs validate --strict" />
  <Exec Command="pgcs generate all --force" />
</Target>
```

### Make

```makefile
.PHONY: generate
generate:
	pgcs validate --strict
	pgcs generate all --force

.PHONY: generate-preview
generate-preview:
	pgcs generate all --dry-run --verbose
```

### Bash Script

```bash
#!/bin/bash
set -e

echo "Validating configuration..."
pgcs validate --strict

echo "Generating code..."
pgcs generate all --force --verbose

echo "Done!"
```

## Getting Help

```bash
# General help
pgcs --help

# Command help
pgcs generate --help
pgcs generate schema --help
pgcs validate --help
pgcs init --help

# Version info
pgcs --version
```
