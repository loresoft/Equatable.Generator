using System.Diagnostics;

namespace Equatable.Attributes;

[Conditional("EQUATABLE_GENERATOR")]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class EqualityComparerAttribute(Type equalityType, string instanceName = "Default") : Attribute
{
    public Type EqualityType { get; } = equalityType;

    public string InstanceName { get; } = instanceName;
}
