using System.Diagnostics;

namespace Equatable.Attributes;

/// <summary>
/// Use <see cref="Enumerable.SequenceEqual{TSource}(IEnumerable{TSource}, IEnumerable{TSource})"/> in Equals and GetHashCode implementations
/// </summary>
[Conditional("EQUATABLE_GENERATOR")]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SequenceEqualityAttribute : Attribute;
