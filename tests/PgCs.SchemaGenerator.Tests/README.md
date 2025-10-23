# PgCs.SchemaGenerator.Tests

Тесты для компонента генерации C# кода из PostgreSQL схем.

## Покрытие тестами

**Всего тестов: 65**

### SchemaGenerator (19 тестов)
- `SchemaGeneratorTests.cs` - основной класс генерации кода
  - Генерация моделей таблиц
  - Генерация enum типов
  - Генерация методов функций
  - Обработка опций генерации

### TableModelGenerator (6 тестов)
- `TableModelGeneratorTests.cs` - генератор моделей для таблиц
  - Генерация классов с различными типами колонок
  - Обработка nullable/required полей
  - XML документация

### ViewModelGenerator (7 тестов)
- `ViewModelGeneratorTests.cs` - генератор моделей для представлений
  - Конвертация имен (snake_case → PascalCase)
  - Сингуляризация (posts → Post)
  - XML документация
  - Namespace

### CustomTypeGenerator (4 теста)
- `CustomTypeGeneratorTests.cs` - генератор enum типов
  - Генерация C# enum из PostgreSQL enum
  - XML документация
  - Обработка значений

### FunctionMethodGenerator (9 тестов)
- `FunctionMethodGeneratorTests.cs` - генератор методов для PostgreSQL функций
  - Генерация repository классов
  - Async Task методы с NpgsqlConnection
  - Обработка параметров функций
  - Генерация SQL запросов

### SyntaxBuilder (26 тестов)
- `SyntaxBuilderTests.cs` - построитель синтаксических деревьев Roslyn
  - BuildTableClass (5 тестов)
  - BuildProperty (4 теста)
  - BuildEnum (4 теста)
  - BuildCompilationUnit (3 теста)
  - GetRequiredUsings (3 теста)
  - BuildNamespace, BuildMethodDeclaration, BuildParameterList

### SchemaGenerationOptions (6 тестов)
- `SchemaGenerationOptionsTests.cs` - опции конфигурации
  - Validate метод
  - Default значения
  - Проверка обязательных полей

## Запуск тестов

```bash
# Все тесты
dotnet test tests/PgCs.SchemaGenerator.Tests/PgCs.SchemaGenerator.Tests.csproj

# С подробным выводом
dotnet test tests/PgCs.SchemaGenerator.Tests/PgCs.SchemaGenerator.Tests.csproj --verbosity detailed

# С покрытием кода
dotnet test tests/PgCs.SchemaGenerator.Tests/PgCs.SchemaGenerator.Tests.csproj --collect:"XPlat Code Coverage"
```

## Структура

```
PgCs.SchemaGenerator.Tests/
├── Unit/
│   ├── SchemaGeneratorTests.cs          (19 тестов)
│   ├── TableModelGeneratorTests.cs      (6 тестов)
│   ├── ViewModelGeneratorTests.cs       (7 тестов)
│   ├── CustomTypeGeneratorTests.cs      (4 теста)
│   ├── FunctionMethodGeneratorTests.cs  (9 тестов)
│   ├── SyntaxBuilderTests.cs            (26 тестов)
│   └── SchemaGenerationOptionsTests.cs  (6 тестов)
└── README.md
```

## Покрытие компонентов

- ✅ SchemaGenerator - 100%
- ✅ TableModelGenerator - 100%
- ✅ ViewModelGenerator - 100%
- ✅ CustomTypeGenerator - 100%
- ✅ FunctionMethodGenerator - 100%
- ✅ SyntaxBuilder - 100%
- ✅ SchemaGenerationOptions - 100%

Все публичные методы покрыты unit-тестами.
