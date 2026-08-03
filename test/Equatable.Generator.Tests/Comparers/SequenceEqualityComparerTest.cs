using System.Collections;

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

    [Fact]
    public void NotEqualsArrayCount()
    {
        var a = new[] { 10, 5, 1 };

        var b = new[] { 10, 5 };

        var comparer = SequenceEqualityComparer<int>.Default;
        Assert.False(comparer.Equals(a, b));
    }

    [Fact]
    public void EqualsReadOnlyCollection()
    {
        var a = new ReadOnlyCollectionOnly<int>([10, 5]);

        var b = new ReadOnlyCollectionOnly<int>([10, 5]);

        var comparer = SequenceEqualityComparer<int>.Default;
        Assert.True(comparer.Equals(a, b));
    }

    [Fact]
    public void NotEqualsReadOnlyCollectionCount()
    {
        var a = new ReadOnlyCollectionOnly<int>([10, 5]);

        var b = new ReadOnlyCollectionOnly<int>([10]);

        var comparer = SequenceEqualityComparer<int>.Default;
        Assert.False(comparer.Equals(a, b));
    }

    [Fact]
    public void NotEqualsLazySequenceCount()
    {
        var a = new CountingEnumerable<int>([10, 5, 1]);

        var b = new CountingEnumerable<int>([10, 5]);

        var comparer = SequenceEqualityComparer<int>.Default;
        Assert.False(comparer.Equals(a, b));
    }

    [Fact]
    public void LazySequenceEnumeratedOnce()
    {
        var a = new CountingEnumerable<int>([10, 5, 1]);

        var b = new CountingEnumerable<int>([10, 5]);

        var comparer = SequenceEqualityComparer<int>.Default;
        comparer.Equals(a, b);

        Assert.Equal(1, a.EnumerationCount);
    }

    private sealed class ReadOnlyCollectionOnly<T>(IReadOnlyList<T> values) : IReadOnlyCollection<T>
    {
        public int Count => values.Count;

        public IEnumerator<T> GetEnumerator() => values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class CountingEnumerable<T>(IReadOnlyList<T> values) : IEnumerable<T>
    {
        public int EnumerationCount { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            EnumerationCount++;
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
