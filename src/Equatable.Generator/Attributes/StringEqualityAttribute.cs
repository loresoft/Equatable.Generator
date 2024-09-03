using System.Diagnostics;

namespace Equatable.Attributes;

[Conditional("EQUATABLE_GENERATOR")]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class StringEqualityAttribute(StringComparison comparisonType) : Attribute
{
    public StringComparison ComparisonType { get; } = comparisonType;
}
