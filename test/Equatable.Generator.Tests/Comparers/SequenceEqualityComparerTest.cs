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
        comparer.Equals(a, b).Should().BeTrue();
    }

    [Fact]
    public void NotEqualsValues()
    {
        var a = new List<int>([10, 5]);

        var b = new List<int>([-10, 5]);

        var comparer = SequenceEqualityComparer<int>.Default;
        comparer.Equals(a, b).Should().BeFalse();
    }

    [Fact]
    public void NotEqualsValuesNull()
    {
        var a = new List<int?>([10, 5]);

        var b = new List<int?>([10, null]);

        var comparer = SequenceEqualityComparer<int?>.Default;
        comparer.Equals(a, b).Should().BeFalse();
    }

    [Fact]
    public void EqualsValuesNull()
    {
        var a = new List<int?>([10, null]);

        var b = new List<int?>([10, null]);


        var comparer = SequenceEqualityComparer<int?>.Default;
        comparer.Equals(a, b).Should().BeTrue();
    }

    [Fact]
    public void NotEqualsCount()
    {
        var a = new List<int>([10, 5]);

        var b = new List<int>();

        var comparer = SequenceEqualityComparer<int>.Default;
        comparer.Equals(a, b).Should().BeFalse();
    }

    [Fact]
    public void NotEqualsNull()
    {
        var a = new List<int>([10, 5]);

        var comparer = SequenceEqualityComparer<int>.Default;
        comparer.Equals(a, null).Should().BeFalse();
    }

    [Fact]
    public void GetHashCodeSame()
    {
        var a = new List<int>([10, 5]);

        var b = new List<int>([10, 5]);

        var comparer = SequenceEqualityComparer<int>.Default;
        var aHash = comparer.GetHashCode(a);
        var bHash = comparer.GetHashCode(b);

        aHash.Should().Be(bHash);
    }
}
