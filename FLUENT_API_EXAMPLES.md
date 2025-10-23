# Fluent API Examples для PgCs

Примеры использования новых Fluent API builders для удобной конфигурации генерации кода.

## 1. SchemaGenerationOptions - Настройка генерации схемы

### Традиционный подход (до рефакторинга):
```csharp
var options = new SchemaGenerationOptions
{
    RootNamespace = "MyApp.Database",
    OutputDirectory = "./Generated",
    UseRecordTypes = true,
    GenerateXmlDocumentation = true,
    FileOrganization = FileOrganization.ByType,
    GenerateMappingAttributes = true,
    TablePrefix = "tbl_",
    ExcludeTablePatterns = new[] { "temp_.*", "backup_.*" }
};
```

### Новый Fluent API подход:
```csharp
var options = SchemaGenerationOptions.CreateBuilder()
    .WithNamespace("MyApp.Database")
    .OutputTo("./Generated")
    .UseRecords()
    .WithXmlDocs()
    .OrganizeByType()
    .WithMappingAttributes()
    .RemoveTablePrefix("tbl_")
    .ExcludeTables("temp_.*", "backup_.*")
    .Build();
```

### Расширенный пример:
```csharp
var options = SchemaGenerationOptions.CreateBuilder()
    .WithNamespace("MyCompany.ERP.Data")
    .OutputTo("./src/Generated/Models")
    // Типы и свойства
    .UseRecords()
    .UsePrimaryConstructors()
    .UseInitProperties()
    .EnableNullable()
    // Документация и атрибуты
    .WithXmlDocs()
    .WithMappingAttributes()
    .WithValidationAttributes()
    // Организация
    .OrganizeBySchemaAndType()
    // Маппинг имён
    .RemoveTablePrefix("tbl_")
    .MapTable("user_accounts", "User")
    .MapColumn("created_ts", "CreatedAt")
    // Кастомные типы
    .MapType("jsonb", "JsonDocument")
    // Фильтрация
    .ExcludeTables("^temp_", "^staging_", ".*_backup$")
    .IncludeOnlyTables("^(users|products|orders)")
    .WithFunctions()
    // Форматирование
    .WithFormatting()
    .OverwriteFiles()
    .Build();
```

## 2. QueryGenerationOptions - Настройка генерации запросов

### Традиционный подход:
```csharp
var options = new QueryGenerationOptions
{
    RootNamespace = "MyApp.Queries",
    OutputDirectory = "./Generated/Queries",
    RepositoryClassName = "UserRepository",
    GenerateInterface = true,
    UseValueTask = true,
    SupportCancellation = true,
    NullHandling = NullHandlingStrategy.Nullable
};
```

### Новый Fluent API:
```csharp
var options = QueryGenerationOptions.CreateBuilder()
    .WithNamespace("MyApp.Queries")
    .OutputTo("./Generated/Queries")
    .WithRepositoryName("UserRepository")
    .WithInterface()
    .UseValueTask()
    .WithCancellation()
    .UseNullableTypes()
    .Build();
```

### Расширенный пример:
```csharp
var options = QueryGenerationOptions.CreateBuilder()
    .WithNamespace("MyCompany.ERP.Repositories")
    .OutputTo("./src/Generated/Repositories")
    .WithRepositoryName("OrderRepository", "IOrderRepository")
    // Асинхронность
    .UseAsync()
    .UseValueTask()
    .WithCancellation()
    // Организация
    .GroupByEntity()
    .WithInterface()
    // Генерация
    .UseRecords()
    .WithXmlDocs()
    .IncludeSqlInDocs()
    // Технологии
    .UseNpgsqlDirectly()
    .WithPreparedStatements()
    .WithTransactionSupport()
    .UseDataSource()
    // Модели
    .AlwaysGenerateResultModels()
    .WithParameterModels(threshold: 3)
    .ReuseSchemaModels("MyCompany.ERP.Data")
    // NULL обработка
    .UseNullableTypes()
    // Форматирование
    .WithFormatting()
    .OverwriteFiles()
    .Build();
```

