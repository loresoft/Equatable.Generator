using System.Text;

using Equatable.Generator.Infrastructure;
using Equatable.Generator.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Equatable.Generator;

[Generator]
public class EquatableGenerator : IIncrementalGenerator
{
    private static readonly SymbolDisplayFormat FullyQualifiedNullableFormat = SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
    private static readonly SymbolDisplayFormat NameAndNamespaces = new(SymbolDisplayGlobalNamespaceStyle.Omitted, SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, SymbolDisplayGenericsOptions.None);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // add embedded attributes and extensions as additional files to compilation
        context.RegisterPostInitializationOutput(static ctx =>
        {
            ctx.AddEmbeddedAttributeDefinition();

            ctx.AddSource(
                hintName: "Equatable.Attributes.g.cs",
                sourceText: SourceText.From(LoadEmbedded("Attributes.cs"), Encoding.UTF8)
            );
            ctx.AddSource(
                hintName: "Equatable.Comparer.g.cs",
                sourceText: SourceText.From(LoadEmbedded("Comparer.cs"), Encoding.UTF8)
            );
        });

        var provider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "Equatable.Attributes.EquatableAttribute",
                predicate: SyntacticPredicate,
                transform: SemanticTransform
            )
            .Where(static context => context is not null)
            .WithTrackingName("EquatableGenerator");

        // output code
        var entityClasses = provider
            .Where(static item => item is not null);

        context.RegisterSourceOutput(entityClasses, Execute);
    }


    private static void Execute(SourceProductionContext context, EquatableClass? entityClass)
    {
        if (entityClass == null)
            return;

        var source = EquatableWriter.Generate(entityClass);

        context.AddSource(entityClass.FileName, source);
    }


    private static bool SyntacticPredicate(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        return (syntaxNode is ClassDeclarationSyntax classDeclaration && !classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
            || (syntaxNode is RecordDeclarationSyntax recordDeclaration && !recordDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
            || (syntaxNode is StructDeclarationSyntax structDeclaration && !structDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword));
    }

    private static EquatableClass? SemanticTransform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.TargetSymbol is not INamedTypeSymbol targetSymbol)
            return null;

        var fullyQualified = targetSymbol.ToDisplayString(FullyQualifiedNullableFormat);
        var classNamespace = targetSymbol.ContainingNamespace.ToDisplayString();
        var className = targetSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        var qualifiedName = targetSymbol.ToDisplayString(NameAndNamespaces);
        var fileName = $"{qualifiedName}.Equatable.g.cs";

        // support nested types
        var containingTypes = GetContainingTypes(targetSymbol);

        var baseHashCode = GetBaseHashCodeMethod(targetSymbol);
        var baseEquals = GetBaseEqualsMethod(targetSymbol);
        var baseEquatable = GetBaseEquatableType(targetSymbol);

        var propertySymbols = GetProperties(targetSymbol, baseHashCode == null && baseEquatable == null);

        var propertyArray = propertySymbols
            .Select(CreateProperty)
            .ToArray() ?? [];

        return new EquatableClass(
            FullyQualified: fullyQualified,
            EntityNamespace: classNamespace,
            EntityName: className,
            FileName: fileName,
            ContainingTypes: containingTypes,
            Properties: propertyArray,
            IsRecord: targetSymbol.IsRecord,
            IsValueType: targetSymbol.IsValueType,
            IsSealed: targetSymbol.IsSealed,
            IncludeBaseEqualsMethod: baseEquals != null || baseEquatable != null,
            IncludeBaseHashMethod: baseHashCode != null || baseEquatable != null
        );
    }


    private static IEnumerable<IPropertySymbol> GetProperties(INamedTypeSymbol targetSymbol, bool includeBaseProperties = true)
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

            if (!includeBaseProperties)
                break;

            currentSymbol = currentSymbol.BaseType;
        }

        return properties.Values;
    }

    private static EquatableProperty CreateProperty(IPropertySymbol propertySymbol)
    {
        var propertyType = propertySymbol.Type.ToDisplayString(FullyQualifiedNullableFormat);
        var propertyName = propertySymbol.Name;
        var isValueType = propertySymbol.Type.IsValueType;

        var underlyingType = UnwrapNullable(propertySymbol.Type);
        var isNullable = !SymbolEqualityComparer.Default.Equals(underlyingType, propertySymbol.Type);

        // string properties compare with StringComparison.Ordinal by default, which is equivalent to string.Equals
        var isString = IsString(underlyingType);

        // only use the == operator when the type actually supports it, otherwise fall back to EqualityComparer<T>.Default
        var defaultComparer = isString
            ? ComparerTypes.String
            : isValueType && HasEqualityOperator(underlyingType)
                ? ComparerTypes.ValueType
                : ComparerTypes.Default;

        // look for custom equality
        var attributes = propertySymbol.GetAttributes();
        if (attributes.Length == 0)
        {
            return new EquatableProperty(
                propertyName,
                propertyType,
                defaultComparer,
                IsNullable: isNullable);
        }

        // search for known attribute
        foreach (var attribute in attributes)
        {
            (var comparerType, var comparerName, var comparerInstance) = GetComparer(attribute);

            if (!comparerType.HasValue)
                continue;

            var isValid = ValidateComparer(propertySymbol, comparerType);
            if (!isValid)
                continue;

            return new EquatableProperty(
                propertyName,
                propertyType,
                comparerType.Value,
                comparerName,
                comparerInstance,
                isNullable);
        }

        // fall back to default comparer if no valid attribute was found
        return new EquatableProperty(
            propertyName,
            propertyType,
            defaultComparer,
            IsNullable: isNullable);
    }

    private static bool ValidateComparer(IPropertySymbol propertySymbol, ComparerTypes? comparerType)
    {
        // don't need to validate these types
        if (comparerType is null or ComparerTypes.Default or ComparerTypes.Reference or ComparerTypes.Custom)
            return true;

        if (comparerType == ComparerTypes.String)
            return IsString(propertySymbol.Type);

        if (comparerType == ComparerTypes.Dictionary)
            return propertySymbol.Type.AllInterfaces.Any(IsDictionary);

        if (comparerType == ComparerTypes.HashSet)
            return propertySymbol.Type.AllInterfaces.Any(IsEnumerable);

        if (comparerType == ComparerTypes.Sequence)
            return propertySymbol.Type.AllInterfaces.Any(IsEnumerable);

        return true;
    }


    private static (ComparerTypes? comparerType, string? comparerName, string? comparerInstance) GetComparer(AttributeData? attribute)
    {
        if (!IsKnownAttribute(attribute))
            return (null, null, null);

        var className = attribute?.AttributeClass?.Name;

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
        if (attribute == null || attribute.ConstructorArguments.Length != 1)
            return (ComparerTypes.String, null, null);

        var argument = attribute.ConstructorArguments[0];

        if (argument.Value is not int value)
            return (ComparerTypes.String, null, null);

        var comparerName = value switch
        {
            0 => "CurrentCulture",
            1 => "CurrentCultureIgnoreCase",
            2 => "InvariantCulture",
            3 => "InvariantCultureIgnoreCase",
            4 => "Ordinal",
            5 => "OrdinalIgnoreCase",
            _ => null
        };

        return (ComparerTypes.String, comparerName, null);
    }

    private static (ComparerTypes? comparerType, string? comparerName, string? comparerInstance) GetEqualityComparer(AttributeData? attribute)
    {
        if (attribute == null || attribute.ConstructorArguments.Length != 2)
            return (ComparerTypes.Default, null, null);

        var comparerArgument = attribute.ConstructorArguments[0];
        if (comparerArgument.Value is not INamedTypeSymbol typeSymbol)
            return (ComparerTypes.Default, null, null); // invalid syntax found

        var comparerName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        var instanceArgument = attribute.ConstructorArguments[1];
        var comparerInstance = instanceArgument.Value as string;

        return (ComparerTypes.Custom, comparerName, comparerInstance);
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

    private static bool IsValueType(INamedTypeSymbol targetSymbol)
    {
        return targetSymbol is
        {
            Name: nameof(ValueType) or nameof(Object),
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

    private static bool IsString(ITypeSymbol targetSymbol)
    {
        return targetSymbol is
        {
            Name: nameof(String),
            ContainingNamespace.Name: "System"
        };
    }

    private static ITypeSymbol UnwrapNullable(ITypeSymbol targetSymbol)
    {
        // the constructed Nullable<T> reports the special type on its original definition
        if (targetSymbol is INamedTypeSymbol { IsGenericType: true, TypeArguments.Length: 1 } namedSymbol
            && namedSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            return namedSymbol.TypeArguments[0];
        }

        return targetSymbol;
    }

    private static bool HasEqualityOperator(ITypeSymbol targetSymbol)
    {
        // enums and built-in types support == via a built-in operator, not a declared member
        if (targetSymbol.TypeKind == TypeKind.Enum)
            return true;

        switch (targetSymbol.SpecialType)
        {
            case SpecialType.System_Boolean:
            case SpecialType.System_Byte:
            case SpecialType.System_Char:
            case SpecialType.System_Decimal:
            case SpecialType.System_Double:
            case SpecialType.System_Int16:
            case SpecialType.System_Int32:
            case SpecialType.System_Int64:
            case SpecialType.System_IntPtr:
            case SpecialType.System_SByte:
            case SpecialType.System_Single:
            case SpecialType.System_UInt16:
            case SpecialType.System_UInt32:
            case SpecialType.System_UInt64:
            case SpecialType.System_UIntPtr:
                return true;
        }

        return targetSymbol
            .GetMembers(WellKnownMemberNames.EqualityOperatorName)
            .OfType<IMethodSymbol>()
            .Any(method => method.MethodKind == MethodKind.UserDefinedOperator);
    }


    private static EquatableArray<ContainingClass> GetContainingTypes(INamedTypeSymbol targetSymbol)
    {
        if (targetSymbol.ContainingType is null)
            return Array.Empty<ContainingClass>();

        var list = new List<ContainingClass>();
        var currentSymbol = targetSymbol.ContainingType;

        while (currentSymbol != null)
        {
            var containingClass = new ContainingClass(
                EntityName: currentSymbol.Name,
                IsRecord: currentSymbol.IsRecord,
                IsValueType: currentSymbol.IsValueType
            );

            list.Add(containingClass);

            currentSymbol = currentSymbol.ContainingType;
        }

        list.Reverse();

        return list.ToArray();
    }

    private static IMethodSymbol? GetBaseHashCodeMethod(INamedTypeSymbol targetSymbol)
    {
        // don't use for value types
        if (targetSymbol.BaseType == null)
            return null;

        var currentSymbol = targetSymbol.BaseType;

        // check all base types for GetHashCode method override
        while (currentSymbol != null)
        {
            // stop at ValueType
            if (IsValueType(currentSymbol))
                return null;

            var methodSymbol = currentSymbol
                .GetMembers(nameof(GetHashCode))
                .OfType<IMethodSymbol>()
                .FirstOrDefault(method => method.IsOverride
                    && method.DeclaredAccessibility == Accessibility.Public
                    && !method.IsStatic
                    && method.Parameters.Length == 0
                    && method.ReturnType.SpecialType == SpecialType.System_Int32
                    && !method.IsAbstract
                );

            if (methodSymbol != null)
                return methodSymbol;

            currentSymbol = currentSymbol.BaseType;
        }

        return null;
    }

    private static IMethodSymbol? GetBaseEqualsMethod(INamedTypeSymbol targetSymbol)
    {
        // don't use for value types
        if (targetSymbol.BaseType == null)
            return null;

        var currentSymbol = targetSymbol.BaseType;

        // check all base types for Equals method override
        while (currentSymbol != null)
        {
            // stop at ValueType
            if (IsValueType(currentSymbol))
                return null;

            var methodSymbol = currentSymbol
                .GetMembers(nameof(Equals))
                .OfType<IMethodSymbol>()
                .FirstOrDefault(method => method.IsOverride
                    && method.DeclaredAccessibility == Accessibility.Public
                    && !method.IsStatic
                    && method.Parameters.Length == 1
                    && method.ReturnType.SpecialType == SpecialType.System_Boolean
                    && method.Parameters[0].Type.SpecialType == SpecialType.System_Object
                    && !method.IsAbstract
                );

            if (methodSymbol != null)
                return methodSymbol;

            currentSymbol = currentSymbol.BaseType;
        }

        return null;
    }

    private static INamedTypeSymbol? GetBaseEquatableType(INamedTypeSymbol targetSymbol)
    {
        if (targetSymbol.BaseType == null)
            return null;

        var currentSymbol = targetSymbol.BaseType;

        // check all base types for Equals method override
        while (currentSymbol != null)
        {
            // stop at ValueType
            if (IsValueType(currentSymbol))
                return null;

            var attributes = currentSymbol.GetAttributes();
            if (attributes.Length > 0 && attributes.Any(a => IsKnownAttribute(a) && a.AttributeClass?.Name == "EquatableAttribute"))
            {
                return currentSymbol;
            }

            currentSymbol = currentSymbol.BaseType;
        }

        return null;

    }


    private static string LoadEmbedded(string name)
    {
        var assembly = typeof(EquatableGenerator).Assembly;
        var resourceName = "Equatable.Generator.Embedded." + name;

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource not found: {resourceName}");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }


}
