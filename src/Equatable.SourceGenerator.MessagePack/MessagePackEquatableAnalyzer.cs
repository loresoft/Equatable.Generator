using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Equatable.SourceGenerator.MessagePack;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MessagePackEquatableAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor MissingMessagePackObjectAttribute = new(
        id: "EQ0021",
        title: "Missing MessagePackObject Attribute",
        messageFormat: "'{0}' is marked with [MessagePackEquatable] but is missing [MessagePackObject]. Key attributes are ignored by MessagePack serializer without it.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor UnannotatedPropertyOnMessagePackEquatable = new(
        id: "EQ0023",
        title: "Unannotated Property on MessagePackEquatable Type",
        messageFormat: "Property '{0}' on [MessagePackEquatable] type '{1}' has no [Key] or [IgnoreMember] attribute. It will be silently excluded from equality. Add [Key(n)] to include it or [IgnoreMember] to suppress this warning.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(MissingMessagePackObjectAttribute, UnannotatedPropertyOnMessagePackEquatable);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;

        if (!HasMessagePackEquatableAttribute(typeSymbol))
            return;

        if (!HasMessagePackObjectAttribute(typeSymbol))
        {
            var location = typeSymbol.Locations.FirstOrDefault();
            context.ReportDiagnostic(Diagnostic.Create(MissingMessagePackObjectAttribute, location, typeSymbol.Name));
        }

        foreach (var property in typeSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            if (!EquatableGenerator.IsPublicInstanceProperty(property))
                continue;

            var attributes = property.GetAttributes();

            if (HasKeyAttribute(attributes) || HasIgnoreMemberAttribute(attributes) || HasIgnoreEqualityAttribute(attributes))
                continue;

            var propertyLocation = property.Locations.FirstOrDefault();
            context.ReportDiagnostic(Diagnostic.Create(
                UnannotatedPropertyOnMessagePackEquatable,
                propertyLocation,
                property.Name,
                typeSymbol.Name));
        }
    }

    private static bool HasMessagePackEquatableAttribute(INamedTypeSymbol typeSymbol) =>
        typeSymbol.GetAttributes().Any(a => a.AttributeClass is
        {
            Name: "MessagePackEquatableAttribute",
            ContainingNamespace: { Name: "MessagePack", ContainingNamespace: { Name: "Attributes", ContainingNamespace.Name: "Equatable" } }
        });

    private static bool HasMessagePackObjectAttribute(INamedTypeSymbol typeSymbol) =>
        typeSymbol.GetAttributes().Any(a => a.AttributeClass is
        {
            Name: "MessagePackObjectAttribute",
            ContainingNamespace.Name: "MessagePack"
        });

    private static bool HasKeyAttribute(ImmutableArray<AttributeData> attributes) =>
        attributes.Any(a => a.AttributeClass is { Name: "KeyAttribute", ContainingNamespace.Name: "MessagePack" });

    private static bool HasIgnoreMemberAttribute(ImmutableArray<AttributeData> attributes) =>
        attributes.Any(a => a.AttributeClass is { Name: "IgnoreMemberAttribute", ContainingNamespace.Name: "MessagePack" });

    private static bool HasIgnoreEqualityAttribute(ImmutableArray<AttributeData> attributes) =>
        attributes.Any(a => a.AttributeClass is
        {
            Name: "IgnoreEqualityAttribute",
            ContainingNamespace: { Name: "Attributes", ContainingNamespace.Name: "Equatable" }
        });
}
