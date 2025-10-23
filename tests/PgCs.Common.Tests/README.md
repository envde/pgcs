# PgCs.Common.Tests

Тесты для проекта `PgCs.Common` — общих компонентов PgCs (утилиты, маппинг типов, форматирование кода, builders).

## 📊 Статистика

**Всего тестов**: 126  
**Структура**:
- `NameConverterTests`: 25 тестов
- `PostgreSqlTypeMapperTests`: 37 тестов
- `RoslynFormatterTests`: 12 тестов
- `NameConversionStrategyBuilderTests`: 32 тестов
- `TypeMapperBuilderTests`: 20 тестов

## 🧪 Тестируемые компоненты

### NameConverter (25 тестов)
Конвертация имен из PostgreSQL конвенций в C# конвенции:

- **ToClassName** (4 теста): `user_profiles` → `UserProfile`, `users` → `User`
- **ToPropertyName** (4 теста): `user_id` → `UserId`, `email` → `Email`
- **ToEnumMemberName** (3 теста): `active_user` → `ActiveUser`, `PENDING_APPROVAL` → `PendingApproval`
- **ToMethodName** (2 теста): `get_user_by_id` → `GetUserById`
- **ToParameterName** (3 теста): `user_id` → `userId`, `first_name` → `firstName`
- **Edge Cases** (9 тестов): пустые строки, пробелы, смешанные разделители, числа

**Ключевые возможности**:
- Поддержка `snake_case`, `UPPER_SNAKE_CASE`, `kebab-case`, spaces
- Singularization через Humanizer (`users` → `User`)
- Обработка множественных разделителей

### NameConversionStrategyBuilder (32 теста)
Fluent builder для кастомизации правил конвертации имен:

- **Case Style Configuration** (6 тестов): UsePascalCase/UseCamelCase для classes, properties, methods, parameters
- **Singularization / Pluralization** (2 теста): SingularizeClassNames, DoNotSingularizeClassNames
- **Prefix / Suffix Removal** (4 теста): RemovePrefix ("tbl_", "v_"), RemoveSuffix ("_table", "_tmp")
- **Custom Rules** (4 теста): AddCustomRule (regex), AddTransformation (custom functions)
- **Quick Presets** (8 тестов): 
  - UseStandardCSharpConventions
  - UseCamelCaseEverywhere
  - UsePascalCaseEverywhere
  - RemoveStandardTablePrefixes
  - RemoveStandardViewPrefixes
  - RemoveStandardProcedurePrefixes
- **Complex Scenarios** (2 теста): Combined configuration, chained configuration
- **Edge Cases** (5 тестов): Empty prefix, null transformation, invalid culture, enum members
- **Enum Member Names** (1 тест): Always PascalCase

**Ключевые возможности**:
- Fluent API для настройки конвертации
- Regex правила и custom трансформации
- Preset конфигурации для быстрой настройки
- Префикс/суффикс removal для legacy схем

### PostgreSqlTypeMapper (37 тестов)
Маппинг PostgreSQL типов в C# типы:

- **Numeric Types** (7 тестов): `smallint` → `short`, `integer` → `int`, `bigint` → `long`, `decimal`, `real` → `float`, `double precision` → `double`, `money` → `decimal`
- **String Types** (3 теста): `varchar` → `string`, `text` → `string`, `char` → `string`
- **DateTime Types** (5 тестов): `timestamp` → `DateTime`, `timestamptz` → `DateTimeOffset`, `date` → `DateOnly`, `time` → `TimeOnly`, `interval` → `TimeSpan`
- **Boolean and UUID** (2 теста): `boolean` → `bool`, `uuid` → `Guid`
- **Binary and JSON** (3 теста): `bytea` → `byte[]`, `json`/`jsonb` → `string`, `xml` → `string`
- **Array Types** (3 теста): `integer[]`, `text[]`, `boolean[]`
- **Type with Parameters** (3 теста): `varchar(100)`, `numeric(10,2)`, `char(10)`
- **Network Types** (3 теста): `inet` → `System.Net.IPAddress`, `cidr`, `macaddr` → `System.Net.NetworkInformation.PhysicalAddress`
- **Range Types** (3 теста): `int4range` → `NpgsqlRange<int>`, `int8range` → `NpgsqlRange<long>`, `daterange` → `NpgsqlRange<DateOnly>`
- **Geometry Types** (1 тест): `geometry`, `geography`, `point` → `object`
- **Unknown Types** (1 тест): fallback to `object`
- **GetRequiredNamespace** (6 тестов): `uuid` → `System`, `inet` → `System.Net`, `macaddr` → `System.Net.NetworkInformation`, `int4range` → `NpgsqlTypes`, `bit` → `System.Collections`
- **Case Insensitivity** (2 теста): `INTEGER`, `VARCHAR`, `UUID`

