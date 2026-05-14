using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Equatable.SourceGenerator.DataContract;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DataContractEquatableAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor MissingDataContractAttribute = new(
        id: "EQ0020",
        title: "Missing DataContract Attribute",
        messageFormat: "'{0}' is marked with [DataContractEquatable] but is missing [DataContract]. DataMember attributes are ignored by DataContractSerializer without it.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor UnannotatedPropertyOnDataContractEquatable = new(
        id: "EQ0022",
        title: "Unannotated Property on DataContractEquatable Type",
        messageFormat: "Property '{0}' on [DataContractEquatable] type '{1}' has no [DataMember] or [IgnoreDataMember] attribute. It will be silently excluded from equality. Add [DataMember] to include it or [IgnoreDataMember] to suppress this warning.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(MissingDataContractAttribute, UnannotatedPropertyOnDataContractEquatable);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;

        if (!HasDataContractEquatableAttribute(typeSymbol))
            return;

        if (!HasDataContractAttribute(typeSymbol))
        {
            var location = typeSymbol.Locations.FirstOrDefault();
            context.ReportDiagnostic(Diagnostic.Create(MissingDataContractAttribute, location, typeSymbol.Name));
        }

        foreach (var property in typeSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            if (!EquatableGenerator.IsPublicInstanceProperty(property))
                continue;

            var attributes = property.GetAttributes();

            if (HasDataMemberAttribute(attributes) || HasIgnoreDataMemberAttribute(attributes) || HasIgnoreEqualityAttribute(attributes))
                continue;

            var propertyLocation = property.Locations.FirstOrDefault();
            context.ReportDiagnostic(Diagnostic.Create(
                UnannotatedPropertyOnDataContractEquatable,
                propertyLocation,
                property.Name,
                typeSymbol.Name));
        }
    }

    private static bool HasDataContractEquatableAttribute(INamedTypeSymbol typeSymbol) =>
        typeSymbol.GetAttributes().Any(a => a.AttributeClass is
        {
            Name: "DataContractEquatableAttribute",
            ContainingNamespace: { Name: "DataContract", ContainingNamespace: { Name: "Attributes", ContainingNamespace.Name: "Equatable" } }
        });

    private static bool HasDataContractAttribute(INamedTypeSymbol typeSymbol) =>
        typeSymbol.GetAttributes().Any(a => a.AttributeClass is
        {
            Name: "DataContractAttribute",
            ContainingNamespace: { Name: "Serialization", ContainingNamespace: { Name: "Runtime", ContainingNamespace.Name: "System" } }
        });

    private static bool HasDataMemberAttribute(ImmutableArray<AttributeData> attributes) =>
        attributes.Any(a => a.AttributeClass is
        {
            Name: "DataMemberAttribute",
            ContainingNamespace: { Name: "Serialization", ContainingNamespace: { Name: "Runtime", ContainingNamespace.Name: "System" } }
        });

    private static bool HasIgnoreDataMemberAttribute(ImmutableArray<AttributeData> attributes) =>
        attributes.Any(a => a.AttributeClass is
        {
            Name: "IgnoreDataMemberAttribute",
            ContainingNamespace: { Name: "Serialization", ContainingNamespace: { Name: "Runtime", ContainingNamespace.Name: "System" } }
        });

    private static bool HasIgnoreEqualityAttribute(ImmutableArray<AttributeData> attributes) =>
        attributes.Any(a => a.AttributeClass is
        {
            Name: "IgnoreEqualityAttribute",
            ContainingNamespace: { Name: "Attributes", ContainingNamespace.Name: "Equatable" }
        });
}
