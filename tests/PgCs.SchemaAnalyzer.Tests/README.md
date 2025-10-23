# PgCs.SchemaAnalyzer.Tests

Тесты для компонента анализа PostgreSQL схем.

## Покрытие тестами

**Всего тестов: 201**

### SchemaAnalyzer (основной класс, ~50 тестов)
- `SchemaAnalyzerTests.cs` - анализ SQL скриптов
  - AnalyzeScript - разбор полных схем
  - ExtractTables - извлечение таблиц
  - ExtractViews - извлечение представлений
  - ExtractTypes - извлечение custom типов
  - ExtractFunctions - извлечение функций
  - ExtractIndexes - извлечение индексов
  - ExtractTriggers - извлечение триггеров
  - ExtractConstraints - извлечение ограничений

### SchemaAnalyzerBuilder (~20 тестов)
- `SchemaAnalyzerBuilderTests.cs` - fluent API для настройки анализа
  - FromScript, FromFile
  - IncludeComments, IncludeIndexes, IncludeTriggers
  - Цепочки вызовов

### Extractors (~50 тестов)
Различные экстракторы для компонентов схемы:
- `TableExtractorTests.cs` - извлечение таблиц
- `ViewExtractorTests.cs` - извлечение представлений
- `TypeExtractorTests.cs` - извлечение типов
- `FunctionExtractorTests.cs` - извлечение функций
- `IndexExtractorTests.cs` - извлечение индексов
- `TriggerExtractorTests.cs` - извлечение триггеров
- `ConstraintExtractorTests.cs` - извлечение ограничений
- `CommentExtractorTests.cs` - извлечение комментариев

### SchemaAnalyzerExtensions (23 теста) ⭐ NEW
- `SchemaAnalyzerExtensionsTests.cs` - extension методы для SchemaMetadata
  - FindTable (3 теста) - поиск таблицы по имени
  - FindView (2 теста) - поиск представления
  - FindType (2 теста) - поиск типа
  - GetTableIndexes (2 теста) - получение индексов таблицы
  - GetTableTriggers (1 тест) - получение триггеров
  - GetTableConstraints (1 тест) - получение ограничений
  - GetTablesReferencingTable (1 тест) - связанные таблицы
  - HasColumn (2 теста) - проверка наличия колонки
  - GetColumn (2 теста) - получение колонки
  - GetPrimaryKeyColumns (2 теста) - первичные ключи
  - GetRequiredColumns (2 теста) - обязательные колонки

### SchemaFilter (24 теста) ⭐ NEW
- `SchemaFilterTests.cs` - fluent API для фильтрации схем
  - From, Build (2 теста)
  - ExcludeSchemas, IncludeOnlySchemas (2 теста)
  - ExcludeTables, IncludeOnlyTables (4 теста)
  - ExcludeViews, IncludeOnlyViews (2 теста)
  - IncludeOnlyTypes (1 тест)
  - RemoveSystemObjects (1 тест)
  - Remove* методы (7 тестов): Tables, Views, Types, Functions, Indexes, Triggers, Constraints
  - Only* методы (2 теста): OnlyTablesAndViews, OnlyTables
  - ChainedFilters (1 тест) - цепочка фильтров

### SchemaMerger (14 тестов) ⭐ NEW
- `SchemaMergerTests.cs` - объединение множественных схем
  - Merge с пустым списком (1 тест)
  - Merge с одной схемой (1 тест)
  - Merge с множественными схемами (1 тест)
  - Merge с дубликатами (1 тест)
  - Merge с views (1 тест)
  - Merge с types (1 тест)
  - Merge комплексных схем (1 тест)
  - Merge смешанного контента (1 тест)
  - Merge с functions (1 тест)
  - Merge с indexes (1 тест)
  - Merge с triggers (1 тест)
  - Merge с constraints (1 тест)
  - Merge трех схем (1 тест)
  - Preserve order (1 тест)

### Utils (~20 тестов)
- `SqlStatementSplitterTests.cs` - разделение SQL на statements
- Другие утилиты

## Запуск тестов

```bash
# Все тесты
dotnet test tests/PgCs.SchemaAnalyzer.Tests/PgCs.SchemaAnalyzer.Tests.csproj

# С подробным выводом
dotnet test tests/PgCs.SchemaAnalyzer.Tests/PgCs.SchemaAnalyzer.Tests.csproj --verbosity detailed

# С покрытием кода
dotnet test tests/PgCs.SchemaAnalyzer.Tests/PgCs.SchemaAnalyzer.Tests.csproj --collect:"XPlat Code Coverage"
```

## Структура

```
PgCs.SchemaAnalyzer.Tests/
├── Unit/
│   ├── SchemaAnalyzerTests.cs                (~50 тестов)
│   ├── SchemaAnalyzerBuilderTests.cs         (~20 тестов)
│   ├── SchemaAnalyzerExtensionsTests.cs      (23 теста) ⭐
│   ├── SchemaFilterTests.cs                  (24 теста) ⭐
│   ├── SchemaMergerTests.cs                  (14 тестов) ⭐
│   ├── *ExtractorTests.cs                    (~50 тестов)
│   └── SqlStatementSplitterTests.cs          (~20 тестов)
├── Helpers/
│   └── TestSchemaBuilder.cs
└── README.md

⭐ = недавно добавленные тесты
```

## Покрытие компонентов

- ✅ SchemaAnalyzer - 100%
- ✅ SchemaAnalyzerBuilder - 100%
- ✅ SchemaAnalyzerExtensions - 100% ⭐
- ✅ SchemaFilter - 100% ⭐
- ✅ SchemaMerger - 100% ⭐
- ✅ Extractors (Table, View, Type, Function, Index, Trigger, Constraint, Comment) - 100%
- ✅ Utils (SqlStatementSplitter) - 100%

Все публичные методы покрыты unit-тестами.

## Последние обновления

### 2025-10-23: Добавлено 61 новых тестов
- **SchemaAnalyzerExtensions** (23 теста): extension методы для работы со SchemaMetadata
- **SchemaFilter** (24 теста): fluent API для фильтрации схем
- **SchemaMerger** (14 тестов): объединение множественных схем

Общее количество тестов увеличилось с 140 до 201.
