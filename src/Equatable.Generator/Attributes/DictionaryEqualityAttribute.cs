using System.Diagnostics;

namespace Equatable.Attributes;

/// <summary>
/// Use a dictionary based comparer to determine if dictionaries are equal.
/// </summary>
/// <remarks>
/// When <paramref name="sequential"/> is <c>false</c> (default), equality and hash code are
/// insertion-order independent — two dictionaries with the same key/value pairs in any order
/// are considered equal and produce the same hash code.
///
/// When <paramref name="sequential"/> is <c>true</c>, insertion order is part of the semantic:
/// equality uses <c>SequenceEqual</c> on the key-value pair sequence and hash code is computed
/// sequentially — two dictionaries with the same pairs in different order are NOT equal.
/// Use this only when dictionary ordering carries business meaning.
/// </remarks>
[Conditional("EQUATABLE_GENERATOR")]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class DictionaryEqualityAttribute(bool sequential = false) : Attribute
{
    public bool Sequential { get; } = sequential;
}
