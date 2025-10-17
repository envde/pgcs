using System.Text;
using PgCs.Common.QueryGenerator.Models;

namespace PgCs.QueryGenerator.Formatting;

/// <summary>
/// Построитель C# кода для методов запросов
/// </summary>
internal sealed class QueryCodeBuilder
{
    private readonly StringBuilder _sb = new();
    private readonly string _indentString;
    private int _indentLevel;

    public QueryCodeBuilder(QueryGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _indentString = options.IndentationStyle == IndentationStyle.Tabs
            ? "\t"
            : new string(' ', options.IndentationSize);
    }

    public QueryCodeBuilder AppendLine(string? value = null)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _sb.Append(GetIndent());
            _sb.AppendLine(value);
        }
        else
        {
            _sb.AppendLine();
        }

        return this;
    }

    public QueryCodeBuilder Indent()
    {
        _indentLevel++;
        return this;
    }

    public QueryCodeBuilder Outdent()
    {
        if (_indentLevel > 0)
        {
            _indentLevel--;
        }

        return this;
    }

    public QueryCodeBuilder AppendXmlSummary(string summary)
    {
        if (string.IsNullOrWhiteSpace(summary))
        {
            return this;
        }

        AppendLine("/// <summary>");
        foreach (var line in WrapText(summary, 100))
        {
            AppendLine($"/// {line}");
        }
        AppendLine("/// </summary>");

        return this;
    }

    public QueryCodeBuilder AppendXmlParam(string name, string description)
    {
        AppendLine($"/// <param name=\"{name}\">{description}</param>");
        return this;
    }

    public QueryCodeBuilder AppendXmlReturns(string description)
    {
        AppendLine($"/// <returns>{description}</returns>");
        return this;
    }

    public QueryCodeBuilder AppendUsings(params string[] usings)
    {
        return AppendUsings((IEnumerable<string>)usings);
    }

    public QueryCodeBuilder AppendUsings(IEnumerable<string> usings)
    {
        var sortedUsings = usings
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Distinct()
            .OrderBy(u => u.StartsWith("System") ? 0 : 1)
            .ThenBy(u => u);

        foreach (var usingDirective in sortedUsings)
        {
            _sb.AppendLine($"using {usingDirective};");
        }

        if (sortedUsings.Any())
        {
            _sb.AppendLine();
        }

        return this;
    }

    public QueryCodeBuilder AppendNamespaceStart(string namespaceName)
    {
        _sb.AppendLine($"namespace {namespaceName};");
        _sb.AppendLine();
        return this;
    }

    public QueryCodeBuilder AppendNamespaceEnd()
    {
        // Для file-scoped namespace ничего не делаем
        return this;
    }

    public QueryCodeBuilder AppendRaw(string code)
    {
        _sb.Append(code);
        return this;
    }

    public override string ToString() => _sb.ToString();

    private string GetIndent() => string.Concat(Enumerable.Repeat(_indentString, _indentLevel));

    private static IEnumerable<string> WrapText(string text, int maxLength)
    {
        if (text.Length <= maxLength)
        {
            yield return text;
            yield break;
        }

        var words = text.Split(' ');
        var currentLine = new StringBuilder();

        foreach (var word in words)
        {
            if (currentLine.Length + word.Length + 1 > maxLength && currentLine.Length > 0)
            {
                yield return currentLine.ToString();
                currentLine.Clear();
            }

            if (currentLine.Length > 0)
            {
                currentLine.Append(' ');
            }

            currentLine.Append(word);
        }

        if (currentLine.Length > 0)
        {
            yield return currentLine.ToString();
        }
    }
}
