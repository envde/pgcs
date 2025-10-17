namespace PgCs.Common.CodeGeneration;

/// <summary>
/// Информация о сгенерированном коде
/// </summary>
public sealed record GeneratedCode
{
    /// <summary>
    /// Сгенерированный исходный код
    /// </summary>
    public required string SourceCode { get; init; }

    /// <summary>
    /// Имя основного типа (класс, интерфейс, enum, record)
    /// </summary>
    public required string TypeName { get; init; }

    /// <summary>
    /// Namespace сгенерированного типа
    /// </summary>
    public required string Namespace { get; init; }

    /// <summary>
    /// Тип сгенерированного кода
    /// </summary>
    public GeneratedFileType CodeType { get; init; }

    /// <summary>
    /// Размер кода в байтах
    /// </summary>
    public long SizeInBytes => System.Text.Encoding.UTF8.GetByteCount(SourceCode);

    /// <summary>
    /// Время генерации
    /// </summary>
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Рекомендуемое имя файла (без пути)
    /// </summary>
    public string SuggestedFileName => $"{TypeName}.cs";
}
