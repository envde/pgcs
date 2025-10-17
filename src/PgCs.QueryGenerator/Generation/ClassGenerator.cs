using PgCs.Common.QueryAnalyzer.Models.Metadata;
using PgCs.Common.QueryAnalyzer.Models.Results;
using PgCs.Common.QueryGenerator.Models;
using PgCs.QueryGenerator.Formatting;

namespace PgCs.QueryGenerator.Generation;

/// <summary>
/// Генератор C# классов с методами запросов
/// </summary>
internal sealed class ClassGenerator : IClassGenerator
{
    private readonly IMethodGenerator _methodGenerator;
    private readonly IResultModelGenerator _resultModelGenerator;

    public ClassGenerator(IMethodGenerator methodGenerator, IResultModelGenerator resultModelGenerator)
    {
        _methodGenerator = methodGenerator ?? throw new ArgumentNullException(nameof(methodGenerator));
        _resultModelGenerator = resultModelGenerator ?? throw new ArgumentNullException(nameof(resultModelGenerator));
    }

    /// <inheritdoc />
    public GeneratedClass Generate(
        IReadOnlyList<QueryMetadata> queries,
        string className,
        QueryGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(queries);
        ArgumentException.ThrowIfNullOrWhiteSpace(className);
        ArgumentNullException.ThrowIfNull(options);

        // Генерируем модели результатов и методы
        var models = new List<GeneratedModel>();
        var methods = new List<GeneratedMethod>();
        var processedModels = new HashSet<string>();

        foreach (var query in queries)
        {
            // Генерируем модель результата если нужно
            if (query.ReturnType != null && 
                query.ReturnCardinality != ReturnCardinality.Exec && 
                query.ReturnCardinality != ReturnCardinality.ExecRows)
            {
                var model = _resultModelGenerator.Generate(query, options);
                if (model != null && !processedModels.Contains(model.Name))
                {
                    models.Add(model);
                    processedModels.Add(model.Name);
                }
            }

            // Генерируем метод
            var method = _methodGenerator.Generate(query, options);
            methods.Add(method);
        }

        var code = new QueryCodeBuilder(options);
        var interfaceName = $"I{className}";

        // Using директивы
        code.AppendUsings(
            "System",
            "System.Collections.Generic",
            "System.Threading",
            "System.Threading.Tasks",
            "Npgsql"
        );
        code.AppendLine();

        // Namespace
        code.AppendNamespaceStart(options.Namespace);
        code.AppendLine();

        // Генерируем модели
        GenerateModels(code, models, options);

        // Генерируем интерфейс
        if (options.GenerateInterface)
        {
            GenerateInterface(code, interfaceName, methods, options);
            code.AppendLine();
        }

        // Генерируем класс
        GenerateClass(code, className, interfaceName, methods, options);

        code.AppendNamespaceEnd();

        return new GeneratedClass
        {
            Name = className,
            InterfaceName = options.GenerateInterface ? interfaceName : null,
            SourceCode = code.ToString(),
            Methods = methods,
            Namespace = options.Namespace
        };
    }

    /// <summary>
    /// Генерирует модели результатов
    /// </summary>
    private static void GenerateModels(QueryCodeBuilder code, IReadOnlyList<GeneratedModel> models, QueryGenerationOptions options)
    {
        foreach (var model in models)
        {
            if (options.GenerateXmlDocumentation)
            {
                code.AppendXmlSummary($"Модель результата для запроса");
            }

            code.AppendLine($"public sealed record {model.Name}");
            code.AppendLine("{");
            code.Indent();

            foreach (var prop in model.Properties)
            {
                if (options.GenerateXmlDocumentation)
                {
                    code.AppendXmlSummary(prop.Documentation ?? prop.Name);
                }

                var requiredKeyword = prop.IsRequired ? "required " : "";
                code.AppendLine($"public {requiredKeyword}{prop.CSharpType} {prop.Name} {{ get; init; }}");
            }

            code.Outdent();
            code.AppendLine("}");
            code.AppendLine();
        }
    }

    /// <summary>
    /// Генерирует интерфейс
    /// </summary>
    private static void GenerateInterface(
        QueryCodeBuilder code,
        string interfaceName,
        IReadOnlyList<GeneratedMethod> methods,
        QueryGenerationOptions options)
    {
        if (options.GenerateXmlDocumentation)
        {
            code.AppendXmlSummary($"Интерфейс для выполнения SQL запросов");
        }

        code.AppendLine($"public interface {interfaceName}");
        code.AppendLine("{");
        code.Indent();

        foreach (var method in methods)
        {
            GenerateInterfaceMethod(code, method, options);
        }

        code.Outdent();
        code.AppendLine("}");
    }

    /// <summary>
    /// Генерирует сигнатуру метода в интерфейсе
    /// </summary>
    private static void GenerateInterfaceMethod(QueryCodeBuilder code, GeneratedMethod method, QueryGenerationOptions options)
    {
        if (options.GenerateXmlDocumentation)
        {
            code.AppendXmlSummary(method.Documentation ?? $"Метод {method.Name}");

            foreach (var param in method.Parameters)
            {
                code.AppendXmlParam(param.Name, param.Documentation ?? param.Name);
            }

            code.AppendXmlReturns(GetReturnDescription(method));
        }

        var parameters = string.Join(", ", method.Parameters.Select(p => $"{p.CSharpType} {p.Name}"));
        code.AppendLine($"{method.ReturnType} {method.Name}({parameters});");
        code.AppendLine();
    }

    /// <summary>
    /// Генерирует класс
    /// </summary>
    private static void GenerateClass(
        QueryCodeBuilder code,
        string className,
        string interfaceName,
        IReadOnlyList<GeneratedMethod> methods,
        QueryGenerationOptions options)
    {
        if (options.GenerateXmlDocumentation)
        {
            code.AppendXmlSummary($"Реализация {interfaceName} для выполнения SQL запросов");
        }

        var implements = options.GenerateInterface ? $" : {interfaceName}" : "";
        code.AppendLine($"public sealed class {className}{implements}");
        code.AppendLine("{");
        code.Indent();

        // Генерируем методы
        for (int i = 0; i < methods.Count; i++)
        {
            var method = methods[i];
            code.AppendRaw(method.SourceCode);

            if (i < methods.Count - 1)
            {
                code.AppendLine();
            }
        }

        code.Outdent();
        code.AppendLine("}");
    }

    private static string GetReturnDescription(GeneratedMethod method)
    {
        if (method.ReturnType.Contains("IReadOnlyList"))
        {
            return "Список результатов";
        }

        if (method.ReturnType.Contains("int"))
        {
            return "Количество затронутых строк";
        }

        if (method.ReturnType == "ValueTask" || method.ReturnType == "Task")
        {
            return "Задача выполнения";
        }

        return "Результат выполнения";
    }
}
