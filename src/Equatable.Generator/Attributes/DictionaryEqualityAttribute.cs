using System.Diagnostics;

namespace Equatable.Attributes;

/// <summary>
/// Use a dictionary based comparer to determine if dictionaries are equal
/// </summary>
[Conditional("EQUATABLE_GENERATOR")]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class DictionaryEqualityAttribute : Attribute;
