using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace PgCs.Common.Services;

/// <summary>
/// Форматировщик C# кода с использованием Roslyn
/// </summary>
public sealed class RoslynFormatter : IRoslynFormatter
{
    private static readonly AdhocWorkspace Workspace = new();

    public string Format(string sourceCode)
    {
        try
        {
            // Парсим код в синтаксическое дерево
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var root = syntaxTree.GetRoot();

            // Форматируем с использованием Roslyn formatter
            var formattedRoot = Formatter.Format(root, Workspace);

            // Возвращаем отформатированный код
            var formattedCode = formattedRoot.ToFullString();
            return formattedCode;
        }
        catch
        {
            // Если форматирование не удалось, возвращаем исходный код
            return sourceCode;
        }
    }
}
