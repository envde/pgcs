namespace PgCs.Core.Definitions.Schema.Base;

/// <summary>
/// Тип ограничения целостности таблицы
/// </summary>
public enum ConstraintType
{
    /// <summary>
    /// PRIMARY KEY - уникальный идентификатор строки, не может быть NULL
    /// </summary>
    PrimaryKey,

    /// <summary>
    /// FOREIGN KEY - ссылка на строку в другой таблице для обеспечения ссылочной целостности
    /// </summary>
    ForeignKey,

    /// <summary>
    /// UNIQUE - уникальность значений в столбце или группе столбцов
    /// </summary>
    Unique,

    /// <summary>
    /// CHECK - пользовательское условие, которое должно быть истинным для каждой строки
    /// </summary>
    Check,

    /// <summary>
    /// EXCLUDE - ограничение исключения, использующее индекс для предотвращения перекрытия значений
    /// </summary>
    Exclude
}