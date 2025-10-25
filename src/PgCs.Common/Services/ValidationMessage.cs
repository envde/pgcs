using PgCs.Common.CodeGeneration;

namespace PgCs.Common.Services;

/// <summary>
/// Validation message используемое в Services для передачи в Commands
/// </summary>
public record ValidationMessage(
    ValidationSeverity Severity,
    string Code,
    string Message,
    string? Location
);
