using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Equatable.SourceGenerator;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EquatableAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticDescriptors.MissingDictionaryEqualityAttribute,
            DiagnosticDescriptors.MissingSequenceEqualityAttribute,
            DiagnosticDescriptors.InvalidStringEqualityAttributeUsage,
            DiagnosticDescriptors.InvalidDictionaryEqualityAttributeUsage,
            DiagnosticDescriptors.InvalidHashSetEqualityAttributeUsage,
            DiagnosticDescriptors.InvalidSequenceEqualityAttributeUsage
        );

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;

        // Only analyze types with [Equatable] attribute
        if (!HasEquatableAttribute(typeSymbol))
            return;

        var properties = typeSymbol
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => !p.IsIndexer
                && p.DeclaredAccessibility == Accessibility.Public
                && !IsIgnored(p));

        foreach (var property in properties)
            AnalyzeProperty(context, property);
    }

    private static void AnalyzeProperty(SymbolAnalysisContext context, IPropertySymbol property)
    {
        var attributes = property.GetAttributes();
        var hasEqualityAttribute = false;

        foreach (var attribute in attributes)
        {
            if (!IsKnownAttribute(attribute))
                continue;

            hasEqualityAttribute = true;

            var className = attribute.AttributeClass?.Name;
            var attributeLocation = attribute.ApplicationSyntaxReference
                ?.GetSyntax(context.CancellationToken).GetLocation()
                ?? property.Locations.FirstOrDefault();

            if (className == "StringEqualityAttribute" && !IsString(property.Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.InvalidStringEqualityAttributeUsage,
                    attributeLocation,
                    property.Name));
            }
            else if (className == "DictionaryEqualityAttribute"
                && !property.Type.AllInterfaces.Any(IsDictionary))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.InvalidDictionaryEqualityAttributeUsage,
                    attributeLocation,
                    property.Name));
            }
            else if (className == "HashSetEqualityAttribute"
                && !property.Type.AllInterfaces.Any(IsEnumerable))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.InvalidHashSetEqualityAttributeUsage,
                    attributeLocation,
                    property.Name));
            }
            else if (className == "SequenceEqualityAttribute"
                && !property.Type.AllInterfaces.Any(IsEnumerable))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.InvalidSequenceEqualityAttributeUsage,
                    attributeLocation,
                    property.Name));
            }
        }

        // Warn when a collection/dictionary property has no equality attribute
        if (!hasEqualityAttribute)
        {
            var propertyLocation = property.Locations.FirstOrDefault();

            if (property.Type.AllInterfaces.Any(IsDictionary))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.MissingDictionaryEqualityAttribute,
                    propertyLocation,
                    property.Name));
            }
            else if (!IsString(property.Type) && property.Type.AllInterfaces.Any(IsEnumerable))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.MissingSequenceEqualityAttribute,
                    propertyLocation,
                    property.Name));
            }
        }
    }

    private static bool HasEquatableAttribute(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.GetAttributes().Any(
            a => IsKnownAttribute(a) && a.AttributeClass?.Name == "EquatableAttribute");
    }

    private static bool IsIgnored(IPropertySymbol propertySymbol)
    {
        return propertySymbol.GetAttributes().Any(
            a => IsKnownAttribute(a) && a.AttributeClass?.Name == "IgnoreEqualityAttribute");
    }

    private static bool IsKnownAttribute(AttributeData? attribute)
    {
        if (attribute == null)
            return false;

        return attribute.AttributeClass is
        {
            ContainingNamespace:
            {
                Name: "Attributes",
                ContainingNamespace.Name: "Equatable"
            }
        };
    }

    private static bool IsString(ITypeSymbol targetSymbol)
    {
        return targetSymbol is
        {
            Name: "String",
            ContainingNamespace.Name: "System"
        };
    }

    private static bool IsEnumerable(INamedTypeSymbol targetSymbol)
    {
        return targetSymbol is
        {
            Name: "IEnumerable",
            IsGenericType: true,
            TypeArguments.Length: 1,
            TypeParameters.Length: 1,
            ContainingNamespace:
            {
                Name: "Generic",
                ContainingNamespace:
                {
                    Name: "Collections",
                    ContainingNamespace:
                    {
                        Name: "System"
                    }
                }
            }
        };
    }

    private static bool IsDictionary(INamedTypeSymbol targetSymbol)
    {
        return targetSymbol is
        {
            Name: "IDictionary",
            IsGenericType: true,
            TypeArguments.Length: 2,
            TypeParameters.Length: 2,
            ContainingNamespace:
            {
                Name: "Generic",
                ContainingNamespace:
                {
                    Name: "Collections",
                    ContainingNamespace:
                    {
                        Name: "System"
                    }
                }
            }
        };
    }
}
