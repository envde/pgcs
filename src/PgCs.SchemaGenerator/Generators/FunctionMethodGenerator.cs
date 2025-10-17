using PgCs.Common.CodeGeneration;
using PgCs.Common.SchemaAnalyzer.Models;
using PgCs.Common.SchemaAnalyzer.Models.Functions;
using PgCs.Common.SchemaGenerator.Models.Options;
using PgCs.SchemaGenerator.Services;

namespace PgCs.SchemaGenerator.Generators;

/// <summary>
/// Генератор C# методов для функций PostgreSQL
/// Пока это заглушка - полная реализация требует дополнительной архитектуры
/// </summary>
public sealed class FunctionMethodGenerator : IFunctionMethodGenerator
{
    private readonly SyntaxBuilder _syntaxBuilder;

    public FunctionMethodGenerator(SyntaxBuilder syntaxBuilder)
    {
        _syntaxBuilder = syntaxBuilder;
    }

    public ValueTask<IReadOnlyList<GeneratedCode>> GenerateAsync(
        IReadOnlyList<FunctionDefinition> functions,
        SchemaGenerationOptions options)
    {
        // TODO: Реализовать генерацию методов для функций
        // Это более сложная задача, требующая:
        // 1. Генерацию класса-репозитория
        // 2. Маппинг параметров функций в параметры методов
        // 3. Обработку return типов (scalar, table, void)
        // 4. Генерацию async методов с DbConnection/DbCommand
        
        var code = new List<GeneratedCode>();
        return ValueTask.FromResult<IReadOnlyList<GeneratedCode>>(code);
    }
}
