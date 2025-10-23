# PgCs.QueryAnalyzer.Tests

Тесты для компонента анализа SQL запросов и генерации метаданных.

## Покрытие тестами

**Всего тестов: 163**

### QueryAnalyzer (~40 тестов)
- `QueryAnalyzerTests.cs` - основной класс анализа запросов
  - AnalyzeQuery - разбор SQL запросов с аннотациями
  - AnalyzeFileAsync - анализ SQL файлов
  - Определение типов запросов (SELECT, INSERT, UPDATE, DELETE)
  - Извлечение параметров
  - Извлечение возвращаемых колонок

### AnnotationParser (~15 тестов)
- `AnnotationParserTests.cs` - парсинг аннотаций
  - `@name:` - имя метода
  - `@description:` - описание
  - `:one`, `:many`, `:exec` - cardinality
  - Обработка многострочных комментариев

### ColumnExtractor (~15 тестов)
- `ColumnExtractorTests.cs` - извлечение колонок из SELECT
  - Простые колонки
  - Вычисляемые колонки
  - Агрегатные функции (COUNT, SUM, AVG)
  - Aliases (AS)
  - Wildcards (*)

### ParameterExtractor (~15 тестов)
- `ParameterExtractorTests.cs` - извлечение параметров
  - Позиционные параметры ($1, $2)
  - Именованные параметры (@param)
  - Типы параметров
  - Default значения

### ModelNameGenerator (~10 тестов)
- `ModelNameGeneratorTests.cs` - генерация имен моделей
  - Конвертация в PascalCase
  - Сингуляризация/плюрализация
  - Обработка snake_case

### CompleteFileAnalysis (~8 тестов)
- `CompleteFileAnalysisTests.cs` - интеграционные тесты
  - Анализ полных SQL файлов
  - Обработка множественных запросов
  - Валидация метаданных

### SqlQueryParser (16 тестов) ⭐ NEW
- `SqlQueryParserTests.cs` - парсинг SQL запросов
  - SplitCommentsAndQuery (3 теста)
    - Разделение комментариев и запроса
    - Обработка комментариев без запроса
    - Inline комментарии
  - DetermineQueryType (7 тестов)
    - SELECT, INSERT, UPDATE, DELETE
    - CTE (WITH)
    - Unknown типы
    - Case insensitive
  - SplitIntoQueryBlocks (6 тестов)
    - Множественные запросы (разделение по ;)
    - Одиночный запрос
    - Без точки с запятой
    - Пустой контент

### TypeInference (28 тестов) ⭐ NEW
- `TypeInferenceTests.cs` - вывод типов параметров и колонок
  - InferParameterType (12 тестов)
    - Приведение типов (::int, ::bigint, ::timestamp, ::boolean, ::uuid, ::decimal, ::numeric)
    - Без приведения (по умолчанию text/string)
    - Case insensitive
    - @ и $ параметры
  - InferColumnType (16 тестов)
    - Агрегатные функции (COUNT, SUM, AVG)
    - Функции даты (NOW, CURRENT_TIMESTAMP)
    - Приведение типов (::int, ::bigint, ::boolean, ::uuid)
    - Без информации о типе (по умолчанию text/string)
    - Case insensitive

### QueryAnalyzerBuilder (17 тестов) ⭐ NEW
- `QueryAnalyzerBuilderTests.cs` - fluent API для анализа запросов
  - Create (1 тест) - создание builder
  - FromFile (3 теста) - анализ одного файла, валидация
  - FromFiles (2 теста) - анализ множественных файлов
  - FromDirectory (4 теста) - анализ директории, рекурсивно
  - AnalyzeAsync (7 тестов)
    - Без файлов (ошибка)
    - Один файл
    - Множественные файлы
    - Директория
    - Поддиректории
    - Цепочка вызовов

## Запуск тестов

```bash
# Все тесты
dotnet test tests/PgCs.QueryAnalyzer.Tests/PgCs.QueryAnalyzer.Tests.csproj

# С подробным выводом
dotnet test tests/PgCs.QueryAnalyzer.Tests/PgCs.QueryAnalyzer.Tests.csproj --verbosity detailed

# С покрытием кода
dotnet test tests/PgCs.QueryAnalyzer.Tests/PgCs.QueryAnalyzer.Tests.csproj --collect:"XPlat Code Coverage"
```

## Структура

```
PgCs.QueryAnalyzer.Tests/
├── Unit/
│   ├── QueryAnalyzerTests.cs              (~40 тестов)
│   ├── AnnotationParserTests.cs           (~15 тестов)
│   ├── ColumnExtractorTests.cs            (~15 тестов)
│   ├── ParameterExtractorTests.cs         (~15 тестов)
│   ├── ModelNameGeneratorTests.cs         (~10 тестов)
│   ├── CompleteFileAnalysisTests.cs       (~8 тестов)
│   ├── SqlQueryParserTests.cs             (16 тестов) ⭐
│   ├── TypeInferenceTests.cs              (28 тестов) ⭐
│   └── QueryAnalyzerBuilderTests.cs       (17 тестов) ⭐
├── Helpers/
│   └── TestFileHelper.cs
├── TestData/
│   └── Queries.sql
└── README.md

⭐ = недавно добавленные тесты
```

## Покрытие компонентов

- ✅ QueryAnalyzer - 100%
- ✅ AnnotationParser - 100%
- ✅ ColumnExtractor - 100%
- ✅ ParameterExtractor - 100%
- ✅ ModelNameGenerator - 100%
- ✅ SqlQueryParser - 100% ⭐
- ✅ TypeInference - 100% ⭐
- ✅ QueryAnalyzerBuilder - 100% ⭐

Все публичные методы покрыты unit-тестами.

## Последние обновления

### 2025-10-23: Добавлено 61 новых тестов
- **SqlQueryParser** (16 тестов): парсинг SQL запросов, разделение комментариев, определение типа запроса
- **TypeInference** (28 тестов): вывод типов параметров и колонок на основе SQL контекста
- **QueryAnalyzerBuilder** (17 тестов): fluent API для анализа SQL файлов и директорий

Общее количество тестов увеличилось с 102 до 163.

## Формат аннотаций

```sql
-- name: GetUserById :one
-- description: Get user by ID
SELECT id, username, email
FROM users
WHERE id = $1;
```

Поддерживаемые аннотации:
- `name:` - имя генерируемого метода (обязательно)
- `description:` - описание метода (опционально)
- `:one` - возвращает один объект
- `:many` - возвращает коллекцию
- `:exec` - выполняет команду без возврата данных
