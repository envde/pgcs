namespace PgCs.Common.SchemaAnalyzer;

/// <summary>
/// Полная схема базы данных
/// </summary>
public class DatabaseSchema
{
    /// <summary>
    /// Имя схемы
    /// </summary>
    public required string SchemaName { get; init; }

    /// <summary>
    /// Все таблицы в схеме
    /// </summary>
    public required IReadOnlyList<TableInfo> Tables { get; init; }

    /// <summary>
    /// Все связи между таблицами
    /// </summary>
    public required IReadOnlyList<ForeignKeyInfo> ForeignKeys { get; init; }

    /// <summary>
    /// Пользовательские типы
    /// </summary>
    public required IReadOnlyList<CustomTypeInfo> CustomTypes { get; init; }

    /// <summary>
    /// ENUM типы
    /// </summary>
    public required IReadOnlyList<EnumTypeInfo> Enums { get; init; }

    /// <summary>
    /// Представления
    /// </summary>
    public required IReadOnlyList<ViewInfo> Views { get; init; }

    /// <summary>
    /// Функции и процедуры
    /// </summary>
    public required IReadOnlyList<FunctionInfo> Functions { get; init; }
}
