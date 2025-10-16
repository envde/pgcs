namespace PgCs.Common.SchemaAnalyzer.Models.Types;

/// <summary>
/// Тип пользовательского типа данных
/// </summary>
public enum TypeKind
{
    Enum,
    Composite,
    Domain,
    Range
}