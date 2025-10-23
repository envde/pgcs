# PgCs.QueryGenerator.Tests

Unit-тесты для модуля генерации C# кода для SQL запросов.

## Статистика

- **Всего тестов:** 59
- **QueryValidatorTests:** 17 тестов
- **QueryGeneratorTests:** 6 тестов
- **QueryMethodGeneratorTests:** 21 тестов
- **QueryModelGeneratorTests:** 15 тестов

## Структура тестов

### QueryValidatorTests (17 тестов)

Тестирование валидации метаданных SQL запросов перед генерацией кода.

#### Empty Queries (1 тест)
- `Validate_EmptyQueryList_ReturnsWarning` - предупреждение при пустом списке запросов

#### Method Name Validation (3 теста)
- `Validate_EmptyMethodName_ReturnsError` - ошибка при пустом имени метода
- `Validate_DuplicateMethodNames_ReturnsError` - ошибка при дублирующихся именах методов
- `Validate_CaseInsensitiveDuplicates_ReturnsError` - ошибка при дубликатах без учета регистра

#### SQL Query Validation (2 теста)
- `Validate_EmptySqlQuery_ReturnsError` - ошибка при пустом SQL запросе
- `Validate_WhitespaceSqlQuery_ReturnsError` - ошибка при SQL запросе из пробелов

#### Parameter Validation (3 теста)
- `Validate_EmptyParameterName_ReturnsError` - ошибка при пустом имени параметра
- `Validate_DuplicateParameterNames_ReturnsWarning` - предупреждение при дублирующихся параметрах
- `Validate_MissingParameterType_ReturnsWarning` - предупреждение при отсутствии типа параметра

#### Return Type Validation (3 теста)
- `Validate_SelectWithoutReturnType_ReturnsError` - ошибка SELECT без информации о возвращаемом типе
- `Validate_SelectWithExecCardinality_AllowsMissingReturnType` - разрешает отсутствие ReturnType для Exec cardinality
- `Validate_SelectWithNoReturnColumns_ReturnsWarning` - предупреждение при отсутствии колонок в результате

#### Column Validation (2 теста)
- `Validate_EmptyColumnName_ReturnsError` - ошибка при пустом имени колонки
- `Validate_DuplicateColumnNames_ReturnsWarning` - предупреждение при дублирующихся колонках

#### Valid Queries (3 теста)
- `Validate_ValidSelectQuery_NoIssues` - валидный SELECT запрос без ошибок
- `Validate_ValidInsertQuery_NoIssues` - валидный INSERT запрос без ошибок
- `Validate_MultipleValidQueries_NoIssues` - несколько валидных запросов без ошибок

### QueryGeneratorTests (6 тестов)

Тестирование главного класса генератора кода.

#### Generate Tests (2 теста)
- `Generate_EmptyQueryList_ReturnsResultWithWarning` - обработка пустого списка запросов
- `Generate_InvalidQuery_ReturnsErrorsAndNoCode` - остановка генерации при валидационных ошибках

#### ValidateQueries Tests (4 теста)
- `ValidateQueries_ValidQueries_ReturnsNoIssues` - валидные запросы не имеют проблем
- `ValidateQueries_InvalidQueries_ReturnsIssues` - невалидные запросы возвращают ошибки
- `ValidateQueries_DuplicateMethodNames_ReturnsError` - обнаружение дублирующихся имен методов
- `ValidateQueries_EmptyList_ReturnsWarning` - предупреждение для пустого списка

### QueryMethodGeneratorTests (21 тестов)

Тестирование генератора C# методов для SQL запросов с использованием Roslyn.

#### Generate Tests (4 теста)
- `Generate_SelectQuery_ReturnsValidMethod` - генерация метода для SELECT запроса
- `Generate_InsertQuery_ReturnsMethodReturningInt` - генерация метода для INSERT
- `Generate_UpdateQuery_ReturnsMethodReturningInt` - генерация метода для UPDATE
- `Generate_DeleteQuery_ReturnsMethodReturningInt` - генерация метода для DELETE

#### Return Type Tests (2 теста)
- `Generate_SelectOne_ReturnsNullableModel` - SELECT с ReturnCardinality.One возвращает nullable
- `Generate_SelectMany_ReturnsList` - SELECT с ReturnCardinality.Many возвращает List<T>

#### Parameter Tests (3 теста)
- `Generate_WithParameters_AddsParametersToMethod` - добавление параметров в метод
- `Generate_MultipleParameters_AddsAllParameters` - добавление нескольких параметров
- `Generate_NoParameters_NoParameterStatements` - метод без параметров

#### Options Tests (3 теста)
- `Generate_WithCancellationSupport_AddsCancellationToken` - добавление CancellationToken
- `Generate_WithTransactionSupport_AddsTransaction` - добавление NpgsqlTransaction
- `Generate_WithSqlInDocumentation_IncludesSqlInComments` - SQL в XML комментариях

