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
}
