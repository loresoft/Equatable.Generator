using System.Diagnostics;

namespace Equatable.Attributes;

/// <summary>
/// Ignore property in Equals and GetHashCode implementations
/// </summary>
[Conditional("EQUATABLE_GENERATOR")]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class IgnoreEqualityAttribute : Attribute;
