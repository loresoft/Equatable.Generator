using Equatable.Entities;

namespace Equatable.Generator.Tests.Properties;

/// <summary>
/// Property-based tests for [DataContractEquatable]:
/// only [DataMember] properties participate in equality.
/// </summary>
public class OrderDataContractProperties
{
    [Property]
    public Property Reflexivity(int id, string? name)
    {
        var o = new OrderDataContract { Id = id, Name = name };
        return o.Equals(o).ToProperty();
    }

    [Property]
    public Property Symmetry(int id1, string? name1, int id2, string? name2)
    {
        var a = new OrderDataContract { Id = id1, Name = name1 };
        var b = new OrderDataContract { Id = id2, Name = name2 };
        return (a.Equals(b) == b.Equals(a)).ToProperty();
    }

    [Property]
    public Property EqualImpliesSameHashCode(int id, string? name)
    {
        var a = new OrderDataContract { Id = id, Name = name };
        var b = new OrderDataContract { Id = id, Name = name };
        return (a.Equals(b) && a.GetHashCode() == b.GetHashCode()).ToProperty();
    }

    [Property]
    public Property NonDataMemberFieldsIgnored(int id, string? name, string? note1, string? note2, string? ignored1, string? ignored2)
    {
        // InternalNote (no [DataMember]) and IgnoredField ([IgnoreDataMember]) must not affect equality
        var a = new OrderDataContract { Id = id, Name = name, InternalNote = note1, IgnoredField = ignored1 };
        var b = new OrderDataContract { Id = id, Name = name, InternalNote = note2, IgnoredField = ignored2 };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property NonDataMemberFieldsIgnoredInHashCode(int id, string? name, string? note1, string? note2)
    {
        var a = new OrderDataContract { Id = id, Name = name, InternalNote = note1 };
        var b = new OrderDataContract { Id = id, Name = name, InternalNote = note2 };
        return (a.GetHashCode() == b.GetHashCode()).ToProperty();
    }

    [Property]
    public Property DifferentIdNotEqual(string? name, int id1, int id2)
    {
        if (id1 == id2)
            return true.ToProperty().When(true);

        var a = new OrderDataContract { Id = id1, Name = name };
        var b = new OrderDataContract { Id = id2, Name = name };
        return (!a.Equals(b)).ToProperty();
    }

    [Property]
    public Property DifferentNameNotEqual(int id, string name1, string name2)
    {
        if (name1 == name2)
            return true.ToProperty().When(true);

        var a = new OrderDataContract { Id = id, Name = name1 };
        var b = new OrderDataContract { Id = id, Name = name2 };
        return (!a.Equals(b)).ToProperty();
    }
}
