using System.Diagnostics;

namespace Equatable.Attributes.MessagePack;

/// <summary>
/// Marks the class to source generate overrides for Equals and GetHashCode,
/// including only properties decorated with <c>MessagePack.KeyAttribute</c>
/// and excluding properties decorated with <c>MessagePack.IgnoreMemberAttribute</c>.
/// </summary>
[Conditional("EQUATABLE_GENERATOR")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class MessagePackEquatableAttribute : Attribute;
