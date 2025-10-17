using PgCs.Common.QueryGenerator.Models;

namespace PgCs.QueryGenerator.Core;

/// <summary>
/// Интерфейс валидатора сгенерированного кода
/// </summary>
internal interface ICodeValidator
{
    /// <summary>
    /// Валидирует сгенерированный C# код
    /// </summary>
    ValidationResult Validate(string code);
}
