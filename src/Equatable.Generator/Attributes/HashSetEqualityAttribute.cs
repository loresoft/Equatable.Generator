using System.Diagnostics;

namespace Equatable.Attributes;

/// <summary>
/// Use <see cref="ISet{T}.SetEquals(IEnumerable{T})"/> in Equals and GetHashCode implementations
/// </summary>
[Conditional("EQUATABLE_GENERATOR")]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class HashSetEqualityAttribute : Attribute;
