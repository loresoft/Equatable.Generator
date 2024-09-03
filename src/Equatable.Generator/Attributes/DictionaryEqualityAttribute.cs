using System.Diagnostics;

namespace Equatable.Attributes;

[Conditional("EQUATABLE_GENERATOR")]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class DictionaryEqualityAttribute : Attribute;
