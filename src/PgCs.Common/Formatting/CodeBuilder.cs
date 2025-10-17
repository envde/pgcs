using System.Text;
using PgCs.Common.Generation.Models;

namespace PgCs.Common.Formatting;

/// <summary>
/// Универсальный построитель C# кода с поддержкой отступов и форматирования
/// </summary>
public sealed class CodeBuilder
{
    private readonly StringBuilder _sb = new();
    private readonly string _indentString;
    private int _indentLevel;

    public CodeBuilder(IndentationStyle indentationStyle = IndentationStyle.Spaces, int indentationSize = 4)
    {
        _indentString = indentationStyle == IndentationStyle.Tabs
            ? "\t"
            : new string(' ', indentationSize);
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
    /// Добавляет сырой код без форматирования
    /// </summary>
    public CodeBuilder AppendRaw(string code)
    {
        _sb.Append(code);
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
        foreach (var line in WrapText(summary, 100))
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

        foreach (var line in WrapText(remarks, 100))
        {
            AppendLine($"/// {line}");
        }

        AppendLine("/// </remarks>");

        return this;
    }

    /// <summary>
    /// Добавляет XML комментарий для параметра
    /// </summary>
    public CodeBuilder AppendXmlParam(string name, string description)
    {
        AppendLine($"/// <param name=\"{name}\">{description}</param>");
        return this;
    }

    /// <summary>
    /// Добавляет XML комментарий для возвращаемого значения
    /// </summary>
    public CodeBuilder AppendXmlReturns(string description)
    {
        AppendLine($"/// <returns>{description}</returns>");
        return this;
    }

    /// <summary>
    /// Добавляет блок using директив
    /// </summary>
    public CodeBuilder AppendUsings(params string[] usings) =>
        AppendUsings((IEnumerable<string>)usings);

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
    /// Добавляет file-scoped namespace
    /// </summary>
    public CodeBuilder AppendNamespaceStart(string namespaceName)
    {
        _sb.AppendLine($"namespace {namespaceName};");
        _sb.AppendLine();
        return this;
    }

    /// <summary>
    /// Заглушка для совместимости (file-scoped namespace не требует закрытия)
    /// </summary>
    public CodeBuilder AppendNamespaceEnd() => this;

    /// <summary>
    /// Добавляет начало класса или record
    /// </summary>
    public CodeBuilder AppendTypeStart(
        string typeName,
        bool isRecord = false,
        bool isSealed = true,
        bool isPartial = false,
        string? baseType = null,
        IEnumerable<string>? interfaces = null)
    {
        var keywords = new List<string> { "public" };

        if (isSealed && !isRecord)
        {
            keywords.Add("sealed");
        }

        if (isPartial)
        {
            keywords.Add("partial");
        }

        keywords.Add(isRecord ? "record" : "class");
        keywords.Add(typeName);

        var declaration = string.Join(" ", keywords);

        // Добавляем наследование и интерфейсы
        var inherits = new List<string>();
        if (!string.IsNullOrWhiteSpace(baseType))
        {
            inherits.Add(baseType);
        }

        if (interfaces is not null)
        {
            inherits.AddRange(interfaces.Where(i => !string.IsNullOrWhiteSpace(i)));
        }

        if (inherits.Count > 0)
        {
            declaration += $" : {string.Join(", ", inherits)}";
        }

        AppendLine(declaration);
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
    public CodeBuilder AppendEnumStart(string enumName, string? baseType = null)
    {
        var declaration = $"public enum {enumName}";
        if (!string.IsNullOrWhiteSpace(baseType))
        {
            declaration += $" : {baseType}";
        }

        AppendLine(declaration);
        AppendLine("{");
        Indent();
        return this;
    }

    /// <summary>
    /// Добавляет значение enum
    /// </summary>
    public CodeBuilder AppendEnumValue(string name, int? value = null, string? comment = null, bool isLast = false)
    {
        if (!string.IsNullOrWhiteSpace(comment))
        {
            AppendXmlSummary(comment);
        }

        var enumValue = value.HasValue ? $"{name} = {value}" : name;
        AppendLine(isLast ? enumValue : $"{enumValue},");

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
        string? comment = null,
        IEnumerable<string>? attributes = null)
    {
        if (attributes is not null)
        {
            foreach (var attribute in attributes.Where(a => !string.IsNullOrWhiteSpace(a)))
            {
                AppendLine($"[{attribute}]");
            }
        }

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
    /// Добавляет атрибут
    /// </summary>
    public CodeBuilder AppendAttribute(string attribute)
    {
        AppendLine($"[{attribute}]");
        return this;
    }

    /// <summary>
    /// Очищает builder
    /// </summary>
    public void Clear()
    {
        _sb.Clear();
        _indentLevel = 0;
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