### Генерация с Dapper:
```csharp
var options = QueryGenerationOptions.CreateBuilder()
    .WithNamespace("MyApp.Data.Dapper")
    .OutputTo("./Data/Repositories")
    .UseDapper()  // Автоматически отключает UseNpgsqlDirectly
    .WithInterface()
    .GroupByEntity()
    .Build();
```

### Генерация extension methods:
```csharp
var options = QueryGenerationOptions.CreateBuilder()
    .WithNamespace("MyApp.Extensions")
    .OutputTo("./Extensions")
    .AsExtensionMethods()  // Генерирует extension методы для IDbConnection
    .UseAsync()
    .WithoutInterface()
    .Build();
```

## 3. WriteOptions - Настройка записи файлов

### Традиционный подход:
```csharp
var writeOptions = new WriteOptions
{
    OutputPath = "./Generated",
    OverwriteExisting = true,
    CreateDirectories = true,
    CreateBackups = true,
    BackupPath = "./Backups",
    Encoding = "UTF-8"
};
```

### Новый Fluent API:
```csharp
var writeOptions = WriteOptions.CreateBuilder()
    .OutputTo("./Generated")
    .OverwriteExisting()
    .CreateDirectories()
    .WithBackups("./Backups")
    .UseUtf8()
    .Build();
```

### Безопасная запись с проверкой:
```csharp
var writeOptions = WriteOptions.CreateBuilder()
    .OutputTo("./Generated")
    .PreserveExisting()  // Не перезаписывать
    .CreateDirectories()
    .WithBackups()
    .DryRun()  // Сначала проверка
    .Build();

// После проверки - реальная запись
var actualOptions = WriteOptions.CreateBuilder()
    .OutputTo("./Generated")
    .OverwriteExisting()
    .WithBackups("./Backups")
    .UseUtf8WithBom()
    .ActualWrite()
    .Build();
```

## 4. SchemaFilter - Фильтрация метаданных

### Фильтрация системных объектов:
```csharp
var filtered = SchemaFilter.From(metadata)
    .RemoveSystemObjects()  // Удаляет pg_catalog, information_schema
    .Build();
```

### Работа только с таблицами:
```csharp
var tablesOnly = SchemaFilter.From(metadata)
    .OnlyTables()  // Оставить только таблицы
    .ExcludeTables("temp_.*", "staging_.*")
    .RemoveSystemObjects()
    .Build();
```

### Сложная фильтрация:
```csharp
var filtered = SchemaFilter.From(metadata)
    // Исключить системные схемы
    .ExcludeSchemas("pg_catalog", "information_schema", "pg_toast")
    // Работать только с определёнными схемами
    .IncludeOnlySchemas("public", "sales", "inventory")
    // Исключить таблицы
    .ExcludeTables("^temp_", "^backup_", ".*_old$", ".*_staging$")
    // Включить только определённые таблицы
    .IncludeOnlyTables("^(users|products|orders|customers)")
    // Типы
    .IncludeOnlyTypes(TypeKind.Enum, TypeKind.Composite)
    // Финальная сборка
    .Build();
```

### Разные комбинации:
```csharp
// Только таблицы и представления
var tablesAndViews = SchemaFilter.From(metadata)
    .OnlyTablesAndViews()
    .RemoveSystemObjects()
    .Build();

// Удалить триггеры и индексы
var simplified = SchemaFilter.From(metadata)
    .RemoveTriggers()
    .RemoveIndexes()
    .Build();

// Работа только с enum типами
var enums = SchemaFilter.From(metadata)
    .RemoveTables()
    .RemoveViews()
    .RemoveFunctions()
    .IncludeOnlyTypes(TypeKind.Enum)
    .Build();
```

## 5. Полный пример использования

