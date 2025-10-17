namespace PgCs.Common.CodeGeneration;

/// <summary>
/// Уровень серьезности проблемы валидации
/// </summary>
public enum ValidationSeverity
{
    /// <summary>
    /// Информационное сообщение
    /// </summary>
    Info,

    /// <summary>
    /// Предупреждение (генерация может продолжиться)
    /// </summary>
    Warning,

    /// <summary>
    /// Ошибка (генерация должна быть прервана)
    /// </summary>
    Error
}
