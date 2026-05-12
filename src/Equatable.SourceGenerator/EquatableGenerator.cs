using Equatable.SourceGenerator.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Equatable.SourceGenerator;

[Generator]
public class EquatableGenerator : IIncrementalGenerator
{
    private static readonly SymbolDisplayFormat FullyQualifiedNullableFormat = SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
    private static readonly SymbolDisplayFormat NameAndNamespaces = new(SymbolDisplayGlobalNamespaceStyle.Omitted, SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, SymbolDisplayGenericsOptions.None);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        RegisterProvider(context,
            fullyQualifiedMetadataName: "Equatable.Attributes.EquatableAttribute",
            trackingName: "EquatableAttribute",
            propertyFilter: IsIncluded);
    }

    public static void RegisterProvider(
        IncrementalGeneratorInitializationContext context,
        string fullyQualifiedMetadataName,
        string trackingName,
        Func<IPropertySymbol, bool> propertyFilter)
    {
        var provider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: fullyQualifiedMetadataName,
                predicate: SyntacticPredicate,
                transform: (ctx, ct) => SemanticTransform(ctx, ct, propertyFilter)
            )
            .Where(static item => item is not null)
            .WithTrackingName(trackingName);

        context.RegisterSourceOutput(provider, Execute);
    }


    public static void Execute(SourceProductionContext context, EquatableClass? entityClass)
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

    private static EquatableClass? SemanticTransform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken, Func<IPropertySymbol, bool> propertyFilter)
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

        var propertySymbols = GetProperties(targetSymbol, baseHashCode == null && baseEquatable == null, propertyFilter);

        var propertyArray = propertySymbols
            .Select(CreateProperty)
            .ToArray() ?? [];

        // the seed value of the hash code method
        var seedHash = 0;

        if (baseHashCode != null)
            seedHash = (seedHash * HashFactor) + GetFNVHashCode(baseHashCode.ContainingSymbol.Name);
        else if (baseEquatable != null)
            seedHash = (seedHash * HashFactor) + GetFNVHashCode(baseEquatable.Name);
        else if (baseEquals != null)
            seedHash = (seedHash * HashFactor) + GetFNVHashCode(baseEquals.ContainingSymbol.Name);

        foreach (var property in propertyArray)
            seedHash = (seedHash * HashFactor) + GetFNVHashCode(property.PropertyName);

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
            IncludeBaseHashMethod: baseHashCode != null || baseEquatable != null,
            SeedHash: seedHash
        );
    }


    private static IEnumerable<IPropertySymbol> GetProperties(INamedTypeSymbol targetSymbol, bool includeBaseProperties, Func<IPropertySymbol, bool> propertyFilter)
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
                .Where(propertyFilter)
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
        var defaultComparer = isValueType && HasEqualityOperator(propertySymbol.Type) ? ComparerTypes.ValueType : ComparerTypes.Default;

        // look for an explicit equality attribute
        var attributes = propertySymbol.GetAttributes();
        foreach (var attribute in attributes)
        {
            (var comparerType, var comparerName, var comparerInstance) = GetComparer(attribute);

            if (!comparerType.HasValue)
                continue;

            var isValid = ValidateComparer(propertySymbol, comparerType);
            if (!isValid)
                return new EquatableProperty(propertyName, propertyType, defaultComparer);

            // for collection attributes, check if the element type itself contains nested collections
            // that need a composed comparer rather than EqualityComparer<TValue>.Default.
            // BuildArrayComparerExpression always returns a non-null expression for arrays
            // (including multi-dimensional, which use MultiDimensionalArrayEqualityComparer).
            if (comparerType is ComparerTypes.Dictionary or ComparerTypes.OrderedDictionary or ComparerTypes.HashSet or ComparerTypes.Sequence)
            {
                string? expression = propertySymbol.Type switch
                {
                    INamedTypeSymbol namedType => BuildCollectionComparerExpression(namedType, comparerType.Value),
                    IArrayTypeSymbol arrayType => BuildArrayComparerExpression(arrayType),
                    _ => null
                };
                if (expression != null)
                    return new EquatableProperty(propertyName, propertyType, ComparerTypes.Expression, ComparerExpression: expression);
            }

            return new EquatableProperty(propertyName, propertyType, comparerType.Value, comparerName, comparerInstance);
        }

        return new EquatableProperty(propertyName, propertyType, defaultComparer);
    }

    // Returns a fully-qualified IEqualityComparer instance expression for a collection type
    // when its element type is itself a collection (requires composition).
    // Returns null when EqualityComparer<T>.Default is sufficient (element is a plain type).
    // Depth is unbounded — recursion terminates naturally when an element type is not a
    // recognised collection interface (BuildElementComparerExpression returns null).
    private static string? BuildCollectionComparerExpression(INamedTypeSymbol collectionType, ComparerTypes kind)
    {
        // unwrap nullable wrapper
        var unwrapped = collectionType.IsGenericType
            && collectionType.OriginalDefinition.SpecialType == SpecialType.None
            && collectionType.Name == "Nullable"
            ? collectionType.TypeArguments[0] as INamedTypeSymbol ?? collectionType
            : collectionType;

        // find the collection interface and extract element type(s)
        INamedTypeSymbol? dictInterface = IsDictionary(unwrapped) ? unwrapped
            : unwrapped.AllInterfaces.FirstOrDefault(IsDictionary);

        INamedTypeSymbol? enumInterface = IsEnumerable(unwrapped) ? unwrapped
            : unwrapped.AllInterfaces.FirstOrDefault(IsEnumerable);

        if ((kind == ComparerTypes.Dictionary || kind == ComparerTypes.OrderedDictionary) && dictInterface != null)
        {
            var keyType = dictInterface.TypeArguments[0];
            var valueType = dictInterface.TypeArguments[1];

            var keyExpr = BuildElementComparerExpression(keyType);
            var valueExpr = BuildElementComparerExpression(valueType);

            var keyTypeFq = keyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var valueTypeFq = valueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            var isReadOnly = IsReadOnlyDictionary(unwrapped)
                || (IsDictionary(unwrapped) is false && unwrapped.AllInterfaces.Any(IsReadOnlyDictionary));

            if (kind == ComparerTypes.OrderedDictionary)
            {
                // ordered: always emit an explicit comparer so the ordered semantics are enforced,
                // even when key/value types don't need composition themselves
                keyExpr ??= $"global::System.Collections.Generic.EqualityComparer<{keyTypeFq}>.Default";
                valueExpr ??= $"global::System.Collections.Generic.EqualityComparer<{valueTypeFq}>.Default";

                var orderedClass = isReadOnly
                    ? "global::Equatable.Comparers.OrderedReadOnlyDictionaryEqualityComparer"
                    : "global::Equatable.Comparers.OrderedDictionaryEqualityComparer";

                return $"new {orderedClass}<{keyTypeFq}, {valueTypeFq}>({keyExpr}, {valueExpr})";
            }

            // unordered: only compose when at least one argument needs a non-default comparer
            if (keyExpr == null && valueExpr == null)
                return null;

            keyExpr ??= $"global::System.Collections.Generic.EqualityComparer<{keyTypeFq}>.Default";
            valueExpr ??= $"global::System.Collections.Generic.EqualityComparer<{valueTypeFq}>.Default";

            var comparerClass = isReadOnly
                ? "global::Equatable.Comparers.ReadOnlyDictionaryEqualityComparer"
                : "global::Equatable.Comparers.DictionaryEqualityComparer";

            return $"new {comparerClass}<{keyTypeFq}, {valueTypeFq}>({keyExpr}, {valueExpr})";
        }

        if ((kind == ComparerTypes.HashSet || kind == ComparerTypes.Sequence) && enumInterface != null)
        {
            var elementType = enumInterface.TypeArguments[0];
            var elementExpr = BuildElementComparerExpression(elementType);

            if (elementExpr == null)
                return null;

            var elementTypeFq = elementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var comparerClass = kind == ComparerTypes.HashSet
                ? "global::Equatable.Comparers.HashSetEqualityComparer"
                : "global::Equatable.Comparers.SequenceEqualityComparer";

            return $"new {comparerClass}<{elementTypeFq}>({elementExpr})";
        }

        return null;
    }

    // Returns a comparer expression for a single element type, or null if EqualityComparer<T>.Default suffices.
    // Terminates naturally when elementType is not a recognised collection interface.
    // visited guards against self-referential types (e.g. class A : IEnumerable<A>).
    private static string? BuildElementComparerExpression(ITypeSymbol elementType, HashSet<ITypeSymbol>? visited = null)
    {
        if (elementType is IArrayTypeSymbol arrayType)
            return BuildArrayComparerExpression(arrayType);

        if (elementType is not INamedTypeSymbol named)
            return null;

        // string implements IEnumerable<char> but must use default equality, not SequenceEqualityComparer<char>
        if (IsString(named))
            return null;

        visited ??= new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        if (!visited.Add(elementType))
            return null; // cycle detected

        // always scan AllInterfaces — covers concrete types (List<T>, HashSet<T>, Dictionary<K,V>, etc.)
        // dictionary check takes priority over enumerable
        var asDictInterface = IsDictionary(named) ? named
            : named.AllInterfaces.FirstOrDefault(IsDictionary);

        if (asDictInterface != null)
        {
            var isReadOnly = IsReadOnlyDictionary(named)
                || (IsDictionary(named) is false && named.AllInterfaces.Any(IsReadOnlyDictionary));
            return BuildDictComparerExpression(asDictInterface, isReadOnly, visited);
        }

        var asEnumInterface = IsEnumerable(named) ? named
            : named.AllInterfaces.FirstOrDefault(IsEnumerable);

        if (asEnumInterface != null)
        {
            var innerType = asEnumInterface.TypeArguments[0];
            var innerExpr = BuildElementComparerExpression(innerType, visited);
            var innerTypeFq = innerType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            var isSet = named.AllInterfaces.Any(i => i is { Name: "ISet" or "IReadOnlySet", IsGenericType: true })
                || named is { Name: "ISet" or "IReadOnlySet", IsGenericType: true };
            var comparerClass = isSet
                ? "global::Equatable.Comparers.HashSetEqualityComparer"
                : "global::Equatable.Comparers.SequenceEqualityComparer";

            if (innerExpr != null)
                return $"new {comparerClass}<{innerTypeFq}>({innerExpr})";

            return $"{comparerClass}<{innerTypeFq}>.Default";
        }

        return null;
    }

    private static string BuildDictComparerExpression(INamedTypeSymbol dictInterface, bool isReadOnly, HashSet<ITypeSymbol>? visited = null)
    {
        var keyType = dictInterface.TypeArguments[0];
        var valueType = dictInterface.TypeArguments[1];
        var keyTypeFq = keyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var valueTypeFq = valueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        var keyExpr = BuildElementComparerExpression(keyType, visited)
            ?? $"global::System.Collections.Generic.EqualityComparer<{keyTypeFq}>.Default";
        var valueExpr = BuildElementComparerExpression(valueType, visited)
            ?? $"global::System.Collections.Generic.EqualityComparer<{valueTypeFq}>.Default";

        var comparerClass = isReadOnly
            ? "global::Equatable.Comparers.ReadOnlyDictionaryEqualityComparer"
            : "global::Equatable.Comparers.DictionaryEqualityComparer";

        return $"new {comparerClass}<{keyTypeFq}, {valueTypeFq}>({keyExpr}, {valueExpr})";
    }

    private static string BuildArrayComparerExpression(IArrayTypeSymbol arrayType)
    {
        var elementType = arrayType.ElementType;
        var elementTypeFq = elementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var innerExpr = BuildElementComparerExpression(elementType);

        if (arrayType.Rank > 1)
        {
            // Multi-dimensional arrays don't implement IEnumerable<T>; use the dedicated comparer
            // that iterates via Array.GetEnumerator() with no LINQ or intermediate allocations.
            if (innerExpr != null)
                return $"new global::Equatable.Comparers.MultiDimensionalArrayEqualityComparer<{elementTypeFq}>({innerExpr})";
            return $"global::Equatable.Comparers.MultiDimensionalArrayEqualityComparer<{elementTypeFq}>.Default";
        }

        if (innerExpr != null)
            return $"new global::Equatable.Comparers.SequenceEqualityComparer<{elementTypeFq}>({innerExpr})";
        return $"global::Equatable.Comparers.SequenceEqualityComparer<{elementTypeFq}>.Default";
    }

    private static bool IsReadOnlyDictionary(INamedTypeSymbol targetSymbol)
    {
        return targetSymbol is
        {
            Name: "IReadOnlyDictionary",
            IsGenericType: true,
            TypeArguments.Length: 2,
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

    private static bool ValidateComparer(IPropertySymbol propertySymbol, ComparerTypes? comparerType)
    {
        // don't need to validate these types
        if (comparerType is null or ComparerTypes.Default or ComparerTypes.Reference or ComparerTypes.Custom or ComparerTypes.Expression)
            return true;

        if (comparerType == ComparerTypes.String)
            return IsString(propertySymbol.Type);

        if (comparerType == ComparerTypes.Dictionary || comparerType == ComparerTypes.OrderedDictionary)
            return (propertySymbol.Type is INamedTypeSymbol nt && IsDictionary(nt))
                || propertySymbol.Type.AllInterfaces.Any(IsDictionary);

        if (comparerType == ComparerTypes.HashSet)
            return (propertySymbol.Type is INamedTypeSymbol ntHs && IsEnumerable(ntHs))
                || propertySymbol.Type.AllInterfaces.Any(IsEnumerable);

        if (comparerType == ComparerTypes.Sequence)
            return propertySymbol.Type is IArrayTypeSymbol
                || (propertySymbol.Type is INamedTypeSymbol ntSeq && IsEnumerable(ntSeq))
                || propertySymbol.Type.AllInterfaces.Any(IsEnumerable);

        return true;
    }


    private static (ComparerTypes? comparerType, string? comparerName, string? comparerInstance) GetComparer(AttributeData? attribute)
    {
        if (!IsKnownAttribute(attribute))
            return (null, null, null);

        var className = attribute?.AttributeClass?.Name;

        return className switch
        {
            "DictionaryEqualityAttribute" => GetDictionaryComparer(attribute),
            "HashSetEqualityAttribute" => (ComparerTypes.HashSet, null, null),
            "ReferenceEqualityAttribute" => (ComparerTypes.Reference, null, null),
            "SequenceEqualityAttribute" => (ComparerTypes.Sequence, null, null),
            "StringEqualityAttribute" => GetStringComparer(attribute),
            "EqualityComparerAttribute" => GetEqualityComparer(attribute),
            _ => (null, null, null),
        };
    }

    private static (ComparerTypes? comparerType, string? comparerName, string? comparerInstance) GetDictionaryComparer(AttributeData? attribute)
    {
        if (attribute == null)
            return (ComparerTypes.Dictionary, null, null);

        // named arg: [DictionaryEquality(sequential: true)]
        var namedArg = attribute.NamedArguments.FirstOrDefault(a => a.Key == "Sequential");
        if (namedArg.Key != null && namedArg.Value.Value is bool namedSequential && namedSequential)
            return (ComparerTypes.OrderedDictionary, null, null);

        // positional arg: [DictionaryEquality(true)]
        if (attribute.ConstructorArguments.Length > 0 &&
            attribute.ConstructorArguments[0].Value is bool positionalSequential && positionalSequential)
            return (ComparerTypes.OrderedDictionary, null, null);

        return (ComparerTypes.Dictionary, null, null);
    }

    private static (ComparerTypes? comparerType, string? comparerName, string? comparerInstance) GetStringComparer(AttributeData? attribute)
    {
        if (attribute == null || attribute.ConstructorArguments.Length != 1)
            return (ComparerTypes.Default, null, null);

        var argument = attribute.ConstructorArguments[0];

        if (argument.Value is not int value)
            return (ComparerTypes.String, "CurrentCulture", null);

        var comparerName = value switch
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


    public static bool IsPublicInstanceProperty(IPropertySymbol propertySymbol) =>
        !propertySymbol.IsIndexer && propertySymbol.DeclaredAccessibility == Accessibility.Public;

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

    private static bool HasEqualityOperator(ITypeSymbol typeSymbol)
    {
        // For Nullable<T>, check the underlying type T
        var typeToCheck = typeSymbol is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable
            ? nullable.TypeArguments[0]
            : typeSymbol;

        // Primitive types and enums always support ==
        if (typeToCheck.SpecialType != SpecialType.None || typeToCheck.TypeKind == TypeKind.Enum)
            return true;

        // Check for user-defined == operator
        return typeToCheck.GetMembers("op_Equality").Any();
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

    private static bool IsString(ITypeSymbol targetSymbol)
    {
        return targetSymbol is
        {
            Name: nameof(String),
            ContainingNamespace.Name: "System"
        };
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
            if (attributes.Length > 0 && attributes.Any(a => IsKnownAttribute(a)
                    && a.AttributeClass?.Name.EndsWith("EquatableAttribute") == true))
            {
                return currentSymbol;
            }

            currentSymbol = currentSymbol.BaseType;
        }

        return null;

    }


    private const int HashFactor = -1521134295;

    private const int FnvOffsetBias = unchecked((int)2166136261);
    private const int FnvPrime = 16777619;

    private static int GetFNVHashCode(string text)
    {
        var hashCode = FnvOffsetBias;
        for (int i = 0; i < text.Length; i++)
            hashCode = unchecked((hashCode ^ text[i]) * FnvPrime);

        return hashCode;
    }
}
