using Equatable.Comparers;

namespace Equatable.Generator.Tests.Comparers;

public class HashSetEqualityComparerTest
{
    [Fact]
    public void DefaultEquals()
    {
        var a = new HashSet<int>([10, 5]);

        var b = new HashSet<int>([10, 5]);

        var comparer = HashSetEqualityComparer<int>.Default;
        Assert.True(comparer.Equals(a, b));
    }

    [Fact]
    public void EqualsOutOfOrder()
    {
        var a = new HashSet<int>([10, 5]);

        var b = new HashSet<int>([5, 10]);

        var comparer = HashSetEqualityComparer<int>.Default;
        Assert.True(comparer.Equals(a, b));
    }

    [Fact]
    public void NotEqualsValues()
    {
        var a = new HashSet<int>([10, 5]);

        var b = new HashSet<int>([-10, 5]);

        var comparer = HashSetEqualityComparer<int>.Default;
        Assert.False(comparer.Equals(a, b));
    }

    [Fact]
    public void NotEqualsValuesNull()
    {
        var a = new HashSet<int?>([10, 5]);

        var b = new HashSet<int?>([10, null]);

        var comparer = HashSetEqualityComparer<int?>.Default;
        Assert.False(comparer.Equals(a, b));
    }

    [Fact]
    public void EqualsValuesNull()
    {
        var a = new HashSet<int?>([10, null]);

        var b = new HashSet<int?>([10, null]);


        var comparer = HashSetEqualityComparer<int?>.Default;
        Assert.True(comparer.Equals(a, b));
    }

    [Fact]
    public void NotEqualsCount()
    {
        var a = new HashSet<int>([10, 5]);

        var b = new HashSet<int>();

        var comparer = HashSetEqualityComparer<int>.Default;
        Assert.False(comparer.Equals(a, b));
    }

    [Fact]
    public void NotEqualsNull()
    {
        var a = new HashSet<int>([10, 5]);

        var comparer = HashSetEqualityComparer<int>.Default;
        Assert.False(comparer.Equals(a, null));
    }

    [Fact]
    public void GetHashCodeSame()
    {
        var a = new HashSet<int>([10, 5]);

        var b = new HashSet<int>([5, 10]);

        var comparer = HashSetEqualityComparer<int>.Default;
        var aHash = comparer.GetHashCode(a);
        var bHash = comparer.GetHashCode(b);

        Assert.Equal(bHash, aHash);
    }

    // ── custom comparer (Path C: plain IEnumerable inputs, neither is ISet) ─────────────────────
    // When both inputs are plain IEnumerable (not ISet), the code builds
    // new HashSet<T>(x, Comparer) and calls SetEquals(y), using this.Comparer.

    [Fact]
    public void CustomComparer_PlainEnumerable_EqualByCustomRule()
    {
        // OrdinalIgnoreCase: "A" and "a" are the same element
        var comparer = new HashSetEqualityComparer<string>(StringComparer.OrdinalIgnoreCase);
        IEnumerable<string> a = new List<string> { "A", "B" };
        IEnumerable<string> b = new List<string> { "b", "a" };

        Assert.True(comparer.Equals(a, b));
    }

    [Fact]
    public void CustomComparer_PlainEnumerable_DefaultComparerWouldNotMatch()
    {
        // Ordinal default: "A" != "a"
        var comparer = HashSetEqualityComparer<string>.Default;
        IEnumerable<string> a = new List<string> { "A" };
        IEnumerable<string> b = new List<string> { "a" };

        Assert.False(comparer.Equals(a, b));
    }

    [Fact]
    public void CustomComparer_GetHashCode_UsesComparer()
    {
        // With OrdinalIgnoreCase, "A" and "a" must produce the same hash
        var comparer = new HashSetEqualityComparer<string>(StringComparer.OrdinalIgnoreCase);
        IEnumerable<string> a = new List<string> { "A" };
        IEnumerable<string> b = new List<string> { "a" };

        Assert.True(comparer.Equals(a, b));
        Assert.Equal(comparer.GetHashCode(a), comparer.GetHashCode(b));
    }
}
