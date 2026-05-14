using System.Diagnostics;

namespace Equatable.Attributes;

/// <summary>
/// Use a dictionary based comparer to determine if dictionaries are equal.
/// Two dictionaries are equal when they contain the same key/value pairs, regardless of insertion order.
/// </summary>
[Conditional("EQUATABLE_GENERATOR")]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class DictionaryEqualityAttribute : Attribute
{
}
