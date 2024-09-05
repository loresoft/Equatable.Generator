namespace Equatable.SourceGenerator.Models;

public record ContainingClass(
    string EntityName,
    bool IsRecord,
    bool IsValueType
);
