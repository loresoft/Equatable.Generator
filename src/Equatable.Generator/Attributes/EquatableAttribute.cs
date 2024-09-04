using System.Diagnostics;

namespace Equatable.Attributes;

[Conditional("EQUATABLE_GENERATOR")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class EquatableAttribute : Attribute;
