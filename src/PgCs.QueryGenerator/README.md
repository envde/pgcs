# PgCs.QueryGenerator

Генератор C# методов для выполнения SQL запросов PostgreSQL с поддержкой транзакций и async/await.

## Обзор

`PgCs.QueryGenerator` - это компонент проекта PgCs, который преобразует SQL запросы с аннотациями в типобезопасные C# методы. Генератор создает:

- **Асинхронные методы** с поддержкой `ValueTask` или `Task`
- **Модели результатов** (record types) для SELECT запросов
- **Транзакционные методы** с опциональным параметром `NpgsqlTransaction`
- **XML документацию** на русском языке
- **Поддержку CancellationToken** для отмены операций

## Архитектура

Генератор построен по принципам SOLID и Clean Architecture с использованием паттернов:

- **Facade** - `QueryGenerator` как единая точка входа
- **Strategy** - различные генераторы для методов, моделей, классов
- **Builder** - `QueryCodeBuilder` для построения кода
- **Dependency Injection** - поддержка внедрения зависимостей

### Основные компоненты

```
PgCs.QueryGenerator/
├── QueryGenerator.cs           # Главный фасад
├── Core/
│   ├── FileWriter.cs          # Запись файлов
│   └── CodeValidator.cs       # Базовая валидация
├── Generation/
│   ├── MethodGenerator.cs     # Генерация методов запросов
│   ├── ClassGenerator.cs      # Генерация классов
│   ├── ResultModelGenerator.cs # Генерация моделей результатов
│   └── ParameterModelGenerator.cs # Генерация моделей параметров
├── Mapping/
│   └── NpgsqlTypeMapper.cs    # Маппинг типов PostgreSQL → C# → NpgsqlDbType
└── Formatting/
    └── QueryCodeBuilder.cs    # Построитель C# кода
```

## Использование

### Пример SQL запроса с аннотацией

```sql
-- name: GetUserById :one
SELECT id, username, email, created_at
FROM users
WHERE id = $1;

-- name: ListActiveUsers :many
SELECT id, username, email, created_at
FROM users
WHERE is_active = true
ORDER BY created_at DESC;

-- name: CreateUser :exec
INSERT INTO users (username, email, password_hash)
VALUES ($1, $2, $3);

-- name: UpdateUserEmail :execrows
UPDATE users
SET email = $1, updated_at = NOW()
WHERE id = $2;
```

### Генерация методов

```csharp
using PgCs.QueryGenerator;
using PgCs.Common.QueryGenerator.Models;
using PgCs.Common.QueryAnalyzer.Models.Metadata;

var generator = new QueryGenerator();

var queries = new List<QueryMetadata>
{
    // Метаданные запросов от QueryAnalyzer
    // ...
};

var options = new QueryGenerationOptions
{
    OutputDirectory = "./Generated",
    Namespace = "MyApp.Data",
    ClassName = "UserQueries",
    GenerateAsyncMethods = true,
    UseValueTask = true,
    GenerateXmlDocumentation = true,
    SupportCancellation = true
};

var result = await generator.GenerateAsync(queries, options);

Console.WriteLine($"Сгенерировано методов: {result.GeneratedMethodsCount}");
Console.WriteLine($"Сгенерировано моделей: {result.GeneratedModelsCount}");
```

### Сгенерированный код

```csharp
namespace MyApp.Data;

/// <summary>
/// Модель результата для запроса GetUserById
/// </summary>
public sealed record GetUserByIdResult
{
    /// <summary>
    /// Колонка id (bigint)
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Колонка username (text)
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// Колонка email (text)
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Колонка created_at (timestamp with time zone)
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Интерфейс для выполнения SQL запросов
/// </summary>
public interface IUserQueries
{
    /// <summary>
    /// Выполняет SQL запрос: GetUserById
    /// </summary>
    /// <param name="connection">Подключение к базе данных</param>
    /// <param name="id">Параметр id</param>
    /// <param name="transaction">Опциональная транзакция</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Один результат или null</returns>
    ValueTask<GetUserByIdResult?> GetUserByIdAsync(
        NpgsqlConnection connection,
        long id,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Выполняет SQL запрос: ListActiveUsers
    /// </summary>
    ValueTask<IReadOnlyList<ListActiveUsersResult>> ListActiveUsersAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Выполняет SQL запрос: CreateUser
    /// </summary>
    ValueTask CreateUserAsync(
        NpgsqlConnection connection,
        string username,
        string email,
        string passwordHash,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Выполняет SQL запрос: UpdateUserEmail
    /// </summary>
    /// <returns>Количество затронутых строк</returns>
    ValueTask<int> UpdateUserEmailAsync(
        NpgsqlConnection connection,
        string email,
        long id,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Реализация IUserQueries для выполнения SQL запросов
/// </summary>
public sealed class UserQueries : IUserQueries
{
    /// <summary>
    /// Выполняет SQL запрос: GetUserById
    /// </summary>
    /// <param name="connection">Подключение к базе данных</param>
    /// <param name="id">Параметр id</param>
    /// <param name="transaction">Опциональная транзакция</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Один результат или null</returns>
    public async ValueTask<GetUserByIdResult?> GetUserByIdAsync(
        NpgsqlConnection connection,
        long id,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT id, username, email, created_at
FROM users
WHERE id = $1
        ";

        await using var cmd = new NpgsqlCommand(sql, connection, transaction);

        // Параметры запроса
        cmd.Parameters.Add(new NpgsqlParameter<long>("id", id));

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new GetUserByIdResult
        {
            Id = reader.GetInt64(0),
            Username = reader.GetString(1),
            Email = reader.GetString(2),
            CreatedAt = reader.GetFieldValue<DateTimeOffset>(3)
        };
    }

    // ... остальные методы
}
```

