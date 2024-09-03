using Equatable.SourceGenerator.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Equatable.SourceGenerator;

[Generator]
public class EquatableGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "Equatable.Attributes.EquatableAttribute",
            predicate: SyntacticPredicate,
            transform: SemanticTransform
        )
        .Where(static context => context is not null);

        // Emit the diagnostics, if needed
        var diagnostics = provider
            .Select(static (item, _) => item?.Diagnostics)
            .Where(static item => item?.Count > 0);

        context.RegisterSourceOutput(diagnostics, ReportDiagnostic);

        // output code
        var entityClasses = provider
            .Select(static (item, _) => item?.EntityClass)
            .Where(static item => item is not null);

        context.RegisterSourceOutput(entityClasses, Execute);
    }

    private static void ReportDiagnostic(SourceProductionContext context, EquatableArray<Diagnostic>? diagnostics)
    {
        if (diagnostics == null)
            return;

        foreach (var diagnostic in diagnostics)
            context.ReportDiagnostic(diagnostic);
    }

    private static void Execute(SourceProductionContext context, EquatableClass? entityClass)
    {
        if (entityClass == null)
            return;

        var qualifiedName = entityClass.EntityNamespace is null
            ? entityClass.EntityName
            : $"{entityClass.EntityNamespace}.{entityClass.EntityName}";

        var source = EquatableWriter.Generate(entityClass);

        context.AddSource($"{qualifiedName}.Equatable.g.cs", source);
    }


    private static bool SyntacticPredicate(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        return (syntaxNode is ClassDeclarationSyntax classDeclaration && !classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
            || (syntaxNode is RecordDeclarationSyntax recordDeclaration && !recordDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
            || (syntaxNode is StructDeclarationSyntax structDeclaration && !structDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword));
    }

    private static EquatableContext? SemanticTransform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.TargetSymbol is not INamedTypeSymbol targetSymbol)
            return null;

        var classNamespace = targetSymbol.ContainingNamespace.ToDisplayString();
        var className = targetSymbol.Name;

        var propertySymbols = GetProperties(targetSymbol);

        var propertyArray = propertySymbols
            .Select(p => CreateProperty(p))
            .ToArray() ?? [];

        var entity = new EquatableClass(classNamespace, className, propertyArray);
        return new EquatableContext(entity, null);
    }


    private static IEnumerable<IPropertySymbol> GetProperties(INamedTypeSymbol targetSymbol)
    {
        var properties = new Dictionary<string, IPropertySymbol>();

        var currentSymbol = targetSymbol;

        // get nested properties
        while (currentSymbol != null)
        {
            var propertySymbols = currentSymbol
                .GetMembers()
                .Where(m => m.Kind == SymbolKind.Property)
                .OfType<IPropertySymbol>()
                .Where(IsIncluded)
                .Where(p => !properties.ContainsKey(p.Name));

            foreach (var propertySymbol in propertySymbols)
                properties.Add(propertySymbol.Name, propertySymbol);

            currentSymbol = currentSymbol.BaseType;
        }

        return properties.Values;
    }

    private static EquatableProperty CreateProperty(IPropertySymbol propertySymbol)
    {
        var propertyType = propertySymbol.Type.ToDisplayString();
        var propertyName = propertySymbol.Name;

        // look for custom equality
        var attributes = propertySymbol.GetAttributes();
        if (attributes == null || attributes.Length == 0)
        {
            return new EquatableProperty(
                propertyName,
                propertyType,
                ComparerTypes.Default);
        }

        // search for known attribute
        foreach (var attribute in attributes)
        {
            (var comparerType, var comparerName, var comparerInstance) = GetComparer(attribute);

            if (!comparerType.HasValue)
                continue;

            return new EquatableProperty(
                propertyName,
                propertyType,
                comparerType.Value,
                comparerName,
                comparerInstance);
        }

        return new EquatableProperty(
            propertyName,
            propertyType,
            ComparerTypes.Default);
    }


    private static (ComparerTypes? comparerType, string? comparerName, string? comparerInstance) GetComparer(AttributeData? attribute)
    {
        // known attributes
        if (attribute == null || attribute.AttributeClass is not { ContainingNamespace: { Name: "Attributes", ContainingNamespace.Name: "Equatable" } })
            return (null, null, null);

        var className = attribute.AttributeClass?.Name;

        return className switch
        {
            "DictionaryEqualityAttribute" => (ComparerTypes.Dictionary, null, null),
            "HashSetEqualityAttribute" => (ComparerTypes.HashSet, null, null),
            "ReferenceEqualityAttribute" => (ComparerTypes.Reference, null, null),
            "SequenceEqualityAttribute" => (ComparerTypes.Sequence, null, null),
            "StringEqualityAttribute" => GetStringComparer(attribute),
            "EqualityComparerAttribute" => GetEqualityComparer(attribute),
            _ => (null, null, null),
        };
    }

    private static (ComparerTypes? comparerType, string? comparerName, string? comparerInstance) GetStringComparer(AttributeData? attribute)
    {
        var argument = attribute?.ConstructorArguments.FirstOrDefault();
        if (argument == null || !argument.HasValue)
            return (ComparerTypes.String, "CurrentCulture", null);

        var comparerName = argument?.Value switch
        {
            0 => "CurrentCulture",
            1 => "CurrentCultureIgnoreCase",
            2 => "InvariantCulture",
            3 => "InvariantCultureIgnoreCase",
            4 => "Ordinal",
            5 => "OrdinalIgnoreCase",
            _ => "CurrentCulture"
        };

        return (ComparerTypes.String, comparerName, null);
    }

    private static (ComparerTypes? comparerType, string? comparerName, string? comparerInstance) GetEqualityComparer(AttributeData? attribute)
    {
        if (attribute == null)
            return (ComparerTypes.Default, null, null);

        // attribute constructor
        var comparerType = attribute.ConstructorArguments.FirstOrDefault();
        if (comparerType.Value is INamedTypeSymbol typeSymbol)
        {
            return (ComparerTypes.Custom, typeSymbol.ToDisplayString(), null);
        }

        // generic attribute
        var attributeClass = attribute.AttributeClass;
        if (attributeClass is { IsGenericType: true }
            && attributeClass.TypeArguments.Length == attributeClass.TypeParameters.Length
            && attributeClass.TypeArguments.Length == 1)
        {
            var typeArgument = attributeClass.TypeArguments[0];
            var comparerName = typeArgument.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            return (ComparerTypes.Custom, comparerName, null);
        }


        return (ComparerTypes.Default, null, null);
    }


    private static bool IsIncluded(IPropertySymbol propertySymbol)
    {
        var attributes = propertySymbol.GetAttributes();
        if (attributes.Length > 0 && attributes.Any(
                a => a.AttributeClass is
                {
                    Name: "IgnoreEqualityAttribute",
                    ContainingNamespace:
                    {
                        Name: "Attributes",
                        ContainingNamespace.Name: "Equatable"
                    }
                }))
        {
            return false;
        }

        return !propertySymbol.IsIndexer && propertySymbol.DeclaredAccessibility == Accessibility.Public;
    }
}
