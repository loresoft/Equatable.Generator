using Equatable.Comparers;

namespace Equatable.Generator.Tests.Comparers;

/// <summary>
/// Verifies the GetHashCode contract for all collection comparers:
///   1. null  → 0
///   2. empty → non-zero (must differ from null)
///   3. equal collections → same hash code
///   4. unequal collections → different hash code (best-effort; not a contract but expected for these cases)
///   5. hash code is consistent with Equals (if Equals returns true, GetHashCode must match)
/// </summary>
public class ComparerGetHashCodeTest
{
    // ── DictionaryEqualityComparer ───────────────────────────────────────────────────────────────

    private static readonly DictionaryEqualityComparer<string, int> DictComparer
        = DictionaryEqualityComparer<string, int>.Default;

    [Fact]
    public void Dictionary_Null_HashIsZero()
    {
        Assert.Equal(0, DictComparer.GetHashCode(null!));
    }

    [Fact]
    public void Dictionary_Empty_HashIsNotZero()
    {
        Assert.NotEqual(0, DictComparer.GetHashCode(new Dictionary<string, int>()));
    }

    [Fact]
    public void Dictionary_EmptyAndNull_HashDiffers()
    {
        Assert.NotEqual(
            DictComparer.GetHashCode(null!),
            DictComparer.GetHashCode(new Dictionary<string, int>()));
    }

    [Fact]
    public void Dictionary_EqualCollections_SameHash()
    {
        var a = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var b = new Dictionary<string, int> { ["b"] = 2, ["a"] = 1 };

        Assert.True(DictComparer.Equals(a, b));
        Assert.Equal(DictComparer.GetHashCode(a), DictComparer.GetHashCode(b));
    }

    [Fact]
    public void Dictionary_DifferentValues_DifferentHash()
    {
        var a = new Dictionary<string, int> { ["a"] = 1 };
        var b = new Dictionary<string, int> { ["a"] = 2 };

        Assert.NotEqual(DictComparer.GetHashCode(a), DictComparer.GetHashCode(b));
    }

    // ── ReadOnlyDictionaryEqualityComparer ───────────────────────────────────────────────────────

    private static readonly ReadOnlyDictionaryEqualityComparer<string, int> ReadOnlyDictComparer
        = ReadOnlyDictionaryEqualityComparer<string, int>.Default;

    [Fact]
    public void ReadOnlyDictionary_Null_HashIsZero()
    {
        Assert.Equal(0, ReadOnlyDictComparer.GetHashCode(null!));
    }

    [Fact]
    public void ReadOnlyDictionary_Empty_HashIsNotZero()
    {
        Assert.NotEqual(0, ReadOnlyDictComparer.GetHashCode(new Dictionary<string, int>()));
    }

    [Fact]
    public void ReadOnlyDictionary_EmptyAndNull_HashDiffers()
    {
        Assert.NotEqual(
            ReadOnlyDictComparer.GetHashCode(null!),
            ReadOnlyDictComparer.GetHashCode(new Dictionary<string, int>()));
    }

    [Fact]
    public void ReadOnlyDictionary_EqualCollections_SameHash()
    {
        IReadOnlyDictionary<string, int> a = new Dictionary<string, int> { ["x"] = 10, ["y"] = 20 };
        IReadOnlyDictionary<string, int> b = new Dictionary<string, int> { ["y"] = 20, ["x"] = 10 };

        Assert.True(ReadOnlyDictComparer.Equals(a, b));
        Assert.Equal(ReadOnlyDictComparer.GetHashCode(a), ReadOnlyDictComparer.GetHashCode(b));
    }

    // ── HashSetEqualityComparer ──────────────────────────────────────────────────────────────────

    private static readonly HashSetEqualityComparer<int> SetComparer
        = HashSetEqualityComparer<int>.Default;

    [Fact]
    public void HashSet_Null_HashIsZero()
    {
        Assert.Equal(0, SetComparer.GetHashCode(null!));
    }

    [Fact]
    public void HashSet_Empty_HashIsNotZero()
    {
        Assert.NotEqual(0, SetComparer.GetHashCode(new HashSet<int>()));
    }

    [Fact]
    public void HashSet_EmptyAndNull_HashDiffers()
    {
        Assert.NotEqual(
            SetComparer.GetHashCode(null!),
            SetComparer.GetHashCode(new HashSet<int>()));
    }

