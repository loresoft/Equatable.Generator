using System.Collections.Generic;
using Equatable.Entities;

namespace Equatable.Generator.Tests.Properties;

/// <summary>
/// Property-based tests for [DataContractEquatable] with inferred HashSet comparers:
/// HashSet<T> and IReadOnlySet<T> without explicit attributes must use set equality (order-insensitive).
/// </summary>
public class OrderDataContractWithHashSetProperties
{
    [Property]
    public Property Reflexivity(int id, string[]? tags, int[]? codes)
    {
        var o = Make(id, tags, codes);
        return o.Equals(o).ToProperty();
    }

    [Property]
    public Property Symmetry(int id1, string[]? tags1, int[]? codes1, int id2, string[]? tags2, int[]? codes2)
    {
        var a = Make(id1, tags1, codes1);
        var b = Make(id2, tags2, codes2);
        return (a.Equals(b) == b.Equals(a)).ToProperty();
    }

    [Property]
    public Property EqualImpliesSameHashCode(int id, string[]? tags, int[]? codes)
    {
        var a = Make(id, tags, codes);
        var b = Make(id, tags, codes);
        return (a.Equals(b) && a.GetHashCode() == b.GetHashCode()).ToProperty();
    }

    [Property]
    public Property SameElementsDifferentOrderAreEqual(int id, string[] tags, int[] codes)
    {
        // Reversing the arrays gives different insertion order but same set membership
        var a = Make(id, tags, codes);
        var b = Make(id, [.. System.Linq.Enumerable.Reverse(tags)], [.. System.Linq.Enumerable.Reverse(codes)]);
        return (a.Equals(b) && a.GetHashCode() == b.GetHashCode()).ToProperty();
    }

    [Property]
    public Property DifferentIdNotEqual(string[]? tags, int[]? codes, int id1, int id2)
    {
        if (id1 == id2)
            return true.ToProperty().When(false);

        var a = Make(id1, tags, codes);
        var b = Make(id2, tags, codes);
        return (!a.Equals(b)).ToProperty();
    }

    [Property]
    public Property NullHashSetEqualsNullHashSet(int id)
    {
        var a = Make(id, null, null);
        var b = Make(id, null, null);
        return a.Equals(b).ToProperty();
    }

    private static OrderDataContractWithHashSet Make(int id, string[]? tags, int[]? codes) =>
        new()
        {
            Id = id,
            Tags = tags is null ? null : new HashSet<string>(tags),
            Codes = codes is null ? null : new HashSet<int>(codes),
        };
}
