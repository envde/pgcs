namespace PgCs.Core.Extraction.Block;

/// <summary>
/// Состояние парсера SQL блоков.
/// Отслеживает, находится ли парсер внутри блока команды и другие флаги.
/// </summary>
public sealed class BlockExtractorState
{
    /// <summary>
    /// Номер строки начала текущего блока.
    /// </summary>
    public int CurrentBlockStartLine { get; private set; } = -1;

    /// <summary>
    /// Флаг: парсер находится внутри SQL блока.
    /// </summary>
    public bool IsInsideBlock { get; private set; }

    /// <summary>
    /// Флаг: после последнего комментария была пустая строка.
    /// Используется для определения, относится ли комментарий к следующему блоку.
    /// </summary>
    public bool HasEmptyLineAfterComment { get; private set; }

    /// <summary>
    /// Переводит парсер в состояние "внутри блока".
    /// </summary>
    public void EnterBlock(int startLine)
    {
        IsInsideBlock = true;
        CurrentBlockStartLine = startLine;
        HasEmptyLineAfterComment = false;
    }

    /// <summary>
    /// Переводит парсер в состояние "вне блока".
    /// </summary>
    public void ExitBlock()
    {
        IsInsideBlock = false;
        CurrentBlockStartLine = -1;
        HasEmptyLineAfterComment = false;
    }

    /// <summary>
    /// Отмечает, что после комментария была пустая строка.
    /// </summary>
    public void MarkEmptyLineAfterComment()
    {
        HasEmptyLineAfterComment = true;
    }

    /// <summary>
    /// Сбрасывает флаг пустой строки после комментария.
    /// </summary>
    public void ResetEmptyLineFlag()
    {
        HasEmptyLineAfterComment = false;
    }
}