    [Fact]
    public void HashSet_EqualCollections_SameHash()
    {
        var a = new HashSet<int> { 1, 2, 3 };
        var b = new HashSet<int> { 3, 1, 2 };

        Assert.True(SetComparer.Equals(a, b));
        Assert.Equal(SetComparer.GetHashCode(a), SetComparer.GetHashCode(b));
    }

    [Fact]
    public void HashSet_DifferentElements_DifferentHash()
    {
        var a = new HashSet<int> { 1, 2 };
        var b = new HashSet<int> { 1, 3 };

        Assert.NotEqual(SetComparer.GetHashCode(a), SetComparer.GetHashCode(b));
    }

    [Fact]
    public void HashSet_SameElementsDifferentCount_DifferentHash()
    {
        // {1, 2} vs {1} — different sets, should produce different hashes
        var a = new HashSet<int> { 1, 2 };
        var b = new HashSet<int> { 1 };

        Assert.NotEqual(SetComparer.GetHashCode(a), SetComparer.GetHashCode(b));
    }

    // ── SequenceEqualityComparer ─────────────────────────────────────────────────────────────────

    private static readonly SequenceEqualityComparer<int> SeqComparer
        = SequenceEqualityComparer<int>.Default;

    [Fact]
    public void Sequence_Null_HashIsZero()
    {
        Assert.Equal(0, SeqComparer.GetHashCode(null!));
    }

    [Fact]
    public void Sequence_Empty_HashDiffersFromNull()
    {
        Assert.NotEqual(SeqComparer.GetHashCode(null!), SeqComparer.GetHashCode(new List<int>()));
    }

    [Fact]
    public void Sequence_EqualCollections_SameHash()
    {
        var a = new List<int> { 1, 2, 3 };
        var b = new List<int> { 1, 2, 3 };

        Assert.True(SeqComparer.Equals(a, b));
        Assert.Equal(SeqComparer.GetHashCode(a), SeqComparer.GetHashCode(b));
    }

    [Fact]
    public void Sequence_DifferentOrder_DifferentHash()
    {
        // SequenceEqualityComparer is order-sensitive — reversed order must produce a different hash
        var a = new List<int> { 1, 2 };
        var b = new List<int> { 2, 1 };

        Assert.False(SeqComparer.Equals(a, b));
        Assert.NotEqual(SeqComparer.GetHashCode(a), SeqComparer.GetHashCode(b));
    }

    // ── OrderedDictionaryEqualityComparer ────────────────────────────────────────────────────────

    private static readonly OrderedDictionaryEqualityComparer<string, int> OrderedDictComparer
        = OrderedDictionaryEqualityComparer<string, int>.Default;

    [Fact]
    public void OrderedDictionary_Null_HashIsZero()
    {
        Assert.Equal(0, OrderedDictComparer.GetHashCode(null!));
    }

    [Fact]
    public void OrderedDictionary_Empty_HashDiffersFromNull()
    {
        Assert.NotEqual(
            OrderedDictComparer.GetHashCode(null!),
            OrderedDictComparer.GetHashCode(new Dictionary<string, int>()));
    }

    [Fact]
    public void OrderedDictionary_EqualCollections_SameHash()
    {
        // Ordered comparer is key-sorted — same content, different insertion order → equal
        var a = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };
        var b = new Dictionary<string, int> { ["c"] = 3, ["a"] = 1, ["b"] = 2 };

        Assert.True(OrderedDictComparer.Equals(a, b));
        Assert.Equal(OrderedDictComparer.GetHashCode(a), OrderedDictComparer.GetHashCode(b));
    }

    [Fact]
    public void OrderedDictionary_DifferentValues_DifferentHash()
    {
        var a = new Dictionary<string, int> { ["a"] = 1 };
        var b = new Dictionary<string, int> { ["a"] = 2 };

        Assert.NotEqual(OrderedDictComparer.GetHashCode(a), OrderedDictComparer.GetHashCode(b));
    }

    // ── Cross-comparer: empty vs single-element ──────────────────────────────────────────────────

    [Fact]
    public void Dictionary_SingleEntry_DiffersFromEmpty()
    {
        var single = new Dictionary<string, int> { ["a"] = 1 };
        var empty = new Dictionary<string, int>();

        Assert.NotEqual(DictComparer.GetHashCode(single), DictComparer.GetHashCode(empty));
    }

    [Fact]
    public void HashSet_SingleEntry_DiffersFromEmpty()
    {
        var single = new HashSet<int> { 42 };
        var empty = new HashSet<int>();

        Assert.NotEqual(SetComparer.GetHashCode(single), SetComparer.GetHashCode(empty));
    }
}
