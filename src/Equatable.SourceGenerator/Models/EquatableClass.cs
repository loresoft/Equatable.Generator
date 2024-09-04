namespace Equatable.SourceGenerator.Models;

public record EquatableClass(
        string EntityNamespace,
        string EntityName,
        EquatableArray<EquatableProperty> Properties,
        bool IsRecord,
        bool IsValueType,
        bool IsSealed,
        bool IncludeBaseEqualsMethod,
        bool IncludeBaseHashMethod,
        int SeedHash
);
