using System.Diagnostics;

namespace Equatable.Attributes;

[Conditional("EQUATABLE_GENERATOR")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class EquatableAttribute : Attribute
{
    /// <summary>
    /// Only members marked with equality attributes will be generated for Equal and GetHashCode.
    /// </summary>
    public bool Explicit { get; set; }

    /// <summary>
    /// Equal and GetHashCode generation do not consider members of base classes.
    /// </summary>
    public bool IgnoreInherited { get; set; }
}