### Комплексный workflow: Анализ → Фильтрация → Генерация

```csharp
using PgCs.SchemaAnalyzer;
using PgCs.QueryAnalyzer;
using PgCs.SchemaGenerator;
using PgCs.QueryGenerator;
using PgCs.Common.SchemaAnalyzer;
using PgCs.Common.SchemaGenerator.Models.Options;
using PgCs.Common.QueryGenerator.Models.Options;
using PgCs.Common.Writer.Models;
using PgCs.FileWriter;

// ============================================================================
// ШАГ 1: Анализ схемы базы данных
// ============================================================================

var schemaMetadata = await SchemaAnalyzerBuilder
    .Create()
    .FromDirectory("./database/migrations")
    .ExcludeSystemSchemas()
    .ExcludeSchemas("audit", "logging", "archive")
    .ExcludeTables("^temp_", "^staging_", ".*_backup$", ".*_old$")
    .IncludeOnlySchemas("public", "app", "sales")
    .WithTables()
    .WithViews()
    .WithTypes()
    .WithFunctions()
    .WithComments()
    .WithoutTriggers()
    .WithoutIndexes()
    .RemoveDuplicates()
    .AnalyzeAsync();

Console.WriteLine($"✅ Проанализировано схемы:");
Console.WriteLine($"   Таблиц: {schemaMetadata.Tables.Count}");
Console.WriteLine($"   Представлений: {schemaMetadata.Views.Count}");
Console.WriteLine($"   Типов: {schemaMetadata.Types.Count}");
Console.WriteLine($"   Функций: {schemaMetadata.Functions.Count}");

// ============================================================================
// ШАГ 2: Дополнительная фильтрация через SchemaFilter
// ============================================================================

var filteredSchema = SchemaFilter
    .From(schemaMetadata)
    .ExcludeTables("migration_.*", "flyway_.*")
    .IncludeOnlyTables("^(users|orders|products|customers|categories)")
    .RemoveSystemObjects()
    .Build();

Console.WriteLine($"✅ После фильтрации осталось {filteredSchema.Tables.Count} таблиц");

// ============================================================================
// ШАГ 3: Настройка генерации моделей схемы
// ============================================================================

var schemaGenOptions = SchemaGenerationOptions.CreateBuilder()
    .WithNamespace("MyCompany.ECommerce.Data.Models")
    .OutputTo("./src/Generated/Models")
    // Современные C# фичи
    .UseRecords()
    .UsePrimaryConstructors()
    .UseInitProperties()
    .EnableNullable()
    // Документация
    .WithXmlDocs()
    .WithMappingAttributes()
    .WithValidationAttributes()
    // Организация файлов
    .OrganizeBySchemaAndType()
    // Маппинг имён
    .RemoveTablePrefix("tbl_")
    .RemoveTableSuffix("_data")
    .MapTable("user_accounts", "User")
    .MapTable("product_catalog", "Product")
    .MapColumn("created_ts", "CreatedAt")
    .MapColumn("updated_ts", "UpdatedAt")
    // Кастомные типы PostgreSQL → C#
    .MapType("jsonb", "JsonDocument")
    .MapType("uuid", "Guid")
    // Функции и форматирование
    .WithFunctions()
    .WithFormatting()
    .OverwriteFiles()
    .Build();

// ============================================================================
// ШАГ 4: Генерация моделей
// ============================================================================

var schemaGenerator = SchemaGenerator.Create();
var schemaResult = await schemaGenerator.GenerateAsync(filteredSchema, schemaGenOptions);

Console.WriteLine($"✅ Сгенерировано {schemaResult.GeneratedCode.Count} файлов моделей");

// ============================================================================
// ШАГ 5: Анализ SQL запросов
// ============================================================================

var queryMetadata = await QueryAnalyzerBuilder
    .Create()
    .FromFiles(
        "./queries/users.sql",
        "./queries/orders.sql",
        "./queries/products.sql"
    )
    .WithParameterExtraction()
    .WithTypeInference()
    .WithAnnotationParsing()
    .SkipInvalidQueries()
    .ExcludeQueries("Test.*", "Debug.*")
    .AnalyzeAsync();

Console.WriteLine($"✅ Проанализировано {queryMetadata.Count} SQL запросов");

// ============================================================================
// ШАГ 6: Настройка генерации репозиториев
// ============================================================================

var queryGenOptions = QueryGenerationOptions.CreateBuilder()
    .WithNamespace("MyCompany.ECommerce.Data.Repositories")
    .OutputTo("./src/Generated/Repositories")
    .WithRepositoryName("ECommerceRepository", "IECommerceRepository")
    // Асинхронность
    .UseAsync()
    .UseValueTask()
    .WithCancellation()
    // Организация
    .GroupByEntity()
    .WithInterface()
    // Генерация
    .UseRecords()
    .WithXmlDocs()
    .IncludeSqlInDocs()
    // ORM и технологии
    .UseDapper()
    .WithPreparedStatements()
    .WithTransactionSupport()
    .UseDataSource()
    // Модели
    .AlwaysGenerateResultModels()
    .WithParameterModels(threshold: 3)
    .ReuseSchemaModels("MyCompany.ECommerce.Data.Models")
    // NULL handling
    .UseNullableTypes()
    // Форматирование
    .WithFormatting()
    .OverwriteFiles()
    .Build();

// ============================================================================
// ШАГ 7: Генерация репозиториев и запросов
// ============================================================================

var queryGenerator = QueryGenerator.Create();
var queryResult = await queryGenerator.GenerateAsync(queryMetadata, queryGenOptions);

Console.WriteLine($"✅ Сгенерировано {queryResult.GeneratedCode.Count} файлов репозиториев");

// ============================================================================
// ШАГ 8: Настройка записи файлов
// ============================================================================

var writeOptions = WriteOptions.CreateBuilder()
    .OutputTo("./src/Generated")
    .OverwriteExisting()
    .CreateDirectories()
    .WithBackups("./backups")
    .UseUtf8()
    .ActualWrite()
    .Build();

// ============================================================================
// ШАГ 9: Запись всех сгенерированных файлов
// ============================================================================

var writer = FileWriter.Create();

// Записать модели
await writer.WriteManyAsync(schemaResult.GeneratedCode, writeOptions);
Console.WriteLine($"✅ Записано {schemaResult.GeneratedCode.Count} файлов моделей");

// Записать репозитории
await writer.WriteManyAsync(queryResult.GeneratedCode, writeOptions);
Console.WriteLine($"✅ Записано {queryResult.GeneratedCode.Count} файлов репозиториев");

// ============================================================================
// ШАГ 10: Статистика
// ============================================================================

Console.WriteLine();
Console.WriteLine("=".PadRight(80, '='));
Console.WriteLine("ИТОГОВАЯ СТАТИСТИКА:");
Console.WriteLine("=".PadRight(80, '='));
Console.WriteLine($"Проанализировано объектов БД:");
Console.WriteLine($"  • Таблиц: {filteredSchema.Tables.Count}");
Console.WriteLine($"  • Представлений: {filteredSchema.Views.Count}");
Console.WriteLine($"  • Типов: {filteredSchema.Types.Count}");
Console.WriteLine($"  • Функций: {filteredSchema.Functions.Count}");
Console.WriteLine();
Console.WriteLine($"Проанализировано SQL запросов: {queryMetadata.Count}");
Console.WriteLine();
Console.WriteLine($"Сгенерировано файлов:");
Console.WriteLine($"  • Моделей: {schemaResult.GeneratedCode.Count}");
Console.WriteLine($"  • Репозиториев: {queryResult.GeneratedCode.Count}");
Console.WriteLine($"  • Всего: {schemaResult.GeneratedCode.Count + queryResult.GeneratedCode.Count}");
Console.WriteLine("=".PadRight(80, '='));
```

