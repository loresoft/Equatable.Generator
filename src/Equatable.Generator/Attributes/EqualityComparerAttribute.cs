using System.Diagnostics;

namespace Equatable.Attributes;

/// <summary>
/// Use the specified <see cref="IEqualityComparer{T}"/> in Equals and GetHashCode implementations
/// </summary>
/// <param name="equalityType">The <see cref="IEqualityComparer{T}"/> type to use</param>
/// <param name="instanceName">The singleton property name to get the instance to use from</param>
[Conditional("EQUATABLE_GENERATOR")]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class EqualityComparerAttribute(Type equalityType, string instanceName = "Default") : Attribute
{
    /// <summary>
    /// The <see cref="IEqualityComparer{T}"/> type to use
    /// </summary>
    public Type EqualityType { get; } = equalityType;

    /// <summary>
    /// The singleton property name to get the instance to use from
    /// </summary>
    public string InstanceName { get; } = instanceName;
}
