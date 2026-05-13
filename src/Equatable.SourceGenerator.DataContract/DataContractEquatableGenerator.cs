using Microsoft.CodeAnalysis;

namespace Equatable.SourceGenerator.DataContract;

[Generator]
public class DataContractEquatableGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        EquatableGenerator.RegisterProvider(context,
            fullyQualifiedMetadataName: "Equatable.Attributes.DataContract.DataContractEquatableAttribute",
            trackingName: "DataContractEquatableAttribute",
            propertyFilter: IsIncludedDataContract,
            postProcessProperty: EquatableGenerator.InferCollectionComparer);
    }

    private static bool IsIncludedDataContract(IPropertySymbol propertySymbol)
    {
        if (!EquatableGenerator.IsPublicInstanceProperty(propertySymbol))
            return false;

        var attributes = propertySymbol.GetAttributes();
        if (attributes.Length == 0)
            return false;

        if (attributes.Any(a => a.AttributeClass is
            {
                Name: "IgnoreDataMemberAttribute",
                ContainingNamespace: { Name: "Serialization", ContainingNamespace: { Name: "Runtime", ContainingNamespace.Name: "System" } }
            }))
            return false;

        return attributes.Any(a => a.AttributeClass is
        {
            Name: "DataMemberAttribute",
            ContainingNamespace: { Name: "Serialization", ContainingNamespace: { Name: "Runtime", ContainingNamespace.Name: "System" } }
        });
    }
}