### Построение запроса программно и его анализ:

```csharp
// Построить SQL запрос через QueryBuilder
var sqlQuery = QueryBuilder
    .Select("u.id", "u.username", "u.email", "COUNT(o.id) as order_count")
    .From("users u")
    .Named("GetUsersWithOrderCount")
    .Many()
    .LeftJoin("orders o", "u.id = o.user_id")
    .WhereEquals("u.status", 1)
    .WhereBetween("u.created_at", 2, 3)
    .GroupBy("u.id", "u.username", "u.email")
    .Having("COUNT(o.id) > 0")
    .OrderByDesc("order_count")
    .LimitParam(4)
    .Build();

Console.WriteLine("Построенный SQL:");
Console.WriteLine(sqlQuery);
Console.WriteLine();

// Проанализировать построенный запрос
var queries = await QueryAnalyzerBuilder
    .Create()
    .FromQuery(sqlQuery)
    .WithParameterExtraction()
    .WithTypeInference()
    .AnalyzeAsync();

var metadata = queries.First();
Console.WriteLine($"Метод: {metadata.MethodName}");
Console.WriteLine($"Тип: {metadata.QueryType}");
Console.WriteLine($"Параметров: {metadata.Parameters.Count}");
Console.WriteLine($"Возвращает: {metadata.ReturnType?.ModelName}");
```

