namespace PgCs.Common.QueryGenerator.Models.Options;

/// <summary>
/// Стратегия организации сгенерированных методов
/// </summary>
public enum MethodOrganization
{
    /// <summary>
    /// Все методы в одном классе репозитория
    /// </summary>
    SingleRepository,

    /// <summary>
    /// Группировка методов по типу запроса (Select, Insert, Update, Delete)
    /// </summary>
    ByQueryType,

    /// <summary>
    /// Группировка методов по таблицам/сущностям
    /// </summary>
    ByEntity,

    /// <summary>
    /// Отдельный файл для каждого метода
    /// </summary>
    PerMethod
}
