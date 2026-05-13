using Equatable.Comparers;

namespace Equatable.Generator.Tests.Comparers;

public class SequenceEqualityComparerTest
{
    [Fact]
    public void DefaultEquals()
    {
        var a = new List<int>([10, 5]);

        var b = new List<int>([10, 5]);

        var comparer = SequenceEqualityComparer<int>.Default;
        Assert.True(comparer.Equals(a, b));
    }

    [Fact]
    public void NotEqualsValues()
    {
        var a = new List<int>([10, 5]);

        var b = new List<int>([-10, 5]);

        var comparer = SequenceEqualityComparer<int>.Default;
        Assert.False(comparer.Equals(a, b));
    }

    [Fact]
    public void NotEqualsValuesNull()
    {
        var a = new List<int?>([10, 5]);

        var b = new List<int?>([10, null]);

        var comparer = SequenceEqualityComparer<int?>.Default;
        Assert.False(comparer.Equals(a, b));
    }

    [Fact]
    public void EqualsValuesNull()
    {
        var a = new List<int?>([10, null]);

        var b = new List<int?>([10, null]);


        var comparer = SequenceEqualityComparer<int?>.Default;
        Assert.True(comparer.Equals(a, b));
    }

    [Fact]
    public void NotEqualsCount()
    {
        var a = new List<int>([10, 5]);

        var b = new List<int>();

        var comparer = SequenceEqualityComparer<int>.Default;
        Assert.False(comparer.Equals(a, b));
    }

    [Fact]
    public void NotEqualsNull()
    {
        var a = new List<int>([10, 5]);

        var comparer = SequenceEqualityComparer<int>.Default;
        Assert.False(comparer.Equals(a, null));
    }

    [Fact]
    public void GetHashCodeSame()
    {
        var a = new List<int>([10, 5]);

        var b = new List<int>([10, 5]);

        var comparer = SequenceEqualityComparer<int>.Default;
        var aHash = comparer.GetHashCode(a);
        var bHash = comparer.GetHashCode(b);

        Assert.Equal(bHash, aHash);
    }

    // ── custom comparer constructor path ─────────────────────────────────────────────────────────

    [Fact]
    public void CustomComparer_EqualByCustomRule()
    {
        // OrdinalIgnoreCase: ["A","B"] and ["a","b"] are element-wise equal
        var comparer = new SequenceEqualityComparer<string>(StringComparer.OrdinalIgnoreCase);
        Assert.True(comparer.Equals(["A", "B"], ["a", "b"]));
    }

    [Fact]
    public void CustomComparer_StillOrderSensitive()
    {
        // Sequence equality is always position-sensitive regardless of element comparer
        var comparer = new SequenceEqualityComparer<string>(StringComparer.OrdinalIgnoreCase);
        Assert.False(comparer.Equals(["A", "B"], ["B", "A"]));
    }

    [Fact]
    public void CustomComparer_GetHashCode_UsesComparer()
    {
        // Equal sequences under the custom comparer must produce the same hash
        var comparer = new SequenceEqualityComparer<string>(StringComparer.OrdinalIgnoreCase);
        Assert.Equal(comparer.GetHashCode(["A", "B"]), comparer.GetHashCode(["a", "b"]));
    }
}
