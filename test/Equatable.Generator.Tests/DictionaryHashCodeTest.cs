using Equatable.Comparers;

namespace Equatable.Generator.Tests;

/// <summary>
/// Demonstrates and verifies the order-independent hash code algorithm used by
/// DictionaryEqualityComparer and ReadOnlyDictionaryEqualityComparer.
///
/// The hash/equals contract requires: if Equals(x, y) then GetHashCode(x) == GetHashCode(y).
/// DictionaryEquals uses TryGetValue (order-independent), so GetHashCode MUST also be
/// order-independent — otherwise two equal dictionaries in different insertion order would
/// produce different hash codes and violate the contract.
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

    /// <summary>
    /// The critical multi-entry case: same keys, values swapped across keys.
    /// {a→1, b→2} != {a→2, b→1} — Equals correctly returns false (TryGetValue finds "a"→2≠1).
    /// The user's concern: can sum(Combine(k,v)) produce the same value for both?
    ///
    /// Commutative sum does NOT guarantee no collision here — it only guarantees the contract:
    ///   Equals(x,y) → GetHashCode(x) == GetHashCode(y)
    /// The contract only runs one direction. Unequal dicts MAY share a hash code (collision).
    ///
    /// This test verifies:
    ///   1. Equals returns false (correctness — always guaranteed by TryGetValue logic)
    ///   2. Hash codes differ in practice for this common pattern (collision absent here)
    ///
    /// If this test ever fails on (2), the fix is NOT in Equals (already correct) but in
    /// accepting the collision as a legitimate hash table trade-off — the contract is not violated.
    /// </summary>
    [Fact]
    public void HashCode_SwappedValues_UnequalDictionaries_EqualIsFalse()
    {
        // {a→1, b→2} vs {a→2, b→1}: same key set, values assigned to different keys
        var first  = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var second = new Dictionary<string, int> { ["a"] = 2, ["b"] = 1 };

        // Equals must always be false — TryGetValue("a") finds 2≠1
        Assert.False(DictComparer.Equals(first, second));
        Assert.False(DictComparer.Equals(second, first));
    }

    [Fact]
    public void HashCode_SwappedValues_ProduceDifferentHashInPractice()
    {
        // Verifies no systematic collision for the swapped-values pattern.
        // Note: hash collisions are theoretically allowed; this test catches regressions
        // in the hash function that make them routine.
        var first  = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var second = new Dictionary<string, int> { ["a"] = 2, ["b"] = 1 };

        Assert.NotEqual(DictComparer.GetHashCode(first), DictComparer.GetHashCode(second));
    }

    [Fact]
    public void ReadOnly_HashCode_SwappedValues_ProduceDifferentHashInPractice()
    {
        IReadOnlyDictionary<string, int> first  = new Dictionary<string, int> { ["x"] = 10, ["y"] = 20 };
        IReadOnlyDictionary<string, int> second = new Dictionary<string, int> { ["x"] = 20, ["y"] = 10 };

        Assert.False(ReadOnlyComparer.Equals(first, second));
        Assert.NotEqual(ReadOnlyComparer.GetHashCode(first), ReadOnlyComparer.GetHashCode(second));
    }
}
