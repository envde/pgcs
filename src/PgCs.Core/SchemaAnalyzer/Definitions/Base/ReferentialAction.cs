namespace PgCs.Core.SchemaAnalyzer.Definitions.Base;

/// <summary>
/// Действие при нарушении ссылочной целостности
/// </summary>
public enum ReferentialAction
{
    /// <summary>
    /// Не выполнять никаких действий (отложенная проверка)
    /// </summary>
    NoAction,
    
    /// <summary>
    /// Запретить удаление/изменение (немедленная проверка)
    /// </summary>
    Restrict,
    
    /// <summary>
    /// Каскадное удаление/изменение связанных записей
    /// </summary>
    Cascade,
    
    /// <summary>
    /// Установить NULL в связанных записях
    /// </summary>
    SetNull,
    
    /// <summary>
    /// Установить значение по умолчанию в связанных записях
    /// </summary>
    SetDefault
}