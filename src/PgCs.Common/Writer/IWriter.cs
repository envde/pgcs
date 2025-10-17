using PgCs.Common.CodeGeneration;
using PgCs.Common.Writer.Models;

namespace PgCs.Common.Writer;

/// <summary>
/// Интерфейс для записи сгенерированного кода в различные хранилища
/// </summary>
public interface IWriter
{
    /// <summary>
    /// Записывает сгенерированный код в хранилище
    /// </summary>
    /// <param name="code">Список сгенерированного кода</param>
    /// <param name="options">Опции записи</param>
    /// <returns>Результат записи файлов</returns>
    ValueTask<WriteResult> WriteManyAsync(
        IReadOnlyList<GeneratedCode> code,
        WriteOptions options);

    /// <summary>
    /// Записывает один элемент сгенерированного кода в хранилище
    /// </summary>
    /// <param name="code">Сгенерированный код</param>
    /// <param name="options">Опции записи</param>
    /// <returns>Результат записи файла</returns>
    ValueTask<WriteResult> WriteOneAsync(
        GeneratedCode code,
        WriteOptions options);

    /// <summary>
    /// Проверяет возможность записи в указанное место назначения
    /// </summary>
    /// <param name="options">Опции записи</param>
    /// <returns>True если запись возможна, иначе false</returns>
    ValueTask<bool> CanWriteAsync(WriteOptions options);

    /// <summary>
    /// Удаляет ранее созданные файлы
    /// </summary>
    /// <param name="filePaths">Пути к файлам для удаления</param>
    /// <returns>Результат удаления</returns>
    ValueTask<WriteResult> DeleteFilesAsync(IReadOnlyList<string> filePaths);
}
