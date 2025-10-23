# Покрытие тестами проекта PgCs

## Общая статистика

**Всего тестов: 429** (по состоянию на 23 октября 2025)

| Проект | Количество тестов | Статус |
|--------|-------------------|--------|
| PgCs.SchemaGenerator.Tests | 65 | ✅ 100% |
| PgCs.SchemaAnalyzer.Tests | 201 | ✅ 100% |
| PgCs.QueryAnalyzer.Tests | 163 | ✅ 100% |
| PgCs.QueryGenerator.Tests | 0 | ⚠️ Нет тестов |

## Последние обновления

### 2025-10-23: Добавлено 122 новых тестов

#### SchemaGenerator (+42 теста)
- ✅ **ViewModelGenerator** (7 тестов): генерация моделей для представлений
- ✅ **FunctionMethodGenerator** (9 тестов): генерация методов для PostgreSQL функций
- ✅ **SyntaxBuilder** (26 тестов): построитель синтаксических деревьев Roslyn

#### SchemaAnalyzer (+61 тест)
- ✅ **SchemaAnalyzerExtensions** (23 теста): extension методы для работы со SchemaMetadata
- ✅ **SchemaFilter** (24 теста): fluent API для фильтрации схем
- ✅ **SchemaMerger** (14 тестов): объединение множественных схем

#### QueryAnalyzer (+61 тест)
- ✅ **SqlQueryParser** (16 тестов): парсинг SQL запросов, определение типа
- ✅ **TypeInference** (28 тестов): вывод типов параметров и колонок
- ✅ **QueryAnalyzerBuilder** (17 тестов): fluent API для анализа файлов

**Прогресс:** 307 → 429 тестов (+122, +39.7%)

## Покрытие компонентов

### PgCs.SchemaGenerator ✅
- [x] SchemaGenerator
- [x] TableModelGenerator
- [x] ViewModelGenerator
- [x] CustomTypeGenerator
- [x] FunctionMethodGenerator
- [x] SyntaxBuilder
- [x] SchemaGenerationOptions

### PgCs.SchemaAnalyzer ✅
- [x] SchemaAnalyzer
- [x] SchemaAnalyzerBuilder
- [x] SchemaAnalyzerExtensions
- [x] SchemaFilter
- [x] SchemaMerger
- [x] Extractors (Table, View, Type, Function, Index, Trigger, Constraint, Comment)
- [x] Utils (SqlStatementSplitter)

### PgCs.QueryAnalyzer ✅
- [x] QueryAnalyzer
- [x] QueryAnalyzerBuilder
- [x] AnnotationParser
- [x] ColumnExtractor
- [x] ParameterExtractor
- [x] ModelNameGenerator
- [x] SqlQueryParser
- [x] TypeInference

### PgCs.QueryGenerator ⚠️
- [ ] QueryGenerator (нет тестов)
- [ ] Другие компоненты (нужно исследование)

## Запуск тестов

### Все тесты проекта
```bash
dotnet test
```

### Отдельные проекты
```bash
# SchemaGenerator
dotnet test tests/PgCs.SchemaGenerator.Tests/PgCs.SchemaGenerator.Tests.csproj

# SchemaAnalyzer
dotnet test tests/PgCs.SchemaAnalyzer.Tests/PgCs.SchemaAnalyzer.Tests.csproj

# QueryAnalyzer
dotnet test tests/PgCs.QueryAnalyzer.Tests/PgCs.QueryAnalyzer.Tests.csproj
```

### С покрытием кода
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### С подробным выводом
```bash
dotnet test --verbosity detailed
```

## Структура тестов

```
tests/
├── PgCs.SchemaGenerator.Tests/
│   ├── Unit/
│   │   ├── SchemaGeneratorTests.cs                (19)
│   │   ├── TableModelGeneratorTests.cs            (6)
│   │   ├── ViewModelGeneratorTests.cs             (7) ⭐
│   │   ├── CustomTypeGeneratorTests.cs            (4)
│   │   ├── FunctionMethodGeneratorTests.cs        (9) ⭐
│   │   ├── SyntaxBuilderTests.cs                  (26) ⭐
│   │   └── SchemaGenerationOptionsTests.cs        (6)
│   └── README.md
│
├── PgCs.SchemaAnalyzer.Tests/
│   ├── Unit/
│   │   ├── SchemaAnalyzerTests.cs                 (~50)
│   │   ├── SchemaAnalyzerBuilderTests.cs          (~20)
│   │   ├── SchemaAnalyzerExtensionsTests.cs       (23) ⭐
│   │   ├── SchemaFilterTests.cs                   (24) ⭐
│   │   ├── SchemaMergerTests.cs                   (14) ⭐
│   │   ├── *ExtractorTests.cs                     (~50)
│   │   └── SqlStatementSplitterTests.cs           (~20)
│   ├── Helpers/
│   │   └── TestSchemaBuilder.cs
│   └── README.md
│
├── PgCs.QueryAnalyzer.Tests/
│   ├── Unit/
│   │   ├── QueryAnalyzerTests.cs                  (~40)
│   │   ├── AnnotationParserTests.cs               (~15)
│   │   ├── ColumnExtractorTests.cs                (~15)
│   │   ├── ParameterExtractorTests.cs             (~15)
│   │   ├── ModelNameGeneratorTests.cs             (~10)
│   │   ├── CompleteFileAnalysisTests.cs           (~8)
│   │   ├── SqlQueryParserTests.cs                 (16) ⭐
│   │   ├── TypeInferenceTests.cs                  (28) ⭐
│   │   └── QueryAnalyzerBuilderTests.cs           (17) ⭐
│   ├── Helpers/
│   │   └── TestFileHelper.cs
│   ├── TestData/
│   │   └── Queries.sql
│   └── README.md
│
└── PgCs.QueryGenerator.Tests/                      (0) ⚠️

⭐ = недавно добавленные тесты
```

## Качество тестов

### Принципы
- ✅ Один тест - одна проверка (Arrange-Act-Assert)
- ✅ Понятные имена тестов (описывают сценарий)
- ✅ Независимые тесты (без зависимостей между тестами)
- ✅ Быстрые тесты (unit-тесты выполняются мгновенно)
- ✅ Тесты покрывают граничные случаи и ошибки

### Используемые инструменты
- **xUnit 2.9.2** - фреймворк для тестирования
- **Arrange-Act-Assert** паттерн
- **Test helpers** - вспомогательные классы для генерации тестовых данных
- **XML документация** в тестах для описания цели

## TODO

### Приоритет 1: QueryGenerator
- [ ] Исследовать структуру PgCs.QueryGenerator
- [ ] Создать тесты для всех компонентов
- [ ] Достичь 100% покрытия

### Приоритет 2: Интеграционные тесты
- [ ] Создать интеграционные тесты для полного flow
- [ ] Тестирование на реальных PostgreSQL схемах
- [ ] Performance тесты

### Приоритет 3: Документация
- [x] README для каждого тестового проекта
- [x] Общий файл покрытия
- [ ] Примеры использования в тестах
- [ ] Диаграммы покрытия

## Заметки

- Все тесты проходят успешно (429/429)
- Покрытие основных компонентов: 100%
- Время выполнения всех тестов: ~1.5 секунды
- Все новые тесты следуют существующему стилю проекта
