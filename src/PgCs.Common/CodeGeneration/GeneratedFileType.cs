namespace PgCs.Common.CodeGeneration;

/// <summary>
/// Тип сгенерированного файла
/// </summary>
public enum GeneratedFileType
{
    /// <summary>
    /// Модель таблицы
    /// </summary>
    TableModel,

    /// <summary>
    /// Модель представления
    /// </summary>
    ViewModel,

    /// <summary>
    /// Enum тип
    /// </summary>
    EnumType,

    /// <summary>
    /// Domain тип
    /// </summary>
    DomainType,

    /// <summary>
    /// Composite тип
    /// </summary>
    CompositeType,

    /// <summary>
    /// Метод функции БД
    /// </summary>
    FunctionMethod,

    /// <summary>
    /// Метод запроса
    /// </summary>
    QueryMethod,

    /// <summary>
    /// Модель результата запроса
    /// </summary>
    ResultModel,

    /// <summary>
    /// Модель параметров запроса
    /// </summary>
    ParameterModel,

    /// <summary>
    /// Интерфейс репозитория
    /// </summary>
    RepositoryInterface,

    /// <summary>
    /// Реализация репозитория
    /// </summary>
    RepositoryImplementation,

    /// <summary>
    /// Класс репозитория (для функций БД и т.д.)
    /// </summary>
    RepositoryClass
}
