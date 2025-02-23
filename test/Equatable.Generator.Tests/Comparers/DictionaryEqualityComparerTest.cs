using Equatable.Comparers;

namespace Equatable.Generator.Tests.Comparers;

public class DictionaryEqualityComparerTest
{
    [Fact]
    public void DefaultEquals()
    {
        var a = new Dictionary<string, int>
        {
            ["a"] = 10,
            ["b"] = 5
        };

        var b = new Dictionary<string, int>
        {
            ["a"] = 10,
            ["b"] = 5
        };

        var comparer = DictionaryEqualityComparer<string, int>.Default;
        Assert.True(comparer.Equals(a, b));
    }

    [Fact]
    public void NotEqualsKeys()
    {
        var a = new Dictionary<string, int>
        {
            ["a"] = 10,
            ["b"] = 5
        };

        var b = new Dictionary<string, int>
        {
            ["c"] = -10,
            ["b"] = 5
        };

        var comparer = DictionaryEqualityComparer<string, int>.Default;
        Assert.False(comparer.Equals(a, b));
    }

    [Fact]
    public void NotEqualsValues()
    {
        var a = new Dictionary<string, int>
        {
            ["a"] = 10,
            ["b"] = 5
        };

        var b = new Dictionary<string, int>
        {
            ["a"] = -10,
            ["b"] = 5
        };

        var comparer = DictionaryEqualityComparer<string, int>.Default;
        Assert.False(comparer.Equals(a, b));
    }

    [Fact]
    public void NotEqualsValuesNull()
    {
        var a = new Dictionary<string, int?>
        {
            ["a"] = 10,
            ["b"] = 5
        };

        var b = new Dictionary<string, int?>
        {
            ["a"] = 10,
            ["b"] = null
        };

        var comparer = DictionaryEqualityComparer<string, int?>.Default;
        Assert.False(comparer.Equals(a, b));
    }

    [Fact]
    public void EqualsValuesNull()
    {
        var a = new Dictionary<string, int?>
        {
            ["a"] = 10,
            ["b"] = null
        };

        var b = new Dictionary<string, int?>
        {
            ["a"] = 10,
            ["b"] = null
        };

        var comparer = DictionaryEqualityComparer<string, int?>.Default;
        Assert.True(comparer.Equals(a, b));
    }

    [Fact]
    public void NotEqualsCount()
    {
        var a = new Dictionary<string, int>
        {
            ["a"] = 10,
            ["b"] = 5
        };

        var b = new Dictionary<string, int>();

        var comparer = DictionaryEqualityComparer<string, int>.Default;
        Assert.False(comparer.Equals(a, b));
    }

    [Fact]
    public void NotEqualsNull()
    {
        var a = new Dictionary<string, int>
        {
            ["a"] = 10,
            ["b"] = 5
        };

        var comparer = DictionaryEqualityComparer<string, int>.Default;
        Assert.False(comparer.Equals(a, null));
    }

    [Fact]
    public void GetHashCodeSame()
    {
        var a = new Dictionary<string, int>
        {
            ["a"] = 10,
            ["b"] = 5
        };

        var b = new Dictionary<string, int>
        {
            ["a"] = 10,
            ["b"] = 5
        };

        var comparer = DictionaryEqualityComparer<string, int>.Default;
        var aHash = comparer.GetHashCode(a);
        var bHash = comparer.GetHashCode(b);

        Assert.Equal(bHash, aHash);
    }

    [Fact]
    public void GetHashCodeSameDifferentOrder()
    {
        var a = new Dictionary<string, int>
        {
            ["a"] = 10,
            ["b"] = 5
        };

        var b = new Dictionary<string, int>
        {
            ["b"] = 5,
            ["a"] = 10
        };

        var comparer = DictionaryEqualityComparer<string, int>.Default;
        var aHash = comparer.GetHashCode(a);
        var bHash = comparer.GetHashCode(b);

        Assert.Equal(bHash, aHash);
    }
}
