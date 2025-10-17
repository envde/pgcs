namespace PgCs.Common.Writer.Models;

/// <summary>
/// Тип ошибки записи
/// </summary>
public enum WriteErrorType
{
    /// <summary>
    /// Нет доступа к директории или файлу
    /// </summary>
    AccessDenied,

    /// <summary>
    /// Директория не существует и не удалось создать
    /// </summary>
    DirectoryNotFound,

    /// <summary>
    /// Файл уже существует и перезапись запрещена
    /// </summary>
    FileExists,

    /// <summary>
    /// Недостаточно места на диске
    /// </summary>
    DiskFull,

    /// <summary>
    /// Ошибка кодировки при записи
    /// </summary>
    EncodingError,

    /// <summary>
    /// Неизвестная ошибка ввода-вывода
    /// </summary>
    IOError,

    /// <summary>
    /// Некорректный путь
    /// </summary>
    InvalidPath
}
