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

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(MissingDataContractAttribute);

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

        if (HasDataContractAttribute(typeSymbol))
            return;

        var location = typeSymbol.Locations.FirstOrDefault();
        context.ReportDiagnostic(Diagnostic.Create(MissingDataContractAttribute, location, typeSymbol.Name));
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
}
