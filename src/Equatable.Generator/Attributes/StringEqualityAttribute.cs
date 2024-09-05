using System.Diagnostics;

namespace Equatable.Attributes;

/// <summary>
/// Use specified <see cref="StringComparer" /> when comparing strings
/// </summary>
/// <param name="comparisonType">The <see cref="StringComparison"/> to use</param>
[Conditional("EQUATABLE_GENERATOR")]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class StringEqualityAttribute(StringComparison comparisonType) : Attribute
{
    /// <summary>
    /// The <see cref="StringComparison"/> to use
    /// </summary>
    public StringComparison ComparisonType { get; } = comparisonType;
}
