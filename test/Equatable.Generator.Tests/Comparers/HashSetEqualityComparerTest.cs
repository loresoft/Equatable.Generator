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
        comparer.Equals(a, b).Should().BeTrue();
    }

    [Fact]
    public void EqualsOutOfOrder()
    {
        var a = new HashSet<int>([10, 5]);

        var b = new HashSet<int>([5, 10]);

        var comparer = HashSetEqualityComparer<int>.Default;
        comparer.Equals(a, b).Should().BeTrue();
    }

    [Fact]
    public void NotEqualsValues()
    {
        var a = new HashSet<int>([10, 5]);

        var b = new HashSet<int>([-10, 5]);

        var comparer = HashSetEqualityComparer<int>.Default;
        comparer.Equals(a, b).Should().BeFalse();
    }

    [Fact]
    public void NotEqualsValuesNull()
    {
        var a = new HashSet<int?>([10, 5]);

        var b = new HashSet<int?>([10, null]);

        var comparer = HashSetEqualityComparer<int?>.Default;
        comparer.Equals(a, b).Should().BeFalse();
    }

    [Fact]
    public void EqualsValuesNull()
    {
        var a = new HashSet<int?>([10, null]);

        var b = new HashSet<int?>([10, null]);


        var comparer = HashSetEqualityComparer<int?>.Default;
        comparer.Equals(a, b).Should().BeTrue();
    }

    [Fact]
    public void NotEqualsCount()
    {
        var a = new HashSet<int>([10, 5]);

        var b = new HashSet<int>();

        var comparer = HashSetEqualityComparer<int>.Default;
        comparer.Equals(a, b).Should().BeFalse();
    }

    [Fact]
    public void NotEqualsNull()
    {
        var a = new HashSet<int>([10, 5]);

        var comparer = HashSetEqualityComparer<int>.Default;
        comparer.Equals(a, null).Should().BeFalse();
    }

    [Fact]
    public void GetHashCodeSame()
    {
        var a = new HashSet<int>([10, 5]);

        var b = new HashSet<int>([5, 10]);

        var comparer = HashSetEqualityComparer<int>.Default;
        var aHash = comparer.GetHashCode(a);
        var bHash = comparer.GetHashCode(b);

        aHash.Should().Be(bHash);
    }
}