#### Code Structure Tests (4 теста)
- `Generate_SelectQuery_ContainsConnectionManagement` - управление connection (await using)
- `Generate_SelectQuery_ContainsCommandCreation` - создание NpgsqlCommand
- `Generate_SelectQuery_ContainsReaderExecution` - ExecuteReaderAsync и ReadAsync
- `Generate_SelectQuery_ValidCSharpSyntax` - валидный C# синтаксис (проверка Roslyn парсером)

#### Method Signature Tests (2 теста)
- `Generate_Query_ContainsMethodSignature` - генерируется MethodSignature
- `Generate_Query_SignatureMatchesSourceCode` - сигнатура соответствует коду

### QueryModelGeneratorTests (15 тестов)

Тестирование генератора моделей для результатов и параметров запросов.

#### GenerateResultModel Tests (8 тестов)
- `GenerateResultModel_ValidQuery_ReturnsSuccess` - успешная генерация модели результата
- `GenerateResultModel_NoReturnType_ReturnsFailure` - нет ReturnType - ошибка
- `GenerateResultModel_NoColumns_ReturnsFailure` - нет колонок - ошибка
- `GenerateResultModel_ValidCSharpCode` - валидный C# код
- `GenerateResultModel_ContainsProperties` - содержит свойства для колонок
- `GenerateResultModel_UsesExplicitModelName` - использует ExplicitModelName
- `GenerateResultModel_GeneratesCorrectFileType` - CodeType = ResultModel
- `GenerateResultModel_ReuseSchemaModels_ReturnsSuccessWithoutCode` - повторное использование моделей схемы

#### GenerateParameterModel Tests (4 теста)
- `GenerateParameterModel_AboveThreshold_ReturnsSuccess` - генерация при достижении threshold
- `GenerateParameterModel_BelowThreshold_ReturnsFailure` - не генерируется ниже threshold
- `GenerateParameterModel_Disabled_ReturnsFailure` - отключена генерация
- `GenerateParameterModel_GeneratesCode` - генерируется код с GetUsersParameters

#### Namespace and Type Tests (3 теста)
- `GenerateResultModel_UsesCorrectNamespace` - правильный namespace для result model
- `GenerateParameterModel_UsesCorrectNamespace` - правильный namespace для parameter model
- `GenerateResultModel_ContainsRequiredUsings` - содержит using System

## Запуск тестов

```bash
# Запуск всех тестов проекта
dotnet test tests/PgCs.QueryGenerator.Tests/PgCs.QueryGenerator.Tests.csproj

# Запуск с подробным выводом
dotnet test tests/PgCs.QueryGenerator.Tests/PgCs.QueryGenerator.Tests.csproj --verbosity detailed

# Запуск конкретного тестового класса
dotnet test tests/PgCs.QueryGenerator.Tests/PgCs.QueryGenerator.Tests.csproj --filter "FullyQualifiedName~QueryValidatorTests"

# Запуск с покрытием кода
dotnet test tests/PgCs.QueryGenerator.Tests/PgCs.QueryGenerator.Tests.csproj --collect:"XPlat Code Coverage"
```

## Тестируемые компоненты

### QueryValidator
Валидирует метаданные SQL запросов:
- Имена методов (пустые, дубликаты)
- SQL запросы (пустые, whitespace)
- Параметры (имена, типы, дубликаты)
- Типы возвращаемых значений (отсутствие для SELECT)
- Колонки результатов (имена, дубликаты)

### QueryGenerator
Главный класс генератора:
- Валидация запросов перед генерацией
- Делегирование на специализированные generators
- Обработка ошибок валидации
- Возврат структурированного результата генерации

### QueryMethodGenerator
Генератор C# методов для SQL запросов с использованием Roslyn:
- Генерация async методов для SELECT/INSERT/UPDATE/DELETE
- Управление connection и command (await using)
- Добавление параметров (AddWithValue)
- Различные типы возврата (Task<T?>, Task<List<T>>, Task<int>)
- Поддержка CancellationToken и NpgsqlTransaction
- XML документация с SQL в комментариях
- Валидный C# синтаксис (проверка Roslyn)

### QueryModelGenerator
Генератор моделей для результатов и параметров:
- Result models для возвращаемых значений SELECT
- Parameter models для запросов с большим количеством параметров
- Повторное использование моделей схемы (ReuseSchemaModels)
- Поддержка ExplicitModelName
- ParameterModelThreshold configuration
- Record types с required init properties

## Архитектура тестов

- **xUnit**: тестовый фреймворк
- **Без дополнительных библиотек**: используются только стандартные средства Assert
- **Структура**: Unit/ для unit-тестов
- **Helper methods**: для создания валидных метаданных запросов
- **Минимальная зависимость от реализации**: тесты проверяют публичные API

## Связанные проекты

- `PgCs.QueryGenerator` - генератор C# методов для SQL запросов
- `PgCs.Common` - общие модели и интерфейсы
- `PgCs.QueryAnalyzer` - анализатор SQL запросов
