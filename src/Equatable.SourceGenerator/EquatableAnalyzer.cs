using System.Collections.Generic;
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
            DiagnosticDescriptors.InvalidSequenceEqualityAttributeUsage,
            DiagnosticDescriptors.InvalidAttributeOnMultiDimensionalArray,
            DiagnosticDescriptors.InvalidEnumerableAttributeOnDictionary
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

        foreach (var property in GetAnalyzableProperties(typeSymbol))
            AnalyzeProperty(context, property);
    }

    private static IEnumerable<IPropertySymbol> GetAnalyzableProperties(INamedTypeSymbol typeSymbol)
    {
        // De-duplicate by name (same as the generator's GetProperties) so that
        // a derived-class property shadows any same-named base-class property.
        var seenPropertyNames = new HashSet<string>(StringComparer.Ordinal);

        for (var currentSymbol = typeSymbol; currentSymbol != null; currentSymbol = currentSymbol.BaseType)
        {
            // Stop at system base types
            if (IsSystemBaseType(currentSymbol))
                break;

            // If a base type (not the target itself) has any generator attribute, stop: it will be analyzed separately
            if (!SymbolEqualityComparer.Default.Equals(currentSymbol, typeSymbol) && HasEquatableAttribute(currentSymbol))
                break;

            foreach (var property in currentSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => !p.IsIndexer
                    && p.DeclaredAccessibility == Accessibility.Public
                    && !IsIgnored(p)))
            {
                if (seenPropertyNames.Add(property.Name))
                    yield return property;
            }
        }
    }

    private static void AnalyzeProperty(SymbolAnalysisContext context, IPropertySymbol property)
    {
        var attributes = property.GetAttributes();
        var hasEqualityAttribute = false;
        var isMultiDimArray = property.Type is IArrayTypeSymbol { Rank: > 1 };

        foreach (var attribute in attributes)
        {
            if (!IsKnownAttribute(attribute))
                continue;

            hasEqualityAttribute = true;

            var className = attribute.AttributeClass?.Name;
            var attributeLocation = attribute.ApplicationSyntaxReference
                ?.GetSyntax(context.CancellationToken).GetLocation()
                ?? property.Locations.FirstOrDefault();

            // Any collection/equality attribute on a multi-dimensional array has no effect:
            // MultiDimensionalArrayEqualityComparer is always used regardless of the annotation.
            if (isMultiDimArray && IsCollectionOrEqualityAttribute(className))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.InvalidAttributeOnMultiDimensionalArray,
                    attributeLocation,
                    property.Name));
            }

            // [SequenceEquality] or [HashSetEquality] on a dictionary type treats the dict as a
            // sequence of KeyValuePair entries, discarding key-lookup semantics entirely.
            if ((className == "SequenceEqualityAttribute" || className == "HashSetEqualityAttribute")
                && ImplementsDictionary(property.Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.InvalidEnumerableAttributeOnDictionary,
                    attributeLocation,
                    property.Name));
            }

            if (className == "StringEqualityAttribute" && !IsString(property.Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.InvalidStringEqualityAttributeUsage,
                    attributeLocation,
                    property.Name));
            }
            else if (className == "DictionaryEqualityAttribute"
                && !ImplementsDictionary(property.Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.InvalidDictionaryEqualityAttributeUsage,
                    attributeLocation,
                    property.Name));
            }
            else if (className == "HashSetEqualityAttribute"
                && !ImplementsEnumerable(property.Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.InvalidHashSetEqualityAttributeUsage,
                    attributeLocation,
                    property.Name));
            }
            else if (className == "SequenceEqualityAttribute"
                && !ImplementsEnumerable(property.Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.InvalidSequenceEqualityAttributeUsage,
                    attributeLocation,
                    property.Name));
            }
        }

        // Warn when a collection/dictionary property has no equality attribute.
        // Multi-dimensional arrays are exempt: they always use MultiDimensionalArrayEqualityComparer
        // by default and do not require an explicit annotation.
        if (!hasEqualityAttribute && !isMultiDimArray)
        {
            var propertyLocation = property.Locations.FirstOrDefault();

            if (ImplementsDictionary(property.Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.MissingDictionaryEqualityAttribute,
                    propertyLocation,
                    property.Name));
            }
            else if (!IsString(property.Type) && ImplementsEnumerable(property.Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.MissingSequenceEqualityAttribute,
                    propertyLocation,
                    property.Name));
            }
        }
    }

    private static bool IsCollectionOrEqualityAttribute(string? className) =>
        className is "SequenceEqualityAttribute"
            or "HashSetEqualityAttribute"
            or "DictionaryEqualityAttribute"
            or "EqualityComparerAttribute"
            or "ReferenceEqualityAttribute"
            or "StringEqualityAttribute";

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

    private static bool IsSystemBaseType(INamedTypeSymbol symbol)
    {
        return symbol is
        {
            Name: "Object" or "ValueType",
            ContainingNamespace.Name: "System"
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

    private static bool ImplementsDictionary(ITypeSymbol type)
    {
        return (type is INamedTypeSymbol named && IsDictionary(named))
            || type.AllInterfaces.Any(IsDictionary);
    }

    private static bool ImplementsEnumerable(ITypeSymbol type)
    {
        // Arrays (including multi-dimensional) are always valid for [SequenceEquality]:
        // single-dim arrays implement IEnumerable<T>; multi-dim use MultiDimensionalArrayEqualityComparer.
        if (type is IArrayTypeSymbol)
            return true;

        return (type is INamedTypeSymbol named && IsEnumerable(named))
            || type.AllInterfaces.Any(IsEnumerable);
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
                    ContainingNamespace.Name: "System"
                }
            }
        };
    }

    private static bool IsDictionary(INamedTypeSymbol targetSymbol)
    {
        return targetSymbol is
        {
            Name: "IDictionary" or "IReadOnlyDictionary",
            IsGenericType: true,
            TypeArguments.Length: 2,
            TypeParameters.Length: 2,
            ContainingNamespace:
            {
                Name: "Generic",
                ContainingNamespace:
                {
                    Name: "Collections",
                    ContainingNamespace.Name: "System"
                }
            }
        };
    }
}
