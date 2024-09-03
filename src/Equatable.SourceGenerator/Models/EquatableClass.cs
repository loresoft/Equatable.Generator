namespace Equatable.SourceGenerator.Models;

public record EquatableClass(
        string EntityNamespace,
        string EntityName,
        EquatableArray<EquatableProperty> Properties);
