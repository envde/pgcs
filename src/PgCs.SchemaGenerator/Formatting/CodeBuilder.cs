using System.Text;
using PgCs.Common.SchemaGenerator.Models;

namespace PgCs.SchemaGenerator.Formatting;

/// <summary>
/// Построитель C# кода с поддержкой отступов и форматирования
/// </summary>
internal sealed class CodeBuilder
{
    private readonly StringBuilder _sb = new();
    private readonly string _indentString;
    private int _indentLevel;

    public CodeBuilder(SchemaGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _indentString = options.IndentationStyle == IndentationStyle.Tabs
            ? "\t"
            : new string(' ', options.IndentationSize);
    }

    /// <summary>
    /// Добавляет строку с текущим отступом
    /// </summary>
    public CodeBuilder AppendLine(string? value = null)
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

    /// <summary>
    /// Добавляет строку без отступа
    /// </summary>
    public CodeBuilder AppendLineRaw(string value)
    {
        _sb.AppendLine(value);
        return this;
    }

    /// <summary>
    /// Добавляет несколько строк
    /// </summary>
    public CodeBuilder AppendLines(IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            AppendLine(line);
        }

        return this;
    }

    /// <summary>
    /// Увеличивает уровень отступа
    /// </summary>
    public CodeBuilder Indent()
    {
        _indentLevel++;
        return this;
    }

    /// <summary>
    /// Уменьшает уровень отступа
    /// </summary>
    public CodeBuilder Outdent()
    {
        if (_indentLevel > 0)
        {
            _indentLevel--;
        }

        return this;
    }

    /// <summary>
    /// Добавляет XML комментарий с summary
    /// </summary>
    public CodeBuilder AppendXmlSummary(string summary)
    {
        if (string.IsNullOrWhiteSpace(summary))
        {
            return this;
        }

        AppendLine("/// <summary>");
        
        // Разбиваем длинные строки
        var lines = WrapText(summary, 100);
        foreach (var line in lines)
        {
            AppendLine($"/// {line}");
        }

        AppendLine("/// </summary>");

        return this;
    }

    /// <summary>
    /// Добавляет XML комментарий с remarks
    /// </summary>
    public CodeBuilder AppendXmlRemarks(string remarks)
    {
        if (string.IsNullOrWhiteSpace(remarks))
        {
            return this;
        }

        AppendLine("/// <remarks>");
        
        var lines = WrapText(remarks, 100);
        foreach (var line in lines)
        {
            AppendLine($"/// {line}");
        }

        AppendLine("/// </remarks>");

        return this;
    }

    /// <summary>
    /// Добавляет блок using директив
    /// </summary>
    public CodeBuilder AppendUsings(IEnumerable<string> usings)
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

    /// <summary>
    /// Добавляет начало namespace
    /// </summary>
    public CodeBuilder AppendNamespaceStart(string namespaceName)
    {
        AppendLineRaw($"namespace {namespaceName};");
        AppendLineRaw("");
        return this;
    }

    /// <summary>
    /// Добавляет начало класса или record
    /// </summary>
    public CodeBuilder AppendTypeStart(string typeName, bool isRecord, bool isSealed = true, bool isPartial = false)
    {
        var keywords = new List<string>();
        
        if (isSealed)
        {
            keywords.Add("sealed");
        }

        if (isPartial)
        {
            keywords.Add("partial");
        }

        keywords.Add(isRecord ? "record" : "class");

        AppendLine($"public {string.Join(" ", keywords)} {typeName}");
        AppendLine("{");
        Indent();

        return this;
    }

    /// <summary>
    /// Добавляет конец типа
    /// </summary>
    public CodeBuilder AppendTypeEnd()
    {
        Outdent();
        AppendLine("}");
        return this;
    }

    /// <summary>
    /// Добавляет начало enum
    /// </summary>
    public CodeBuilder AppendEnumStart(string enumName)
    {
        AppendLine($"public enum {enumName}");
        AppendLine("{");
        Indent();
        return this;
    }

    /// <summary>
    /// Добавляет значение enum
    /// </summary>
    public CodeBuilder AppendEnumValue(string name, string? comment = null, bool isLast = false)
    {
        if (!string.IsNullOrWhiteSpace(comment))
        {
            AppendXmlSummary(comment);
        }

        AppendLine(isLast ? name : $"{name},");

        if (!isLast)
        {
            AppendLine();
        }

        return this;
    }

    /// <summary>
    /// Добавляет свойство
    /// </summary>
    public CodeBuilder AppendProperty(
        string type,
        string name,
        bool isRequired = false,
        bool hasInit = true,
        string? defaultValue = null,
        string? comment = null)
    {
        if (!string.IsNullOrWhiteSpace(comment))
        {
            AppendXmlSummary(comment);
        }

        var modifiers = new List<string> { "public" };
        if (isRequired)
        {
            modifiers.Add("required");
        }

        var accessor = hasInit ? "{ get; init; }" : "{ get; set; }";
        var propertyDeclaration = $"{string.Join(" ", modifiers)} {type} {name} {accessor}";

        if (!string.IsNullOrEmpty(defaultValue))
        {
            propertyDeclaration += $" = {defaultValue};";
        }

        AppendLine(propertyDeclaration);
        AppendLine();

        return this;
    }

    /// <summary>
    /// Возвращает сгенерированный код
    /// </summary>
    public override string ToString() => _sb.ToString();

    /// <summary>
    /// Получает строку отступа
    /// </summary>
    private string GetIndent() => string.Concat(Enumerable.Repeat(_indentString, _indentLevel));

    /// <summary>
    /// Разбивает текст на строки заданной длины
    /// </summary>
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
