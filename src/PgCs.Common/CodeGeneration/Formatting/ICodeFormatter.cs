using PgCs.Common.CodeGeneration.Formatting.Models;
using PgCs.Common.CodeGeneration.Models;

namespace PgCs.Common.CodeGeneration.Formatting;

/// <summary>
/// Форматировщик сгенерированного C# кода
/// </summary>
public interface ICodeFormatter
{
    /// <summary>
    /// Форматирует C# код
    /// </summary>
    /// <param name="code">Исходный код</param>
    /// <param name="options">Опции форматирования</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Отформатированный код</returns>
    ValueTask<string> FormatAsync(
        string code,
        FormattingOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Форматирует несколько файлов
    /// </summary>
    /// <param name="files">Файлы для форматирования</param>
    /// <param name="options">Опции форматирования</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Отформатированные файлы</returns>
    ValueTask<IReadOnlyList<GeneratedFile>> FormatFilesAsync(
        IReadOnlyList<GeneratedFile> files,
        FormattingOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверяет код на соответствие стандартам форматирования
    /// </summary>
    /// <param name="code">Код для проверки</param>
    /// <param name="options">Опции форматирования</param>
    /// <returns>True если код отформатирован корректно</returns>
    bool IsFormatted(string code, FormattingOptions? options = null);
}