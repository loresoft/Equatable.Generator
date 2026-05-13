using Equatable.Comparers;

namespace Equatable.Generator.Tests.Comparers;

/// <summary>
/// Tests for nested collection comparisons using explicitly composed comparers.
///
/// DictionaryEqualityComparer.Default uses EqualityComparer&lt;TValue&gt;.Default for values,
/// which is reference equality for any collection type. To compare dictionaries whose
/// values are themselves collections structurally, pass an explicit valueComparer:
///
///   new DictionaryEqualityComparer&lt;string, Dictionary&lt;string,int&gt;&gt;(
///       EqualityComparer&lt;string&gt;.Default,
///       DictionaryEqualityComparer&lt;string,int&gt;.Default)
/// </summary>
public class NestedDictionaryEqualityComparerTest
{
    // ── Dict<K, Dict<K2, V>> ─────────────────────────────────────────────────────────────────────

    private static readonly DictionaryEqualityComparer<string, Dictionary<string, int>> NestedDictComparer =
        new(EqualityComparer<string>.Default,
            new DictionaryEqualityComparer<string, int>());

    [Fact]
    public void NestedDict_EqualContent_ReturnsTrue()
    {
        var a = new Dictionary<string, Dictionary<string, int>>
        {
            ["x"] = new() { ["a"] = 1, ["b"] = 2 },
            ["y"] = new() { ["c"] = 3 },
        };
        var b = new Dictionary<string, Dictionary<string, int>>
        {
            ["x"] = new() { ["a"] = 1, ["b"] = 2 },
            ["y"] = new() { ["c"] = 3 },
        };

        Assert.True(NestedDictComparer.Equals(a, b));
    }

    [Fact]
    public void NestedDict_DifferentInnerValue_ReturnsFalse()
    {
        var a = new Dictionary<string, Dictionary<string, int>>
        {
            ["x"] = new() { ["a"] = 1 },
        };
        var b = new Dictionary<string, Dictionary<string, int>>
        {
            ["x"] = new() { ["a"] = 99 },
        };

        Assert.False(NestedDictComparer.Equals(a, b));
    }

    [Fact]
    public void NestedDict_InnerInsertionOrderIndependent_ReturnsTrue()
    {
        var a = new Dictionary<string, Dictionary<string, int>>
        {
            ["x"] = new() { ["a"] = 1, ["b"] = 2 },
        };
        var b = new Dictionary<string, Dictionary<string, int>>
        {
            ["x"] = new() { ["b"] = 2, ["a"] = 1 },
        };

        Assert.True(NestedDictComparer.Equals(a, b));
    }

    [Fact]
    public void NestedDict_OuterInsertionOrderIndependent_ReturnsTrue()
    {
        var a = new Dictionary<string, Dictionary<string, int>>
        {
            ["x"] = new() { ["a"] = 1 },
            ["y"] = new() { ["b"] = 2 },
        };
        var b = new Dictionary<string, Dictionary<string, int>>
        {
            ["y"] = new() { ["b"] = 2 },
            ["x"] = new() { ["a"] = 1 },
        };

        Assert.True(NestedDictComparer.Equals(a, b));
    }

    [Fact]
    public void NestedDict_EqualContent_SameHashCode()
    {
        var a = new Dictionary<string, Dictionary<string, int>>
        {
            ["x"] = new() { ["a"] = 1, ["b"] = 2 },
            ["y"] = new() { ["c"] = 3 },
        };
        var b = new Dictionary<string, Dictionary<string, int>>
        {
            ["y"] = new() { ["c"] = 3 },
            ["x"] = new() { ["b"] = 2, ["a"] = 1 },
        };

        Assert.True(NestedDictComparer.Equals(a, b));
        Assert.Equal(NestedDictComparer.GetHashCode(a), NestedDictComparer.GetHashCode(b));
    }

    [Fact]
    public void NestedDict_NullValue_EqualsBothNull_ReturnsTrue()
    {
        // DictionaryEqualityComparer.Default uses EqualityComparer<TValue>.Default.
        // EqualityComparer<T>.Default.Equals(null, null) == true, so two entries with
        // null values compare equal even without a custom inner comparer.
        var comparer = DictionaryEqualityComparer<string, Dictionary<string, int>?>.Default;

        var a = new Dictionary<string, Dictionary<string, int>?> { ["x"] = null };
        var b = new Dictionary<string, Dictionary<string, int>?> { ["x"] = null };

        Assert.True(comparer.Equals(a, b));
    }

    [Fact]
    public void NestedDict_NullValueVsEmpty_ReturnsFalse()
    {
        // null and empty dict are different values, so entries with null vs [] are not equal.
        var comparer = DictionaryEqualityComparer<string, Dictionary<string, int>?>.Default;

        var a = new Dictionary<string, Dictionary<string, int>?> { ["x"] = null };
        var b = new Dictionary<string, Dictionary<string, int>?> { ["x"] = [] };

        Assert.False(comparer.Equals(a, b));
    }

    // ── Dict<K, List<V>> ─────────────────────────────────────────────────────────────────────────

    private static readonly DictionaryEqualityComparer<string, List<int>> DictOfListComparer =
        new(EqualityComparer<string>.Default,
            new SequenceEqualityComparer<int>());

    [Fact]
    public void DictOfList_EqualContent_ReturnsTrue()
    {
        var a = new Dictionary<string, List<int>> { ["a"] = [1, 2, 3], ["b"] = [4] };
        var b = new Dictionary<string, List<int>> { ["a"] = [1, 2, 3], ["b"] = [4] };

        Assert.True(DictOfListComparer.Equals(a, b));
    }

