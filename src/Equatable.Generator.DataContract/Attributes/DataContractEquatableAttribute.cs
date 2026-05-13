using System.Diagnostics;

namespace Equatable.Attributes.DataContract;

/// <summary>
/// Marks the class to source generate overrides for Equals and GetHashCode,
/// including only properties decorated with <see cref="System.Runtime.Serialization.DataMemberAttribute"/>
/// and excluding properties decorated with <see cref="System.Runtime.Serialization.IgnoreDataMemberAttribute"/>.
/// </summary>
[Conditional("EQUATABLE_GENERATOR")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class DataContractEquatableAttribute : Attribute;