## Работа с транзакциями

Все сгенерированные методы поддерживают транзакции через опциональный параметр `NpgsqlTransaction?`:

### Без транзакции

```csharp
await using var connection = dataSource.CreateConnection();
await connection.OpenAsync();

var user = await queries.GetUserByIdAsync(connection, userId);
```

### С транзакцией

```csharp
await using var connection = dataSource.CreateConnection();
await connection.OpenAsync();

await using var transaction = await connection.BeginTransactionAsync();

try
{
    await queries.CreateUserAsync(connection, "john", "john@example.com", "hash", transaction);
    await queries.UpdateUserEmailAsync(connection, "newemail@example.com", userId, transaction);
    
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### Изоляция транзакций

```csharp
await using var transaction = await connection.BeginTransactionAsync(
    IsolationLevel.ReadCommitted);

var users = await queries.ListActiveUsersAsync(connection, transaction);
```

## Маппинг типов

`NpgsqlTypeMapper` поддерживает автоматическое преобразование между PostgreSQL, C# и NpgsqlDbType:

| PostgreSQL Type          | C# Type            | NpgsqlDbType         |
|-------------------------|-------------------|---------------------|
| smallint, int2          | short             | Smallint            |
| integer, int4           | int               | Integer             |
| bigint, int8            | long              | Bigint              |
| real, float4            | float             | Real                |
| double precision, float8| double            | Double              |
| numeric, decimal        | decimal           | Numeric             |
| text, varchar           | string            | Text                |
| boolean, bool           | bool              | Boolean             |
| timestamp               | DateTime          | Timestamp           |
| timestamptz             | DateTimeOffset    | TimestampTz         |
| date                    | DateOnly          | Date                |
| time                    | TimeOnly          | Time                |
| interval                | TimeSpan          | Interval            |
| uuid                    | Guid              | Uuid                |
| bytea                   | byte[]            | Bytea               |
| json, jsonb             | string            | Jsonb               |
| _int4, integer[]        | int[]             | Integer \| Array    |
| _text, text[]           | string[]          | Text \| Array       |

## Опции генерации

```csharp
public sealed record QueryGenerationOptions
{
    // Базовые опции
    public required string OutputDirectory { get; init; }
    public required string Namespace { get; init; }
    public string ClassName { get; init; } = "Queries";

    // Асинхронность
    public bool GenerateAsyncMethods { get; init; } = true;
    public bool UseValueTask { get; init; } = true;
    public bool SupportCancellation { get; init; } = true;

    // Модели
    public bool GenerateResultModels { get; init; } = true;
    public bool GenerateParameterModels { get; init; } = false;
    public bool UseRecordsForResults { get; init; } = true;

    // Код
    public bool GenerateXmlDocumentation { get; init; } = true;
    public bool GenerateInterface { get; init; } = true;
    public bool UseNullableReferenceTypes { get; init; } = true;

    // Провайдер БД
    public DatabaseProvider DatabaseProvider { get; init; } = DatabaseProvider.Npgsql;

    // Форматирование
    public IndentationStyle IndentationStyle { get; init; } = IndentationStyle.Spaces;
    public int IndentationSize { get; init; } = 4;
    public string MethodSuffix { get; init; } = "Async";
}
```

## Результат генерации

```csharp
public sealed record QueryGenerationResult
{
    public required GeneratedClass GeneratedClass { get; init; }
    public int GeneratedMethodsCount { get; init; }
    public int GeneratedModelsCount { get; init; }
    public int GeneratedFilesCount { get; init; }
    public int TotalLinesGenerated { get; init; }
    public TimeSpan GenerationTime { get; init; }
    public ValidationResult ValidationResult { get; init; }
}
```

## Производительность

- **ValueTask** вместо Task для методов без аллокаций в синхронном пути
- **await using** для автоматического освобождения ресурсов
- **NpgsqlParameter<T>** для типобезопасной передачи параметров
- **GetFieldValue<T>** для оптимального чтения данных
- **IReadOnlyList** вместо List для результатов

## Интеграция с ASP.NET Core

```csharp
// Program.cs
builder.Services.AddNpgsqlDataSource(connectionString);
builder.Services.AddSingleton<IUserQueries, UserQueries>();

// Controller
public class UsersController : ControllerBase
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly IUserQueries _queries;

    public UsersController(NpgsqlDataSource dataSource, IUserQueries queries)
    {
        _dataSource = dataSource;
        _queries = queries;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetUserByIdResult>> GetUser(
        long id,
        CancellationToken cancellationToken)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        
        var user = await _queries.GetUserByIdAsync(connection, id, cancellationToken: cancellationToken);
        
        return user is null ? NotFound() : Ok(user);
    }
}
```

## Требования

- .NET 9.0+
- C# 14 (preview)
- Npgsql 9.0.1+
- PgCs.Common

## Связанные проекты

- **PgCs.QueryAnalyzer** - анализ SQL запросов и извлечение метаданных
- **PgCs.SchemaGenerator** - генерация C# моделей из схемы PostgreSQL
- **PgCs.SchemaAnalyzer** - анализ схемы PostgreSQL

## Лицензия

MIT
