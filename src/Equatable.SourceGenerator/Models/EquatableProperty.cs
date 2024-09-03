namespace Equatable.SourceGenerator.Models;

public record EquatableProperty(
        string PropertyName,
        string PropertyType,
        ComparerTypes ComparerType = ComparerTypes.Default,
        string? ComparerName = null,
        string? ComparerInstance = null);
