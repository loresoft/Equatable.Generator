using System.Diagnostics;

namespace Equatable.Attributes;

/// <summary>
/// Use <see cref="Object.ReferenceEquals(object, object)"/> to determines whether instances are the same instance
/// </summary>
[Conditional("EQUATABLE_GENERATOR")]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ReferenceEqualityAttribute : Attribute;