### Динамическое построение конфигурации:

```csharp
// Динамически настроить генерацию на основе environment
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var isDevelopment = environment == "Development";

var options = SchemaGenerationOptions.CreateBuilder()
    .WithNamespace("MyApp.Data")
    .OutputTo(isDevelopment ? "./src/Generated" : "./build/Generated");

// В development - больше документации и проверок
if (isDevelopment)
{
    options = options
        .WithXmlDocs()
        .WithValidationAttributes()
        .WithMappingAttributes()
        .EnableNullable();
}
else
{
    // В production - минимум overhead
    options = options
        .WithoutXmlDocs()
        .WithoutValidationAttributes();
}

var finalOptions = options
    .UseRecords()
    .OrganizeByType()
    .Build();

Console.WriteLine($"Генерация для {environment}:");
Console.WriteLine($"  Namespace: {finalOptions.RootNamespace}");
Console.WriteLine($"  Output: {finalOptions.OutputDirectory}");
Console.WriteLine($"  XML Docs: {finalOptions.GenerateXmlDocumentation}");
```

## Преимущества Fluent API

1. **Читаемость** - код читается как предложение
2. **Типобезопасность** - невозможно создать невалидную конфигурацию
3. **IntelliSense** - IDE подсказывает доступные методы
4. **Immutability** - builder не мутирует состояние, создаёт новые объекты
5. **Цепочки вызовов** - удобное комбинирование настроек
6. **Самодокументирование** - понятно без комментариев
7. **Программное построение** - SQL запросы и конфигурации можно создавать динамически
8. **Композируемость** - легко комбинировать разные builder'ы
9. **Testability** - легко тестировать благодаря fluent interface

## 6. QueryBuilder - Построение SQL запросов

### Простой SELECT запрос:
```csharp
var query = QueryBuilder
    .Select("id", "username", "email")
    .From("users")
    .Named("GetUserById")
    .One()
    .WhereEquals("id", 1)
    .Build();

// Результат:
// -- name: GetUserById :one
// SELECT id,
//        username,
//        email
// FROM users
// WHERE id = $1
```

### SELECT с JOIN:
```csharp
var query = QueryBuilder
    .Select("o.id", "o.order_number", "o.total", "u.username", "u.email")
    .From("orders o")
    .Named("GetOrderWithUser")
    .One()
    .InnerJoin("users u", "o.user_id = u.id")
    .WhereEquals("o.id", 1)
    .Build();

// Результат:
// -- name: GetOrderWithUser :one
// SELECT o.id,
//        o.order_number,
//        o.total,
//        u.username,
//        u.email
// FROM orders o
//     INNER JOIN users u ON o.user_id = u.id
// WHERE o.id = $1
```

