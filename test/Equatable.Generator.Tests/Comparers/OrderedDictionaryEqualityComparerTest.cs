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


    // ── custom IEqualityComparer+IComparer: StringComparer.OrdinalIgnoreCase ──────────────────────
    // Demonstrates why sequential: true exists. StringComparer implements both interfaces, so
    // the same comparer drives key equality AND sort order — hash is insertion-order independent
    // even when the dictionary uses a non-default key comparer.

    private static OrderedDictionaryEqualityComparer<string, int> CaseInsensitive()
        => new(StringComparer.OrdinalIgnoreCase, EqualityComparer<int>.Default);

    [Fact]
    public void CustomComparer_CaseInsensitiveKeys_Equal()
    {
        // "West" and "WEST" are the same key under OrdinalIgnoreCase
        var a = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["West"] = 42, ["east"] = 17 };
        var b = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["WEST"] = 42, ["EAST"] = 17 };

        Assert.True(CaseInsensitive().Equals(a, b));
    }

    [Fact]
    public void CustomComparer_DifferentInsertionOrder_SameHashCode()
    {
        // Same pairs, different insertion order — hash must match because OrdinalIgnoreCase
        // drives both equality and sort order, making the result fully deterministic.
        var a = new Dictionary<string, int> { ["West"] = 42, ["east"] = 17, ["NORTH"] = 99 };
        var b = new Dictionary<string, int> { ["NORTH"] = 99, ["West"] = 42, ["east"] = 17 };

        Assert.Equal(CaseInsensitive().GetHashCode(a), CaseInsensitive().GetHashCode(b));
    }

    [Fact]
    public void CustomComparer_GetHashCode_CaseVariantsProduceSameHash()
    {
        // "West"→42 and "WEST"→42 are the same entry under OrdinalIgnoreCase — same hash
        var a = new Dictionary<string, int> { ["West"] = 42 };
        var b = new Dictionary<string, int> { ["WEST"] = 42 };

        Assert.Equal(CaseInsensitive().GetHashCode(a), CaseInsensitive().GetHashCode(b));
    }

    [Fact]
    public void DefaultComparer_CaseVariants_NotEqual()
    {
        // Contrast: with default (ordinal) comparer, "West" != "WEST" — different keys
        var a = new Dictionary<string, int> { ["West"] = 42 };
        var b = new Dictionary<string, int> { ["WEST"] = 42 };

        Assert.False(Comparer.Equals(a, b));
        Assert.NotEqual(Comparer.GetHashCode(a), Comparer.GetHashCode(b));
    }

    // ────────────────────────────────────────────────────────────────────────────────────────────

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
