using Microsoft.CodeAnalysis;

namespace Equatable.SourceGenerator;

internal static class DiagnosticDescriptors
{
    public static DiagnosticDescriptor InvalidStringEqualityAttributeUsage => new(
        id: "EQ0010",
        title: "Invalid String Equality Attribute Usage",
        messageFormat: "Invalid String equality attribute usage for property {0}.  Property return type is not a string",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor InvalidDictionaryEqualityAttributeUsage => new(
        id: "EQ0011",
        title: "Invalid Dictionary Equality Attribute Usage",
        messageFormat: "Invalid Dictionary equality attribute usage for property {0}.  Property return type does not implement IDictionary<TKey, TValue>",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor InvalidHashSetEqualityAttributeUsage => new(
        id: "EQ0012",
        title: "Invalid HashSet Equality Attribute Usage",
        messageFormat: "Invalid HashSet equality attribute usage for property {0}.  Property return type does not implement IEnumerable<T>",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor InvalidSequenceEqualityAttributeUsage => new(
        id: "EQ0013",
        title: "Invalid Sequence Equality Attribute Usage",
        messageFormat: "Invalid Sequence equality attribute usage for property {0}.  Property return type does not implement IEnumerable<T>",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

}
