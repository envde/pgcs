# PgCs.Tests.Shared

Общая библиотека для тестовых утилит, используемых во всех тестовых проектах PgCs.

## Цель

Устранение дублирования кода между тестовыми проектами и предоставление единого API для работы с тестовыми данными.

## Компоненты

### `TempSqlFile`

Класс для создания временных SQL файлов в тестах с автоматической очисткой.

```csharp
using PgCs.Tests.Shared.Helpers;

// Использование
using var tempFile = new TempSqlFile("SELECT * FROM users;");
var path = tempFile.Path;
// Файл автоматически удалится при выходе из using блока
```

### `TestFileHelper`

Утилиты для работы с файлами в папке TestData.

```csharp
using PgCs.Tests.Shared.Helpers;

// Чтение тестового файла
var content = TestFileHelper.ReadTestFile("Schema.sql");

// Асинхронное чтение
var content = await TestFileHelper.ReadTestFileAsync("Queries.sql");

// Создание временного файла
using var tempFile = TestFileHelper.CreateTempFile("SQL content");
```

## Использование в тестовых проектах

Добавьте ссылку на проект в `.csproj`:

```xml
<ItemGroup>
    <ProjectReference Include="../PgCs.Tests.Shared/PgCs.Tests.Shared.csproj" />
</ItemGroup>
```

Импортируйте namespace:

```csharp
using PgCs.Tests.Shared.Helpers;
```

## Преимущества

1. **DRY (Don't Repeat Yourself)**: Единый источник для тестовых утилит
2. **Согласованность**: Одинаковое поведение во всех тестах
3. **Поддерживаемость**: Изменения в одном месте применяются везде
4. **Расширяемость**: Легко добавлять новые утилиты

## Рекомендации по миграции

### До (с дублированием)

```csharp
// В PgCs.QueryAnalyzer.Tests/Helpers/TempSqlFile.cs
public sealed class TempSqlFile : IDisposable { /* ... */ }

// В PgCs.SchemaAnalyzer.Tests/Helpers/TempSqlFile.cs  
public sealed class TempSqlFile : IDisposable { /* ... */ }
```

### После (без дублирования)

```csharp
// В обоих проектах
using PgCs.Tests.Shared.Helpers;

using var tempFile = new TempSqlFile(content);
```

## История изменений

### v1.0.0 (2025-10-25)
- Создание общей библиотеки тестовых утилит
- Унификация `TempSqlFile` из разных проектов
- Унификация `TestFileHelper` с улучшенным API
