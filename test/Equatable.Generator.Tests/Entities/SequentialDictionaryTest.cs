using System.Collections.Generic;
using Equatable.Entities;

namespace Equatable.Generator.Tests.Entities;

/// <summary>
/// Tests for [DictionaryEquality(sequential: true)].
/// Equality and hash code both sort by key before comparing,
/// so two dictionaries are equal iff they have the same key/value pairs regardless of insertion order.
/// </summary>
public class SequentialDictionaryTest
{
    [Fact]
    public void Equals_SamePairs_DifferentInsertionOrder_ReturnsTrue()
    {
        var left  = new SequentialDictionary { Entries = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 } };
        var right = new SequentialDictionary { Entries = new Dictionary<string, int> { ["b"] = 2, ["a"] = 1 } };

        Assert.True(left.Equals(right));
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var left  = new SequentialDictionary { Entries = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 } };
        var right = new SequentialDictionary { Entries = new Dictionary<string, int> { ["a"] = 1, ["b"] = 99 } };

        Assert.False(left.Equals(right));
    }

    [Fact]
    public void Equals_DifferentKeys_ReturnsFalse()
    {
        var left  = new SequentialDictionary { Entries = new Dictionary<string, int> { ["a"] = 1 } };
        var right = new SequentialDictionary { Entries = new Dictionary<string, int> { ["z"] = 1 } };

        Assert.False(left.Equals(right));
    }

    [Fact]
    public void HashCode_SamePairs_DifferentInsertionOrder_Equal()
    {
        var left  = new SequentialDictionary { Entries = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 } };
        var right = new SequentialDictionary { Entries = new Dictionary<string, int> { ["c"] = 3, ["a"] = 1, ["b"] = 2 } };

        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }

    [Fact]
    public void HashCode_DifferentValues_NotEqual()
    {
        var left  = new SequentialDictionary { Entries = new Dictionary<string, int> { ["a"] = 1 } };
        var right = new SequentialDictionary { Entries = new Dictionary<string, int> { ["a"] = 2 } };

        Assert.NotEqual(left.GetHashCode(), right.GetHashCode());
    }

    [Fact]
    public void ReadOnly_Equals_SamePairs_DifferentInsertionOrder_ReturnsTrue()
    {
        var left  = new SequentialDictionary { ReadOnlyEntries = new Dictionary<string, int> { ["x"] = 10, ["y"] = 20 } };
        var right = new SequentialDictionary { ReadOnlyEntries = new Dictionary<string, int> { ["y"] = 20, ["x"] = 10 } };

        Assert.True(left.Equals(right));
    }

    [Fact]
    public void ReadOnly_HashCode_SamePairs_DifferentInsertionOrder_Equal()
    {
        var left  = new SequentialDictionary { ReadOnlyEntries = new Dictionary<string, int> { ["x"] = 10, ["y"] = 20 } };
        var right = new SequentialDictionary { ReadOnlyEntries = new Dictionary<string, int> { ["y"] = 20, ["x"] = 10 } };

        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }
}
