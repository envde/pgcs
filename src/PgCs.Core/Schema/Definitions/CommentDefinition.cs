using PgCs.Core.Schema.Common;

namespace PgCs.Core.Schema.Definitions;

/// <summary>
/// Универсальное определение комментария в PostgreSQL (COMMENT ON)
/// <para>
/// Поддерживает комментарии для различных объектов базы данных:
/// <list type="bullet">
/// <item><description>TABLE - комментарий к таблице</description></item>
/// <item><description>COLUMN - комментарий к колонке таблицы</description></item>
/// <item><description>FUNCTION - комментарий к функции или процедуре</description></item>
/// <item><description>INDEX - комментарий к индексу</description></item>
/// <item><description>TRIGGER - комментарий к триггеру</description></item>
/// <item><description>CONSTRAINT - комментарий к ограничению</description></item>
/// <item><description>TYPE - комментарий к пользовательскому типу (ENUM, Composite, Domain)</description></item>
/// <item><description>VIEW - комментарий к представлению</description></item>
/// </list>
/// </para>
/// </summary>
public sealed record CommentDefinition : DefinitionBase
{
    /// <summary>
    /// Тип объекта схемы базы данных, к которому относится комментарий
    /// </summary>
    public required SchemaObjectType ObjectType { get; init; }
    
    /// <summary>
    /// Имя объекта, к которому относится комментарий
    /// Для COLUMN это имя колонки, для TABLE - имя таблицы, для FUNCTION - имя функции и т.д.
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Имя таблицы (для комментариев к колонкам, триггерам и ограничениям)
    /// Для COLUMN: имя таблицы, которой принадлежит колонка
    /// Для TRIGGER и CONSTRAINT: имя таблицы, на которую установлен триггер/ограничение
    /// </summary>
    public string? TableName { get; init; }
    
    /// <summary>
    /// Сигнатура функции с параметрами (только для FUNCTION)
    /// Например: "calculate_total(integer, text)"
    /// </summary>
    public string? FunctionSignature { get; init; }
    
    /// <summary>
    /// Текст комментария PostgreSQL
    /// </summary>
    public required string Comment { get; init; }
}