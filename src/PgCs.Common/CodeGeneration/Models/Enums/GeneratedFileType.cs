namespace PgCs.Common.CodeGeneration.Models.Enums;

/// <summary>
/// Тип сгенерированного файла
/// </summary>
public enum GeneratedFileType
{
    /// <summary>
    /// Модель таблицы
    /// </summary>
    Model,

    /// <summary>
    /// Перечисление
    /// </summary>
    Enum,

    /// <summary>
    /// Интерфейс
    /// </summary>
    Interface,

    /// <summary>
    /// Репозиторий с методами запросов
    /// </summary>
    Repository,

    /// <summary>
    /// Контекст базы данных
    /// </summary>
    DbContext,

    /// <summary>
    /// Класс расширений
    /// </summary>
    Extension,

    /// <summary>
    /// Конфигурация/настройки
    /// </summary>
    Configuration,

    /// <summary>
    /// DTO (Data Transfer Object)
    /// </summary>
    Dto,

    /// <summary>
    /// Модель представления (View)
    /// </summary>
    ViewModel,

    /// <summary>
    /// Композитный тип PostgreSQL
    /// </summary>
    CompositeType
}