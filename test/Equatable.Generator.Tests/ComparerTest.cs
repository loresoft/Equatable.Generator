namespace Equatable.Generator.Tests;

public class ComparerTest
{
    [Fact]
    public void SequenceEqualsReturnsTrueForSameReference()
    {
        int[] values = [1, 2];

        Assert.True(Comparer.SequenceEquals(values, values));
    }

    [Fact]
    public void SequenceEqualsReturnsTrueForTwoNullSequences()
    {
        Assert.True(Comparer.SequenceEquals<int>(null, null));
    }

    [Fact]
    public void SequenceEqualsReturnsFalseForOneNullSequence()
    {
        Assert.False(Comparer.SequenceEquals([1], null));
    }

    [Theory]
    [InlineData(new[] { 1, 2 }, new[] { 1, 2 }, true)]
    [InlineData(new[] { 1, 2 }, new[] { 2, 1 }, false)]
    [InlineData(new[] { 1, 2 }, new[] { 1 }, false)]
    [InlineData(new[] { 1, 2 }, new[] { 1, 3 }, false)]
    public void SequenceEqualsComparesValuesInOrder(int[] left, int[] right, bool expected)
    {
        Assert.Equal(expected, Comparer.SequenceEquals(left, right));
    }

    [Fact]
    public void DictionaryEqualsReturnsTrueForSameReference()
    {
        var values = new Dictionary<string, int> { ["one"] = 1 };

        Assert.True(Comparer.DictionaryEquals(values, values));
    }

    [Fact]
    public void DictionaryEqualsReturnsTrueForTwoNullDictionaries()
    {
        Assert.True(Comparer.DictionaryEquals<string, int>(null, null));
    }

    [Fact]
    public void DictionaryEqualsReturnsFalseForOneNullDictionary()
    {
        var values = new Dictionary<string, int> { ["one"] = 1 };

        Assert.False(Comparer.DictionaryEquals(values, null));
    }

    [Fact]
    public void DictionaryEqualsReturnsTrueForEqualEntriesInDifferentOrder()
    {
        var left = new Dictionary<string, int?> { ["one"] = 1, ["none"] = null };
        var right = new Dictionary<string, int?> { ["none"] = null, ["one"] = 1 };

        Assert.True(Comparer.DictionaryEquals(left, right));
    }

    [Fact]
    public void DictionaryEqualsReturnsFalseForDifferentCounts()
    {
        var left = new Dictionary<string, int> { ["one"] = 1 };
        var right = new Dictionary<string, int>();

        Assert.False(Comparer.DictionaryEquals(left, right));
    }

    [Fact]
    public void DictionaryEqualsReturnsFalseForDifferentKeys()
    {
        var left = new Dictionary<string, int> { ["one"] = 1 };
        var right = new Dictionary<string, int> { ["two"] = 1 };

        Assert.False(Comparer.DictionaryEquals(left, right));
    }

    [Fact]
    public void DictionaryEqualsReturnsFalseForDifferentValues()
    {
        var left = new Dictionary<string, int> { ["one"] = 1 };
        var right = new Dictionary<string, int> { ["one"] = 2 };

        Assert.False(Comparer.DictionaryEquals(left, right));
    }

    [Fact]
    public void HashSetEqualsReturnsTrueForSameReference()
    {
        var values = new HashSet<int> { 1, 2 };

        Assert.True(Comparer.HashSetEquals(values, values));
    }

    [Fact]
    public void HashSetEqualsReturnsTrueForTwoNullSets()
    {
        Assert.True(Comparer.HashSetEquals<int>(null, null));
    }

    [Fact]
    public void HashSetEqualsReturnsFalseForOneNullSet()
    {
        Assert.False(Comparer.HashSetEquals([1], null));
    }

    [Theory]
    [InlineData(new[] { 1, 2 }, new[] { 2, 1 }, true)]
    [InlineData(new[] { 1, 2 }, new[] { 1, 3 }, false)]
    [InlineData(new[] { 1, 2 }, new[] { 1 }, false)]
    public void HashSetEqualsComparesSequencesWithoutConsideringOrder(int[] left, int[] right, bool expected)
    {
        Assert.Equal(expected, Comparer.HashSetEquals(left, right));
    }

    [Fact]
    public void HashSetEqualsUsesLeftSetComparer()
    {
        var left = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "VALUE" };
        string[] right = ["value"];

        Assert.True(Comparer.HashSetEquals(left, right));
    }

    [Fact]
    public void HashSetEqualsUsesRightSetComparer()
    {
        string[] left = ["VALUE"];
        var right = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "value" };

        Assert.True(Comparer.HashSetEquals(left, right));
    }

    [Fact]
    public void SequenceHashCodeReturnsZeroForNull()
    {
        Assert.Equal(0, Comparer.SequenceHashCode<int>(null));
    }

    [Fact]
    public void SequenceHashCodeReturnsSeedForEmptySequence()
    {
        Assert.Equal(Comparer.HashSeed, Comparer.SequenceHashCode(Array.Empty<int>()));
    }

    [Fact]
    public void SequenceHashCodeIncludesOrderAndNullValues()
    {
        int?[] values = [1, null, 2];
        int expected = Comparer.HashSeed;
        expected = unchecked((expected * Comparer.HashMultiplier) + 1);
        expected = unchecked(expected * Comparer.HashMultiplier);
        expected = unchecked((expected * Comparer.HashMultiplier) + 2);

        Assert.Equal(expected, Comparer.SequenceHashCode(values));
    }

    [Fact]
    public void DictionaryHashCodeReturnsZeroForNull()
    {
        Assert.Equal(0, Comparer.DictionaryHashCode<string, int>(null));
    }

    [Fact]
    public void DictionaryHashCodeReturnsSeedForEmptyDictionary()
    {
        var values = new Dictionary<string, int>();

        Assert.Equal(Comparer.HashSeed, Comparer.DictionaryHashCode(values));
    }

    [Fact]
    public void DictionaryHashCodeIsIndependentOfOrderAndSupportsNullValues()
    {
        var left = new Dictionary<int, string?> { [1] = "one", [2] = null };
        var right = new Dictionary<int, string?> { [2] = null, [1] = "one" };

        Assert.Equal(
            Comparer.DictionaryHashCode(left),
            Comparer.DictionaryHashCode(right));
    }

    [Fact]
    public void HashSetHashCodeReturnsZeroForNull()
    {
        Assert.Equal(0, Comparer.HashSetHashCode<int>(null));
    }

    [Fact]
    public void HashSetHashCodeReturnsSeedForEmptySet()
    {
        Assert.Equal(Comparer.HashSeed, Comparer.HashSetHashCode(Array.Empty<int>()));
    }

    [Fact]
    public void HashSetHashCodeIsIndependentOfOrderAndSupportsNullValues()
    {
        int?[] left = [1, null, 2];
        int?[] right = [2, 1, null];

        Assert.Equal(
            Comparer.HashSetHashCode(left),
            Comparer.HashSetHashCode(right));
    }

    [Theory]
    [InlineData(null, -1)]
    [InlineData(false, 0)]
    [InlineData(true, 1)]
    public void NullableBoolHashCodeReturnsStableValue(bool? value, int expected)
    {
        Assert.Equal(expected, Comparer.NullableBoolHashCode(value));
    }
}
