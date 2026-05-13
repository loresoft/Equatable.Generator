using System.Collections.Generic;
using Equatable.Comparers;

namespace Equatable.Generator.Tests.Comparers;

public class OrderedDictionaryEqualityComparerTest
{
    // ── nested: OrderedDictionaryEqualityComparer<string, IDictionary<string,int>> ──
    // Verifies that when the value comparer is itself an OrderedDictionaryEqualityComparer,
    // inner-dict insertion order is also irrelevant (both levels sorted by key).

    private static OrderedDictionaryEqualityComparer<string, IDictionary<string, int>> NestedOrdered()
        => new(EqualityComparer<string>.Default,
               new OrderedDictionaryEqualityComparer<string, int>());

    [Fact]
    public void Nested_OrderedBothLevels_InnerInsertionOrderDiffers_ReturnsTrue()
    {
        var inner1 = new Dictionary<string, int> { ["x"] = 1, ["y"] = 2 };
        var inner2 = new Dictionary<string, int> { ["y"] = 2, ["x"] = 1 };
        var a = new Dictionary<string, IDictionary<string, int>> { ["k"] = inner1 };
        var b = new Dictionary<string, IDictionary<string, int>> { ["k"] = inner2 };

        Assert.True(NestedOrdered().Equals(a, b));
    }

    [Fact]
    public void Nested_OrderedBothLevels_OuterInsertionOrderDiffers_ReturnsTrue()
    {
        var a = new Dictionary<string, IDictionary<string, int>>
        {
            ["a"] = new Dictionary<string, int> { ["x"] = 1 },
            ["b"] = new Dictionary<string, int> { ["y"] = 2 },
        };
        var b = new Dictionary<string, IDictionary<string, int>>
        {
            ["b"] = new Dictionary<string, int> { ["y"] = 2 },
            ["a"] = new Dictionary<string, int> { ["x"] = 1 },
        };

        Assert.True(NestedOrdered().Equals(a, b));
    }

    [Fact]
    public void Nested_OrderedBothLevels_InnerValueDiffers_ReturnsFalse()
    {
        var a = new Dictionary<string, IDictionary<string, int>> { ["k"] = new Dictionary<string, int> { ["x"] = 1 } };
        var b = new Dictionary<string, IDictionary<string, int>> { ["k"] = new Dictionary<string, int> { ["x"] = 99 } };

        Assert.False(NestedOrdered().Equals(a, b));
    }

    [Fact]
    public void Nested_OrderedBothLevels_SamePairs_GetHashCodesEqual()
    {
        var inner1 = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var inner2 = new Dictionary<string, int> { ["b"] = 2, ["a"] = 1 };
        var a = new Dictionary<string, IDictionary<string, int>> { ["k"] = inner1 };
        var b = new Dictionary<string, IDictionary<string, int>> { ["k"] = inner2 };

        var cmp = NestedOrdered();
        Assert.Equal(cmp.GetHashCode(a), cmp.GetHashCode(b));
    }

    // ── nested with unordered inner: verify behaviour when inner is DictionaryEqualityComparer ──

    private static OrderedDictionaryEqualityComparer<string, IDictionary<string, int>> NestedUnordered()
        => new(EqualityComparer<string>.Default,
               new DictionaryEqualityComparer<string, int>());

    [Fact]
    public void Nested_OrderedOuter_UnorderedInner_InnerInsertionOrderDiffers_ReturnsTrue()
    {
        // Inner uses DictionaryEqualityComparer (unordered) → inner insertion order irrelevant.
        var a = new Dictionary<string, IDictionary<string, int>> { ["k"] = new Dictionary<string, int> { ["x"] = 1, ["y"] = 2 } };
        var b = new Dictionary<string, IDictionary<string, int>> { ["k"] = new Dictionary<string, int> { ["y"] = 2, ["x"] = 1 } };

        Assert.True(NestedUnordered().Equals(a, b));
    }


    private static readonly OrderedDictionaryEqualityComparer<string, int> Comparer
        = OrderedDictionaryEqualityComparer<string, int>.Default;

    private static readonly OrderedReadOnlyDictionaryEqualityComparer<string, int> ReadOnlyComparer
        = OrderedReadOnlyDictionaryEqualityComparer<string, int>.Default;

    [Fact]
    public void Equals_SamePairs_DifferentInsertionOrder_ReturnsTrue()
    {
        var a = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var b = new Dictionary<string, int> { ["b"] = 2, ["a"] = 1 };

        Assert.True(Comparer.Equals(a, b));
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var a = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var b = new Dictionary<string, int> { ["a"] = 1, ["b"] = 99 };

        Assert.False(Comparer.Equals(a, b));
    }

    [Fact]
    public void Equals_DifferentKeys_ReturnsFalse()
    {
        var a = new Dictionary<string, int> { ["a"] = 1 };
        var b = new Dictionary<string, int> { ["z"] = 1 };

        Assert.False(Comparer.Equals(a, b));
    }

    [Fact]
    public void Equals_NullBoth_ReturnsTrue()
    {
        Assert.True(Comparer.Equals(null, null));
    }

    [Fact]
    public void Equals_NullOne_ReturnsFalse()
    {
        var a = new Dictionary<string, int> { ["a"] = 1 };
        Assert.False(Comparer.Equals(a, null));
        Assert.False(Comparer.Equals(null, a));
    }

    [Fact]
    public void GetHashCode_SamePairs_DifferentInsertionOrder_Equal()
    {
        var a = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };
        var b = new Dictionary<string, int> { ["c"] = 3, ["a"] = 1, ["b"] = 2 };

        Assert.Equal(Comparer.GetHashCode(a), Comparer.GetHashCode(b));
    }

    [Fact]
    public void GetHashCode_DifferentValues_NotEqual()
    {
        var a = new Dictionary<string, int> { ["a"] = 1 };
        var b = new Dictionary<string, int> { ["a"] = 2 };

        Assert.NotEqual(Comparer.GetHashCode(a), Comparer.GetHashCode(b));
    }

    [Fact]
    public void EqualDictionaries_HaveSameHashCode()
    {
        var a = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var b = new Dictionary<string, int> { ["b"] = 2, ["a"] = 1 };

        Assert.True(Comparer.Equals(a, b));
        Assert.Equal(Comparer.GetHashCode(a), Comparer.GetHashCode(b));
    }

    [Fact]
    public void ReadOnly_Equals_SamePairs_DifferentInsertionOrder_ReturnsTrue()
    {
        IReadOnlyDictionary<string, int> a = new Dictionary<string, int> { ["x"] = 10, ["y"] = 20 };
        IReadOnlyDictionary<string, int> b = new Dictionary<string, int> { ["y"] = 20, ["x"] = 10 };

        Assert.True(ReadOnlyComparer.Equals(a, b));
    }

    [Fact]
    public void ReadOnly_GetHashCode_SamePairs_DifferentInsertionOrder_Equal()
    {
        IReadOnlyDictionary<string, int> a = new Dictionary<string, int> { ["x"] = 10, ["y"] = 20 };
        IReadOnlyDictionary<string, int> b = new Dictionary<string, int> { ["y"] = 20, ["x"] = 10 };

        Assert.Equal(ReadOnlyComparer.GetHashCode(a), ReadOnlyComparer.GetHashCode(b));
    }
}
