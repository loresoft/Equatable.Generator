using System.Diagnostics;

namespace Equatable.Attributes;

/// <summary>
/// Marks the class to source generate overrides for Equals and GetHashCode.
/// </summary>
[Conditional("EQUATABLE_GENERATOR")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class EquatableAttribute : Attribute;
