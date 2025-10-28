namespace PgCs.Core.Validation;

/// <summary>
/// Проблема валидации схемы или запроса
/// </summary>
public sealed record ValidationIssue
{
    /// <summary>
    /// Уровень серьезности проблемы
    /// </summary>
    public required ValidationSeverity Severity { get; init; }
    
    /// <summary>
    /// Тип объекта в котором произошла ошибка
    /// </summary>
    public required ValidationDefinitionType DefinitionType { get; init; }

    /// <summary>
    /// Сообщение о проблеме
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Код проблемы для программной обработки
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Местоположение проблемы
    /// </summary>
    public required ValidationLocation Location { get; init; }

    /// <summary>
    /// Дополнительные детали проблемы
    /// </summary>
    public IReadOnlyDictionary<string, string>? Details { get; init; }
    
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

    /// <summary>
    /// Тип объекта, в котором произошла ошибка
    /// </summary>
    public enum ValidationDefinitionType
    {
        /// <summary>
        /// Таблица
        /// </summary>
        Table,
        /// <summary>
        /// Представление
        /// </summary>
        View,
        /// <summary>
        /// Функция
        /// </summary>
        Function,
        /// <summary>
        /// Триггер
        /// </summary>
        Trigger,
        /// <summary>
        /// Ограничение
        /// </summary>
        Constraint,
        /// <summary>
        /// Индекс
        /// </summary>
        Index,
        /// <summary>
        /// Схема
        /// </summary>
        Schema,
        /// <summary>
        /// ENUM тип
        /// </summary>
        Enum,
        /// <summary>
        /// Composite тип
        /// </summary>
        Composite,
        /// <summary>
        /// Domain тип
        /// </summary>
        Domain,
        /// <summary>
        /// Запрос
        /// </summary>
        Query,
    }

    /// <summary>
    /// Локация проблемы
    /// </summary>
    public record ValidationLocation
    {
        /// <summary>
        /// Сегмент кода в котором произошла ошибка
        /// </summary>
        public required string Segment { get; init; }
        /// <summary>
        /// Строка
        /// </summary>
        public required int Line { get; init; }
        /// <summary>
        /// Столбец
        /// </summary>
        public required int Column { get; init; }
    }


    /// <summary>
    /// Создает ошибку валидации (Error)
    /// </summary>
    /// <param name="definitionType">Тип объекта, в котором произошла ошибка</param>
    /// <param name="code">Код ошибка</param>
    /// <param name="message">Сообщение</param>
    /// <param name="location">Место положение ошибки</param>
    /// <param name="details">Детали</param>
    /// <returns></returns>
    public static ValidationIssue Error(ValidationDefinitionType definitionType, string code, string message, ValidationLocation location, Dictionary<string, string>? details = null)
    {
        return new ValidationIssue
        {
            Severity = ValidationSeverity.Error,
            DefinitionType = definitionType,
            Code = code,
            Message = message,
            Location = location,
            Details = details
        };
    }

    /// <summary>
    /// Создает предупреждение валидации (Warning)
    /// </summary>
    /// <param name="definitionType">Тип объекта, в котором произошла ошибка</param>
    /// <param name="code">Код ошибка</param>
    /// <param name="message">Сообщение</param>
    /// <param name="location">Место положение ошибки</param>
    /// <param name="details">Детали</param>
    /// <returns></returns>
    public static ValidationIssue Warning(ValidationDefinitionType definitionType, string code, string message, ValidationLocation location, Dictionary<string, string>? details = null)
    {
        return new ValidationIssue
        {
            Severity = ValidationSeverity.Warning,
            DefinitionType = definitionType,
            Code = code,
            Message = message,
            Location = location,
            Details = details
        };
    }

    /// <summary>
    /// Создает информационное сообщение валидации (Info)
    /// </summary>
    /// <param name="definitionType">Тип объекта, в котором произошла ошибка</param>
    /// <param name="code">Код ошибка</param>
    /// <param name="message">Сообщение</param>
    /// <param name="location">Место положение ошибки</param>
    /// <param name="details">Детали</param>
    /// <returns></returns>
    public static ValidationIssue Info(ValidationDefinitionType definitionType,string code, string message, ValidationLocation location, Dictionary<string, string>? details = null)
    {
        return new ValidationIssue
        {
            Severity = ValidationSeverity.Info,
            DefinitionType = definitionType,
            Code = code,
            Message = message,
            Location = location,
            Details = details
        };
    }
}