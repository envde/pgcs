namespace PgCs.Common.CodeGeneration.Query.Models.Enums;

/// <summary>
/// Режим генерации методов
/// </summary>
public enum MethodGenerationMode
{
    /// <summary>
    /// Только синхронные методы
    /// </summary>
    SyncOnly,

    /// <summary>
    /// Только асинхронные методы
    /// </summary>
    AsyncOnly,

    /// <summary>
    /// И синхронные и асинхронные методы
    /// </summary>
    Both
}