**Покрытие**: 80+ PostgreSQL типов

### TypeMapperBuilder (20 тестов)
Fluent builder для кастомизации маппинга типов:

- **Basic Custom Mappings** (2 теста): MapType, MapTypes (multiple)
- **Namespace Configuration** (2 теста): AddNamespace, AddNamespaces
- **Default Type Configuration** (2 теста): WithDefaultTypeForUnknown
- **Standard Mappings Control** (2 теста): ClearStandardMappings, UseStandardMappings
- **JSON Presets** (3 теста): UseSystemTextJson, UseNewtonsoftJson, UseStringForJson
- **NodaTime Preset** (1 тест): UseNodaTime (Instant, LocalDate, LocalTime, Period)
- **NetTopologySuite Preset** (1 тест): UseNetTopologySuite (Geometry, Point, Polygon)
- **Nullable and Array Handling** (3 теста): Custom mappings with nullable/arrays
- **Complex Scenarios** (2 теста): Combined configuration, chained presets
- **Type Parameter Cleaning** (1 тест): varchar(100) → varchar
- **Edge Cases** (8 тестов): Null validations, case insensitivity

**Ключевые возможности**:
- Кастомные маппинги PostgreSQL → C#
- Preset конфигурации (System.Text.Json, NodaTime, NetTopologySuite)
- Namespace management
- Fallback на default тип для unknown types

### RoslynFormatter (12 тестов)
Форматирование C# кода через Roslyn:

- **Valid Code** (4 теста): class, method, property, complex structures
- **Already Formatted** (1 тест): идемпотентность форматирования
- **Error Handling** (2 теста): invalid syntax, whitespace-only
- **Complex Scenarios** (5 тестов): namespace, usings, multiple classes, nested types, records

**Ключевые возможности**:
- Graceful error handling (возвращает исходный код при ошибке)
- Поддержка C# 11 features (records, etc.)

## 🏃 Запуск тестов

```bash
# Все тесты Common
dotnet test tests/PgCs.Common.Tests

# С детальным выводом
dotnet test tests/PgCs.Common.Tests --verbosity detailed

# С покрытием кода
dotnet test tests/PgCs.Common.Tests --collect:"XPlat Code Coverage"
```

## 📁 Структура

```
PgCs.Common.Tests/
├── GlobalUsings.cs                                # Глобальные using directives
└── Unit/
    ├── NameConverterTests.cs                      # 25 тестов
    ├── NameConversionStrategyBuilderTests.cs      # 32 теста
    ├── PostgreSqlTypeMapperTests.cs               # 37 тестов
    ├── TypeMapperBuilderTests.cs                  # 20 тестов
    └── RoslynFormatterTests.cs                    # 12 тестов
```

## ✅ Результаты

Все 126 тестов проходят успешно:

```
Test summary: total: 126, failed: 0, succeeded: 126, skipped: 0
```

## 🔧 Зависимости

- **xUnit** 2.9.2 - тестовый фреймворк
- **PgCs.Common** - тестируемый проект
- **Humanizer** (через Common) - для singularization
- **Microsoft.CodeAnalysis** (через Common) - для Roslyn formatting

## 📝 Заметки

- `byte[]` является reference type, поэтому nullable не добавляет `?`
- Roslyn форматирует whitespace-only строки в минимальную форму (`\n`)
- NameConverter использует Humanizer.Singularize() для plurals → singular
- PostgreSqlTypeMapper поддерживает type parameters (`varchar(100)` → `varchar`)
- Case-insensitive маппинг типов (`INTEGER` == `integer`)