### SELECT с множественными условиями:
```csharp
var query = QueryBuilder
    .Select("id", "username", "email", "status", "created_at")
    .From("users")
    .Named("SearchUsers")
    .Many()
    .WhereIn("status", 1, "user_status")
    .WhereBetween("created_at", 2, 3)
    .WhereILike("username", 4)
    .OrderByDesc("created_at")
    .LimitParam(5)
    .OffsetParam(6)
    .Build();

// Результат:
// -- name: SearchUsers :many
// SELECT id,
//        username,
//        email,
//        status,
//        created_at
// FROM users
// WHERE status = ANY($1::user_status[])
//   AND created_at BETWEEN $2 AND $3
//   AND username ILIKE $4
// ORDER BY created_at DESC
// LIMIT $5
// OFFSET $6
```

### SELECT с JSONB и массивами:
```csharp
var query = QueryBuilder
    .Select("id", "username", "preferences", "tags")
    .From("users")
    .Named("SearchUsersByPreferences")
    .Many()
    .WhereJsonbContains("preferences", 1)
    .WhereArrayOverlaps("tags", 2, "text")
    .OrderBy("created_at")
    .Build();

// Результат:
// -- name: SearchUsersByPreferences :many
// SELECT id,
//        username,
//        preferences,
//        tags
// FROM users
// WHERE preferences @> $1::jsonb
//   AND tags && $2::text[]
// ORDER BY created_at
```

### SELECT с GROUP BY и агрегацией:
```csharp
var query = QueryBuilder
    .Select(
        "u.id",
        "u.username",
        "COUNT(o.id) as order_count",
        "SUM(o.total) as total_spent"
    )
    .From("users u")
    .Named("GetUserOrdersSummary")
    .One()
    .LeftJoin("orders o", "u.id = o.user_id")
    .WhereEquals("u.id", 1)
    .GroupBy("u.id", "u.username")
    .Having("COUNT(o.id) > 0")
    .Build();
```

### SELECT с CTE (Common Table Expression):
```csharp
var cteQuery = @"
    SELECT user_id,
           COUNT(*) as order_count,
           SUM(total) as total_spent
    FROM orders
    WHERE status != 'cancelled'
    GROUP BY user_id";

var query = QueryBuilder
    .Select("u.id", "u.username", "os.order_count", "os.total_spent")
    .WithCte("order_stats", cteQuery)
    .From("users u")
    .Named("GetUsersWithOrderStats")
    .Many()
    .LeftJoin("order_stats os", "u.id = os.user_id")
    .WhereEquals("u.status", 1)
    .OrderByDesc("os.total_spent")
    .LimitParam(2)
    .Build();
```

### INSERT запрос:
```csharp
var query = QueryBuilder
    .Insert("users")
    .Named("CreateUser")
    .One()
    .Values("username", "email", "password_hash", "full_name")
    .Returning("id", "external_id", "created_at")
    .Build();

// Результат:
// -- name: CreateUser :one
// INSERT INTO users (
//     username,
//     email,
//     password_hash,
//     full_name
// )
// VALUES ($1, $2, $3, $4) RETURNING id, external_id, created_at
```

### UPDATE запрос:
```csharp
var query = QueryBuilder
    .Update("users")
    .Named("UpdateUserStatus")
    .ExecRows()
    .SetParam("status", 2)
    .SetNow("updated_at")
    .WhereEquals("id", 1)
    .Build();

// Результат:
// -- name: UpdateUserStatus :execrows
// UPDATE users
// SET status = $2,
//     updated_at = NOW()
// WHERE id = $1
```

