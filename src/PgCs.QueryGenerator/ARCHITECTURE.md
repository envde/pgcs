# Архитектура PgCs.QueryGenerator

## Обзор

PgCs.QueryGenerator генерирует типобезопасные C# методы для выполнения SQL запросов PostgreSQL. Архитектура построена на принципах SOLID с четким разделением ответственности.

## Принципы дизайна

### 1. Single Responsibility Principle (SRP)
Каждый компонент отвечает за одну задачу:
- `MethodGenerator` - генерация методов
- `ResultModelGenerator` - генерация моделей результатов
- `ClassGenerator` - генерация классов
- `NpgsqlTypeMapper` - маппинг типов

### 2. Open/Closed Principle (OCP)
Расширяемость через интерфейсы:
- `IMethodGenerator`, `IResultModelGenerator`, `IClassGenerator`
- Новые генераторы без изменения существующего кода

### 3. Liskov Substitution Principle (LSP)
Все реализации интерфейсов взаимозаменяемы через DI

### 4. Interface Segregation Principle (ISP)
Узкие специализированные интерфейсы вместо одного большого

### 5. Dependency Inversion Principle (DIP)
Зависимости через интерфейсы, инъекция в конструкторы

## Компоненты

### QueryGenerator (Facade)

Главная точка входа, координирует работу всех компонентов:

```csharp
public sealed class QueryGenerator : IQueryGenerator
{
    private readonly IMethodGenerator _methodGenerator;
    private readonly IResultModelGenerator _resultModelGenerator;
    private readonly IParameterModelGenerator _parameterModelGenerator;
    private readonly IClassGenerator _classGenerator;
    private readonly IFileWriter _fileWriter;
    private readonly ICodeValidator _codeValidator;
}
```

**Responsibilities:**
- Координация процесса генерации
- Валидация опций
- Сбор статистики
- Запись файлов

### MethodGenerator

Генерирует асинхронные методы для выполнения SQL запросов.

**Ключевые возможности:**
- Поддержка 4 типов возврата: `:one`, `:many`, `:exec`, `:execrows`
- Опциональная транзакция через параметр `NpgsqlTransaction?`
- CancellationToken для отмены операций
- XML документация на русском

**Алгоритм:**
1. Определить тип возврата (`ValueTask<T>` или `Task<T>`)
2. Построить список параметров (connection, query params, transaction, cancellationToken)
3. Сгенерировать сигнатуру метода с XML документацией
4. Сгенерировать тело метода:
   - SQL запрос как константа
   - Создание `NpgsqlCommand` с транзакцией
   - Добавление параметров с правильным `NpgsqlDbType`
   - Выполнение через `ExecuteReaderAsync` или `ExecuteNonQueryAsync`
   - Маппинг результатов в модели

### ResultModelGenerator

Генерирует record-классы для результатов SELECT запросов.

**Особенности:**
- Record types с init-only свойствами
- Required members для NOT NULL колонок
- Маппинг имён snake_case → PascalCase
- XML документация для каждого свойства
- Дедупликация моделей с одинаковыми именами

### ParameterModelGenerator

Генерирует record-классы для параметров (при 3+ параметрах).

**Логика:**
- Генерируется только если `GenerateParameterModels = true`
- Только для запросов с 3+ параметрами
- Record с required свойствами

### ClassGenerator

Генерирует класс и интерфейс с методами запросов.

**Структура:**
```
namespace MyApp.Data;

// Модели результатов
public sealed record GetUserResult { ... }

// Интерфейс
public interface IUserQueries
{
    ValueTask<GetUserResult?> GetUserAsync(...);
}

// Реализация
public sealed class UserQueries : IUserQueries
{
    public async ValueTask<GetUserResult?> GetUserAsync(...)
    {
        // Сгенерированный код
    }
}
```

### NpgsqlTypeMapper

Централизованный маппинг типов PostgreSQL ↔ C# ↔ NpgsqlDbType.

**Методы:**
- `MapToCSharpType(postgresType, isNullable)` - PostgreSQL → C#
- `MapToNpgsqlDbType(postgresType)` - PostgreSQL → NpgsqlDbType
- `GetReaderMethod(csharpType)` - C# → метод NpgsqlDataReader

**Поддерживаемые типы:**
- Численные: smallint, integer, bigint, real, double, numeric
- Строковые: text, varchar, char
- Булевы: boolean
- Дата/время: timestamp, timestamptz, date, time, interval
- Специальные: uuid, bytea, json, jsonb
- Массивы: _int4, _text, etc.

### QueryCodeBuilder

Fluent API для построения C# кода с автоматическим форматированием.

**Методы:**
- `AppendLine(string)` - добавить строку с учётом отступов
- `Indent()` / `Outdent()` - управление отступами
- `AppendXmlSummary/Param/Returns()` - XML документация
- `AppendUsings()` - using директивы с сортировкой
- `AppendNamespaceStart()` - file-scoped namespace

## Поток генерации

