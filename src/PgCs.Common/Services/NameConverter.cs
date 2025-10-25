using Humanizer;
using PgCs.Common.Utils;

namespace PgCs.Common.Services;

/// <summary>
/// Конвертер имен из PostgreSQL конвенций в C# конвенции
/// Использует библиотеку Humanizer для плюрализации
/// </summary>
public sealed class NameConverter : INameConverter
{
    public string ToClassName(string tableName)
    {
        // Преобразуем snake_case в PascalCase и делаем singular
        var pascalCase = CaseConverter.ToPascalCase(tableName);
        
        // Используем Humanizer для singularization
        return pascalCase.Singularize(inputIsKnownToBePlural: false);
    }

    public string ToPropertyName(string columnName)
    {
        return CaseConverter.ToPascalCase(columnName);
    }

    public string ToEnumMemberName(string enumValue)
    {
        // Обрабатываем UPPER_SNAKE_CASE, snake_case, kebab-case
        return CaseConverter.ToPascalCase(enumValue);
    }

    public string ToMethodName(string functionName)
    {
        return CaseConverter.ToPascalCase(functionName);
    }

    public string ToParameterName(string parameterName)
    {
        return CaseConverter.ToCamelCase(parameterName);
    }
}
