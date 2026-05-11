using Microsoft.CodeAnalysis;

namespace Equatable.SourceGenerator.MessagePack;

[Generator]
public class MessagePackEquatableGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        EquatableGenerator.RegisterProvider(context,
            fullyQualifiedMetadataName: "Equatable.Attributes.MessagePackEquatableAttribute",
            trackingName: "MessagePackEquatableAttribute",
            propertyFilter: IsIncludedMessagePack);
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

        return attributes.Any(a => a.AttributeClass is { Name: "KeyAttribute", ContainingNamespace.Name: "MessagePack" });
    }
}