### UPDATE с JSONB:
```csharp
var query = QueryBuilder
    .Update("users")
    .Named("UpdateUserPreferences")
    .Exec()
    .Set("preferences", "preferences || $2::jsonb")
    .SetNow("updated_at")
    .WhereEquals("id", 1)
    .Build();
```

### DELETE запрос:
```csharp
var query = QueryBuilder
    .Delete("orders")
    .Named("DeleteOrdersByUser")
    .ExecRows()
    .WhereEquals("user_id", 1)
    .WhereEquals("status", 2)
    .Build();

// Результат:
// -- name: DeleteOrdersByUser :execrows
// DELETE FROM orders
// WHERE user_id = $1
//   AND status = $2
```

### Комплексный пример с LEFT JOIN и NULL проверками:
```csharp
var query = QueryBuilder
    .Select(
        "o.id",
        "o.order_number",
        "o.status",
        "o.total",
        "u.username",
        "COALESCE(u.email, 'no-email@example.com') as email"
    )
    .From("orders o")
    .Named("ListOrdersWithUserInfo")
    .Many()
    .LeftJoin("users u", "o.user_id = u.id")
    .WhereEquals("o.status", 1)
    .WhereNotNull("o.shipped_at")
    .OrderByDesc("o.created_at")
    .LimitParam(2)
    .Build();
```

### Использование Distinct:
```csharp
var query = QueryBuilder
    .Select("DISTINCT category_id")
    .Distinct()
    .From("order_items")
    .Named("GetUsedCategories")
    .Many()
    .OrderBy("category_id")
    .Build();
```

## 7. SchemaAnalyzerBuilder - Анализ схемы базы данных

### Анализ одного файла:
```csharp
var metadata = await SchemaAnalyzerBuilder
    .Create()
    .FromFile("./schema.sql")
    .AnalyzeAsync();

Console.WriteLine($"Найдено {metadata.Tables.Count} таблиц");
```

### Анализ директории с SQL файлами:
```csharp
var metadata = await SchemaAnalyzerBuilder
    .Create()
    .FromDirectory("./migrations")
    .ExcludeSystemSchemas()
    .OnlyTables()
    .AnalyzeAsync();
```

### Анализ с фильтрацией:
```csharp
var metadata = await SchemaAnalyzerBuilder
    .Create()
    .FromFiles("./schema.sql", "./types.sql", "./functions.sql")
    .ExcludeSystemSchemas()
    .ExcludeSchemas("audit", "logging")
    .IncludeOnlySchemas("public", "sales")
    .ExcludeTables("^temp_", "^staging_", ".*_backup$")
    .IncludeOnlyTables("^(users|orders|products)")
    .WithTables()
    .WithViews()
    .WithTypes()
    .WithoutFunctions()
    .WithoutTriggers()
    .WithoutIndexes()
    .RemoveDuplicates()
    .AnalyzeAsync();

Console.WriteLine($"Таблицы: {metadata.Tables.Count}");
Console.WriteLine($"Представления: {metadata.Views.Count}");
Console.WriteLine($"Типы: {metadata.Types.Count}");
```

### Извлечение только типов (ENUM, DOMAIN):
```csharp
var metadata = await SchemaAnalyzerBuilder
    .Create()
    .FromFile("./schema.sql")
    .OnlyTypes()
    .ExcludeSystemSchemas()
    .AnalyzeAsync();

foreach (var type in metadata.Types)
{
    Console.WriteLine($"{type.Name} ({type.Kind})");
}
```

### Анализ с комментариями:
```csharp
var metadata = await SchemaAnalyzerBuilder
    .Create()
    .FromDirectory("./database")
    .WithTables()
    .WithComments()
    .ExcludeSystemSchemas()
    .AnalyzeAsync();

// Получить комментарий для таблицы
if (metadata.Comments?.TryGetValue("TABLE:users", out var comment) == true)
{
    Console.WriteLine($"Users table: {comment}");
}
```

