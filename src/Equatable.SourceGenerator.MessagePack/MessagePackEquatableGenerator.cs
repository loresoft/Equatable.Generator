using Microsoft.CodeAnalysis;

namespace Equatable.SourceGenerator.MessagePack;

[Generator]
public class MessagePackEquatableGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        EquatableGenerator.RegisterProvider(context,
            fullyQualifiedMetadataName: "Equatable.Attributes.MessagePack.MessagePackEquatableAttribute",
            trackingName: "MessagePackEquatableAttribute",
            propertyFilter: IsIncludedMessagePack,
            postProcessProperty: EquatableGenerator.InferCollectionComparer);
    }

    private static bool IsIncludedMessagePack(IPropertySymbol propertySymbol)
    {
        if (!EquatableGenerator.IsPublicInstanceProperty(propertySymbol))
            return false;

        var attributes = propertySymbol.GetAttributes();
        if (attributes.Length == 0)
            return false;

        if (attributes.Any(a => a.AttributeClass is { Name: "IgnoreMemberAttribute", ContainingNamespace.Name: "MessagePack" }))
            return false;

        if (attributes.Any(a => a.AttributeClass is
            {
                Name: "IgnoreEqualityAttribute",
                ContainingNamespace: { Name: "Attributes", ContainingNamespace.Name: "Equatable" }
            }))
            return false;

        return attributes.Any(a => a.AttributeClass is { Name: "KeyAttribute", ContainingNamespace.Name: "MessagePack" });
    }
}