```
QueryMetadata (from QueryAnalyzer)
    ↓
QueryGenerator.GenerateAsync()
    ↓
    ├─→ ResultModelGenerator.Generate()     → GeneratedModel (record)
    ├─→ MethodGenerator.Generate()          → GeneratedMethod (async method)
    └─→ ClassGenerator.Generate()           → GeneratedClass (interface + class)
    ↓
FileWriter.WriteAsync()
    ↓
Generated .cs files
```

## Обработка типов возврата

### :one (Один результат)

```csharp
public async ValueTask<GetUserResult?> GetUserAsync(...)
{
    await using var reader = await cmd.ExecuteReaderAsync();
    
    if (!await reader.ReadAsync())
        return null;
    
    return new GetUserResult
    {
        Id = reader.GetInt64(0),
        Name = reader.GetString(1)
    };
}
```

### :many (Список результатов)

```csharp
public async ValueTask<IReadOnlyList<ListUsersResult>> ListUsersAsync(...)
{
    await using var reader = await cmd.ExecuteReaderAsync();
    var results = new List<ListUsersResult>();
    
    while (await reader.ReadAsync())
    {
        results.Add(new ListUsersResult { ... });
    }
    
    return results;
}
```

### :exec (Без возврата)

```csharp
public async ValueTask CreateUserAsync(...)
{
    await cmd.ExecuteNonQueryAsync();
}
```

### :execrows (Количество строк)

```csharp
public async ValueTask<int> UpdateUserAsync(...)
{
    return await cmd.ExecuteNonQueryAsync();
}
```

## Поддержка транзакций

Все методы принимают опциональный параметр `NpgsqlTransaction? transaction = null`:

```csharp
public async ValueTask<UserResult?> GetUserAsync(
    NpgsqlConnection connection,        // Обязательное подключение
    long id,                            // Параметры запроса
    NpgsqlTransaction? transaction = null,  // Опциональная транзакция
    CancellationToken cancellationToken = default)
{
    await using var cmd = new NpgsqlCommand(sql, connection, transaction);
    // ...
}
```

**Почему опциональная транзакция:**
- Простые запросы не требуют транзакций
- Пользователь сам управляет жизненным циклом транзакции
- Поддержка вложенных транзакций (savepoints)
- Гибкость в изоляции

## Производительность

### Оптимизации:

1. **ValueTask вместо Task**
   - Нет аллокаций для синхронного пути
   - Лучше для hot path

2. **NpgsqlParameter<T>**
   - Типобезопасные параметры
   - Избегаем boxing

3. **await using**
   - Автоматическая очистка ресурсов
   - Защита от утечек

4. **GetFieldValue<T> vs GetInt32/GetString**
   - `GetInt32`, `GetString` для примитивов (быстрее)
   - `GetFieldValue<T>` для сложных типов

5. **const string sql**
   - SQL как константа
   - Компилятор может оптимизировать

6. **IReadOnlyList**
   - Иммутабельность результатов
   - Лучше для API

## Расширяемость

### Добавление нового провайдера БД

1. Создать новый mapper: `MySqlTypeMapper`
2. Реализовать интерфейс `ITypeMapper`
3. Добавить в `DatabaseProvider` enum
4. Обновить `MethodGenerator` для использования правильного mapper

### Добавление нового типа генератора

1. Определить интерфейс: `ICustomGenerator`
2. Реализовать генератор: `CustomGenerator`
3. Добавить в `QueryGenerator` конструктор
4. Использовать в `GenerateAsync`

### Кастомизация форматирования

1. Наследовать от `QueryCodeBuilder`
2. Переопределить методы форматирования
3. Передать через DI в генераторы

## Тестирование

### Unit тесты:
- `MethodGenerator` - различные типы возврата
- `ResultModelGenerator` - маппинг типов
- `NpgsqlTypeMapper` - покрытие всех типов PostgreSQL
- `QueryCodeBuilder` - корректность форматирования

### Integration тесты:
- End-to-end генерация
- Компиляция сгенерированного кода
- Выполнение на реальной БД

## Зависимости

```
PgCs.QueryGenerator
    ├── PgCs.Common (интерфейсы и модели)
    ├── Npgsql 9.0.1 (доступ к БД)
    └── .NET 9.0 (C# 14 preview)
```

## Будущие улучшения

1. **Prepared Statements** - кеширование подготовленных команд
2. **Bulk Operations** - поддержка COPY для массовых операций
3. **Connection Pooling** - интеграция с NpgsqlDataSource
4. **Metrics** - сбор метрик выполнения запросов
5. **Retry Logic** - автоматические повторы при временных ошибках
6. **Query Hints** - аннотации для оптимизации запросов

## Выводы

Архитектура PgCs.QueryGenerator обеспечивает:

✅ **Типобезопасность** - компилятор проверяет корректность
✅ **Производительность** - минимальные аллокации, ValueTask
✅ **Гибкость** - транзакции, CancellationToken, async/await
✅ **Поддерживаемость** - чистый код, SOLID принципы
✅ **Расширяемость** - интерфейсы, DI, паттерны проектирования
