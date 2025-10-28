# PgCs Schema Analyzer - Implementation Summary

## Обзор

Реализована инфраструктура для анализа схемы PostgreSQL с поддержкой извлечения ENUM типов. Архитектура спроектирована с учётом расширяемости для других типов объектов (Tables, Views, Domains, Composites и т.д.).

## Реализованные компоненты

### 1. Общая инфраструктура парсинга (PgCs.Core/Parsing)

#### `SqlBlock.cs`
Представляет блок SQL кода - одну команду PostgreSQL с полным контекстом:
- `Content` - основной SQL текст без комментариев
- `RawContent` - полный исходный текст включая комментарии
- `PrecedingComment` - комментарий перед блоком
- `InlineComments` - inline комментарии внутри блока
- `StartLine`, `EndLine` - позиция в файле
- `FilePath` - путь к источнику

#### `ISqlBlockParser.cs` / `SqlBlockParser.cs`
Парсер SQL скрипта на блоки команд с поддержкой:
- Разделения команд по пустым строкам или точке с запятой (`;`)
- Извлечения preceding комментариев (только непосредственно перед блоком)
- Обработки inline комментариев (`--`)
- Корректной работы с многострочными командами
- Умного сброса accumulated комментариев при пустых строках

**Ключевые особенности:**
- Блок завершается пустой строкой ИЛИ точкой с запятой
- Комментарии отделённые пустой строкой от команды не включаются
- Поддержка регулярных выражений C# 14 (source generators)

### 2. Контекст парсинга (PgCs.Core/Schema/Analyzer)

#### `ParsingContext.cs`
Предоставляет контекст с доступом к соседним блокам:
- `CurrentBlock` - текущий блок
- `NextBlock` - следующий блок (для поиска COMMENT ON)
- `PreviousBlock` - предыдущий блок
- `GetNextBlocks(count)` - несколько следующих блоков

Это позволяет парсерам смотреть вперёд для связывания, например, `CREATE TYPE` с `COMMENT ON TYPE`.

### 3. Парсер ENUM типов

#### `IEnumTypeParser.cs` (PgCs.Core)
Интерфейс для извлечения ENUM типов с методом:
```csharp
bool TryParse(ParsingContext context, 
              out EnumTypeDefinition? enumDefinition, 
              out IReadOnlyList<ValidationIssue> validationIssues);
```

#### `EnumTypeParser.cs` (PgCs.SchemaAnalyzer.Tante/Parsers)
Реализация парсера ENUM с поддержкой:
- Регулярных выражений для `CREATE TYPE ... AS ENUM`
- Извлечения схемы и имени типа
- Парсинга значений ENUM (с кавычками или без)
- Поиска `COMMENT ON TYPE` в следующем блоке
- Fallback на preceding comment если COMMENT ON TYPE не найден
- Генерации ValidationIssue при ошибках

**Поддерживаемые форматы:**
```sql
-- Комментарий перед
CREATE TYPE status AS ENUM ('active', 'inactive');

-- или

CREATE TYPE status AS ENUM ('active', 'inactive');
COMMENT ON TYPE status IS 'Статус пользователя';

-- или с схемой

CREATE TYPE public.status AS ENUM ('active', 'inactive');
COMMENT ON TYPE public.status IS 'Статус пользователя';
```

### 4. Обновление ValidationIssue

Добавлены новые типы объектов в `ValidationDefinitionType`:
- `Enum` - ENUM тип
- `Composite` - Composite тип
- `Domain` - Domain тип
- `Query` - SQL запрос

### 5. SchemaAnalyzer (PgCs.SchemaAnalyzer.Tante)

#### Реализованные методы:

**`AnalyzeFileAsync`**
- Читает SQL файл асинхронно
- Парсит на блоки через `SqlBlockParser`
- Извлекает ENUM типы через `EnumTypeParser`
- Собирает ValidationIssues
- Возвращает `SchemaMetadata`

**`AnalyzeDirectoryAsync`**
- Сканирует директорию рекурсивно (`*.sql`)
- Обрабатывает каждый файл
- Объединяет результаты
- Возвращает общую `SchemaMetadata`

**`ExtractEnums`**
- Публичный метод для извлечения ENUM из SQL скрипта
- Использует внутренний `ExtractEnumsInternal` с поддержкой filePath

## Архитектурные принципы

1. **Один файл - один класс** - каждый класс в отдельном файле
2. **Минимум приватных методов** - для упрощения тестирования
3. **Интерфейсы для расширяемости** - `ISqlBlockParser`, `IEnumTypeParser`
4. **Immutable модели** - использование `record` и `required` свойств
5. **Эффективный код** - source generated регулярные выражения, `Span<T>` где возможно
6. **Контекст для парсеров** - `ParsingContext` предоставляет доступ к соседним блокам

## Использование

```csharp
using PgCs.SchemaAnalyzer.Tante;

var analyzer = new SchemaAnalyzer();

// Анализ одного файла
var metadata = await analyzer.AnalyzeFileAsync("schema.sql");

// Анализ директории
var metadata = await analyzer.AnalyzeDirectoryAsync("./schemas");

// Извлечение только ENUM
var enums = analyzer.ExtractEnums(sqlScript);

// Работа с результатами
foreach (var enumType in metadata.Enums)
{
    Console.WriteLine($"ENUM: {enumType.Name}");
    Console.WriteLine($"Values: {string.Join(", ", enumType.Values)}");
    Console.WriteLine($"Comment: {enumType.Comment}");
}

// Проверка ValidationIssues
foreach (var issue in metadata.ValidationIssues)
{
    Console.WriteLine($"[{issue.Severity}] {issue.Code}: {issue.Message}");
}
```

## Тестирование

Запуск CLI для тестирования:
```bash
dotnet run --project src/PgCs.Cli/PgCs.Cli.csproj src/PgCs.Core/Example/Schema.sql
```

Результат:
```
📦 ENUM: user_status
   Values: [active, inactive, suspended, deleted]
   Comment: Возможные статусы пользователя в системе

📦 ENUM: order_status
   Values: [pending, processing, shipped, delivered, cancelled]
   Comment: Статусы жизненного цикла заказа

...
```

## Следующие шаги

Для добавления поддержки других типов объектов:

1. Создать интерфейс парсера в `PgCs.Core/Schema/Analyzer` (например, `ITableParser`)
2. Реализовать парсер в `PgCs.SchemaAnalyzer.Tante/Parsers`
3. Добавить тип в `ValidationDefinitionType` если нужно
4. Обновить `SchemaAnalyzer` добавив соответствующий метод Extract
5. Интегрировать в `AnalyzeFileAsync` и `AnalyzeDirectoryAsync`

**Общая инфраструктура готова и переиспользуема:**
- `SqlBlockParser` - парсинг любых SQL команд
- `ParsingContext` - контекст для любых парсеров
- `ValidationIssue` - единая система валидации
- `SchemaMetadata` - общая модель результата

## Совместимость

- C# 14 features
- .NET 9
- PostgreSQL 18 syntax
- Async/await patterns
- Source generators для regex
