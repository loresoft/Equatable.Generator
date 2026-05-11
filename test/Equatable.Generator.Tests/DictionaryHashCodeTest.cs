using Equatable.Comparers;

namespace Equatable.Generator.Tests;

/// <summary>
/// Demonstrates and verifies the order-independent hash code algorithm used by
/// DictionaryEqualityComparer and ReadOnlyDictionaryEqualityComparer.
///
/// The implementation sums HashCode.Combine(key, value) over every entry.
/// Addition is commutative, so the total is the same regardless of insertion order:
///
///   hash({a→1, b→2}) = Combine("a",1) + Combine("b",2)
///                    = Combine("b",2) + Combine("a",1)
///                    = hash({b→2, a→1})
/// </summary>
public class DictionaryHashCodeTest
{
    private static readonly DictionaryEqualityComparer<string, int> DictComparer
        = DictionaryEqualityComparer<string, int>.Default;

    private static readonly ReadOnlyDictionaryEqualityComparer<string, int> ReadOnlyComparer
        = ReadOnlyDictionaryEqualityComparer<string, int>.Default;

    [Fact]
    public void HashCode_IsInsertionOrderIndependent()
    {
        // same key-value pairs, different insertion order
        var first  = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };
        var second = new Dictionary<string, int> { ["c"] = 3, ["a"] = 1, ["b"] = 2 };

        Assert.Equal(DictComparer.GetHashCode(first), DictComparer.GetHashCode(second));
    }

    [Fact]
    public void HashCode_DifferentValueProducesDifferentHash()
    {
        var first  = new Dictionary<string, int> { ["a"] = 1 };
        var second = new Dictionary<string, int> { ["a"] = 2 };

        // different values → different Combine(key,value) → different sum
        Assert.NotEqual(DictComparer.GetHashCode(first), DictComparer.GetHashCode(second));
    }

    [Fact]
    public void HashCode_DifferentKeyProducesDifferentHash()
    {
        var first  = new Dictionary<string, int> { ["a"] = 1 };
        var second = new Dictionary<string, int> { ["b"] = 1 };

        Assert.NotEqual(DictComparer.GetHashCode(first), DictComparer.GetHashCode(second));
    }

    [Fact]
    public void ReadOnly_HashCode_IsInsertionOrderIndependent()
    {
        IReadOnlyDictionary<string, int> first  = new Dictionary<string, int> { ["x"] = 10, ["y"] = 20 };
        IReadOnlyDictionary<string, int> second = new Dictionary<string, int> { ["y"] = 20, ["x"] = 10 };

        Assert.Equal(ReadOnlyComparer.GetHashCode(first), ReadOnlyComparer.GetHashCode(second));
    }

    [Fact]
    public void EqualDictionaries_HaveSameHashCode()
    {
        // two separately constructed dictionaries with identical content
        var first  = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var second = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };

        Assert.True(DictComparer.Equals(first, second));
        Assert.Equal(DictComparer.GetHashCode(first), DictComparer.GetHashCode(second));
    }
}