### Анализ только таблиц и представлений:
```csharp
var metadata = await SchemaAnalyzerBuilder
    .Create()
    .FromFile("./schema.sql")
    .OnlyTablesAndViews()
    .AnalyzeAsync();
```

## 8. QueryAnalyzerBuilder - Анализ SQL запросов

### Анализ файла с запросами:
```csharp
var queries = await QueryAnalyzerBuilder
    .Create()
    .FromFile("./queries/users.sql")
    .AnalyzeAsync();

foreach (var query in queries)
{
    Console.WriteLine($"{query.MethodName}: {query.QueryType} ({query.ReturnCardinality})");
    Console.WriteLine($"  Параметров: {query.Parameters.Count}");
    if (query.ReturnType != null)
    {
        Console.WriteLine($"  Возвращает: {query.ReturnType.ModelName}");
    }
}
```

### Анализ нескольких файлов:
```csharp
var queries = await QueryAnalyzerBuilder
    .Create()
    .FromFiles("./queries/users.sql", "./queries/orders.sql", "./queries/products.sql")
    .WithParameterExtraction()
    .WithTypeInference()
    .SkipInvalidQueries()
    .AnalyzeAsync();

Console.WriteLine($"Проанализировано {queries.Count} запросов");
```

### Анализ конкретного запроса:
```csharp
var sqlQuery = @"
-- name: GetUserById :one
SELECT id, username, email
FROM users
WHERE id = $1";

var queries = await QueryAnalyzerBuilder
    .Create()
    .FromQuery(sqlQuery)
    .AnalyzeAsync();

var metadata = queries.First();
Console.WriteLine($"Метод: {metadata.MethodName}");
Console.WriteLine($"SQL: {metadata.SqlQuery}");
```

### Анализ только SELECT запросов:
```csharp
var queries = await QueryAnalyzerBuilder
    .Create()
    .FromFile("./queries/all.sql")
    .OnlySelects()
    .WithTypeInference()
    .AnalyzeAsync();

foreach (var query in queries)
{
    Console.WriteLine($"{query.MethodName} возвращает {query.ReturnType?.Columns.Count ?? 0} колонок");
}
```

### Фильтрация по именам запросов:
```csharp
var queries = await QueryAnalyzerBuilder
    .Create()
    .FromFile("./queries/users.sql")
    .IncludeOnlyQueries("GetUserById", "GetUserByEmail", "ListActiveUsers")
    .ExcludeQueries("DeleteUser", "HardDeleteUser")
    .AnalyzeAsync();
```

### Анализ с валидацией:
```csharp
var queries = await QueryAnalyzerBuilder
    .Create()
    .FromFiles("./queries/*.sql")
    .ValidateSyntax()
    .FailOnInvalidQuery()  // Бросить исключение при ошибке
    .WithParameterExtraction()
    .WithTypeInference()
    .AnalyzeAsync();
```

### Анализ INSERT/UPDATE запросов:
```csharp
var inserts = await QueryAnalyzerBuilder
    .Create()
    .FromFile("./queries/mutations.sql")
    .OnlyInserts()
    .AnalyzeAsync();

var updates = await QueryAnalyzerBuilder
    .Create()
    .FromFile("./queries/mutations.sql")
    .OnlyUpdates()
    .AnalyzeAsync();

Console.WriteLine($"INSERT запросов: {inserts.Count}");
Console.WriteLine($"UPDATE запросов: {updates.Count}");
```

## Преимущества Fluent API

1. **Читаемость** - код читается как предложение
2. **Типобезопасность** - невозможно создать невалидную конфигурацию
3. **IntelliSense** - IDE подсказывает доступные методы
4. **Immutability** - builder не мутирует состояние, создаёт новые объекты
5. **Цепочки вызовов** - удобное комбинирование настроек
6. **Самодокументирование** - понятно без комментариев
7. **Программное построение** - SQL запросы можно создавать динамически