    [Fact]
    public void DictOfList_InnerOrderMatters_ReturnsFalse()
    {
        var a = new Dictionary<string, List<int>> { ["a"] = [1, 2] };
        var b = new Dictionary<string, List<int>> { ["a"] = [2, 1] };

        Assert.False(DictOfListComparer.Equals(a, b));
    }

    [Fact]
    public void DictOfList_EqualContent_SameHashCode()
    {
        var a = new Dictionary<string, List<int>> { ["a"] = [1, 2], ["b"] = [3] };
        var b = new Dictionary<string, List<int>> { ["b"] = [3], ["a"] = [1, 2] };

        Assert.True(DictOfListComparer.Equals(a, b));
        Assert.Equal(DictOfListComparer.GetHashCode(a), DictOfListComparer.GetHashCode(b));
    }

    // ── Dict<K, HashSet<V>> ──────────────────────────────────────────────────────────────────────

    private static readonly DictionaryEqualityComparer<string, HashSet<int>> DictOfSetComparer =
        new(EqualityComparer<string>.Default,
            new HashSetEqualityComparer<int>());

    [Fact]
    public void DictOfSet_EqualContent_ReturnsTrue()
    {
        var a = new Dictionary<string, HashSet<int>> { ["a"] = [1, 2], ["b"] = [3] };
        var b = new Dictionary<string, HashSet<int>> { ["a"] = [1, 2], ["b"] = [3] };

        Assert.True(DictOfSetComparer.Equals(a, b));
    }

    [Fact]
    public void DictOfSet_InnerSetOrderIndependent_ReturnsTrue()
    {
        var a = new Dictionary<string, HashSet<int>> { ["a"] = [1, 2] };
        var b = new Dictionary<string, HashSet<int>> { ["a"] = [2, 1] };

        Assert.True(DictOfSetComparer.Equals(a, b));
    }

    [Fact]
    public void DictOfSet_EqualContent_SameHashCode()
    {
        var a = new Dictionary<string, HashSet<int>> { ["a"] = [1, 2], ["b"] = [3] };
        var b = new Dictionary<string, HashSet<int>> { ["b"] = [3], ["a"] = [2, 1] };

        Assert.True(DictOfSetComparer.Equals(a, b));
        Assert.Equal(DictOfSetComparer.GetHashCode(a), DictOfSetComparer.GetHashCode(b));
    }

    // ── Dict<K, Dict<K2, List<V>>> (3-level) ────────────────────────────────────────────────────

    private static readonly DictionaryEqualityComparer<string, Dictionary<string, List<int>>> ThreeLevelComparer =
        new(EqualityComparer<string>.Default,
            new DictionaryEqualityComparer<string, List<int>>(
                EqualityComparer<string>.Default,
                new SequenceEqualityComparer<int>()));

    [Fact]
    public void ThreeLevel_EqualContent_ReturnsTrue()
    {
        var a = new Dictionary<string, Dictionary<string, List<int>>>
        {
            ["outer"] = new() { ["inner"] = [1, 2, 3] },
        };
        var b = new Dictionary<string, Dictionary<string, List<int>>>
        {
            ["outer"] = new() { ["inner"] = [1, 2, 3] },
        };

        Assert.True(ThreeLevelComparer.Equals(a, b));
    }

    [Fact]
    public void ThreeLevel_DifferentLeafValue_ReturnsFalse()
    {
        var a = new Dictionary<string, Dictionary<string, List<int>>>
        {
            ["outer"] = new() { ["inner"] = [1, 2, 3] },
        };
        var b = new Dictionary<string, Dictionary<string, List<int>>>
        {
            ["outer"] = new() { ["inner"] = [1, 2, 99] },
        };

        Assert.False(ThreeLevelComparer.Equals(a, b));
    }

    [Fact]
    public void ThreeLevel_InsertionOrderIndependent_EqualContent_SameHashCode()
    {
        var a = new Dictionary<string, Dictionary<string, List<int>>>
        {
            ["x"] = new() { ["p"] = [10, 20], ["q"] = [30] },
            ["y"] = new() { ["r"] = [40] },
        };
        var b = new Dictionary<string, Dictionary<string, List<int>>>
        {
            ["y"] = new() { ["r"] = [40] },
            ["x"] = new() { ["q"] = [30], ["p"] = [10, 20] },
        };

        Assert.True(ThreeLevelComparer.Equals(a, b));
        Assert.Equal(ThreeLevelComparer.GetHashCode(a), ThreeLevelComparer.GetHashCode(b));
    }

    // ── ReadOnlyDictionary variants ──────────────────────────────────────────────────────────────

    private static readonly ReadOnlyDictionaryEqualityComparer<string, IReadOnlyList<int>> ReadOnlyNestedComparer =
        new(EqualityComparer<string>.Default,
            new SequenceEqualityComparer<int>());

    [Fact]
    public void ReadOnlyNestedDict_EqualContent_ReturnsTrue()
    {
        IReadOnlyDictionary<string, IReadOnlyList<int>> a = new Dictionary<string, IReadOnlyList<int>>
        {
            ["a"] = [1, 2],
            ["b"] = [3],
        };
        IReadOnlyDictionary<string, IReadOnlyList<int>> b = new Dictionary<string, IReadOnlyList<int>>
        {
            ["b"] = [3],
            ["a"] = [1, 2],
        };

        Assert.True(ReadOnlyNestedComparer.Equals(a, b));
        Assert.Equal(ReadOnlyNestedComparer.GetHashCode(a), ReadOnlyNestedComparer.GetHashCode(b));
    }
}
