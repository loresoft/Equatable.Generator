namespace Equatable.SourceGenerator.Models;

public record EquatableClass(
    string FullyQualified,
    string EntityNamespace,
    string EntityName,
    EquatableArray<ContainingClass> ContainingTypes,
    EquatableArray<EquatableProperty> Properties,
    bool IsRecord,
    bool IsValueType,
    bool IsSealed,
    bool IncludeBaseEqualsMethod,
    bool IncludeBaseHashMethod,
    int SeedHash
);
