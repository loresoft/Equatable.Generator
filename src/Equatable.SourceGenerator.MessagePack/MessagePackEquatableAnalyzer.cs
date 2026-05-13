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

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(MissingMessagePackObjectAttribute);

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

        if (HasMessagePackObjectAttribute(typeSymbol))
            return;

        var location = typeSymbol.Locations.FirstOrDefault();
        context.ReportDiagnostic(Diagnostic.Create(MissingMessagePackObjectAttribute, location, typeSymbol.Name));
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
}
