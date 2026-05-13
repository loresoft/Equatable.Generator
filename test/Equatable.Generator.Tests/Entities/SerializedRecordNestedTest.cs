using Equatable.Entities;

namespace Equatable.Generator.Tests.Entities;

/// <summary>
/// Verifies that [MessagePackEquatable] infers structurally-recursive comparers
/// for nested collection properties — i.e. that InferCollectionComparer composes
/// DictionaryEqualityComparer / SequenceEqualityComparer at every level rather than
/// falling back to EqualityComparer&lt;T&gt;.Default (which would be reference equality
/// for any collection value type).
///
/// Each test exercises the generated Equals / GetHashCode directly on entity instances,
/// not the comparer classes themselves.
/// </summary>
public class SerializedRecordNestedTest
{
    // ── Dict<string, List<int>>: values are sequences, order inside lists matters ─────────────────

    [Fact]
    public void DictOfList_GeneratedEquals_ComparesInnerListsStructurally()
    {
        // Separate List<int> instances with the same content must be equal.
        // Fails with reference equality; passes only when SequenceEqualityComparer is composed.
        var a = new SerializedRecordNested { Id = 1, TagGroups = new() { ["sports"] = [10, 20], ["news"] = [30] } };
        var b = new SerializedRecordNested { Id = 1, TagGroups = new() { ["sports"] = [10, 20], ["news"] = [30] } };

        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void DictOfList_GeneratedEquals_OuterDictInsertionOrderIsIgnored()
    {
        // DictionaryEqualityComparer uses TryGetValue — outer key order must not matter.
        var a = new SerializedRecordNested { Id = 1, TagGroups = new() { ["sports"] = [10], ["news"] = [20] } };
        var b = new SerializedRecordNested { Id = 1, TagGroups = new() { ["news"] = [20], ["sports"] = [10] } };

        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void DictOfList_GeneratedEquals_InnerListOrderIsEnforced()
    {
        // SequenceEqualityComparer is order-sensitive: [10, 20] ≠ [20, 10].
        var a = new SerializedRecordNested { Id = 1, TagGroups = new() { ["sports"] = [10, 20] } };
        var b = new SerializedRecordNested { Id = 1, TagGroups = new() { ["sports"] = [20, 10] } };

        Assert.False(a.Equals(b));
    }

    [Fact]
    public void DictOfList_GeneratedEquals_DifferentInnerElement_IsNotEqual()
    {
        var a = new SerializedRecordNested { Id = 1, TagGroups = new() { ["sports"] = [10] } };
        var b = new SerializedRecordNested { Id = 1, TagGroups = new() { ["sports"] = [99] } };

        Assert.False(a.Equals(b));
    }

    // ── Dict<string, Dict<string, int>>: values are dicts, insertion order at both levels ignored ──

    [Fact]
    public void DictOfDict_GeneratedEquals_ComparesInnerDictsStructurally()
    {
        // Separate inner Dictionary instances with the same content must be equal.
        // Fails with reference equality; passes only when DictionaryEqualityComparer is composed.
        var a = new SerializedRecordNested { Id = 1, NestedMap = new() { ["outer"] = new() { ["inner"] = 42 } } };
        var b = new SerializedRecordNested { Id = 1, NestedMap = new() { ["outer"] = new() { ["inner"] = 42 } } };

        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void DictOfDict_GeneratedEquals_InnerDictInsertionOrderIsIgnored()
    {
        var a = new SerializedRecordNested { Id = 1, NestedMap = new() { ["outer"] = new() { ["a"] = 1, ["b"] = 2 } } };
        var b = new SerializedRecordNested { Id = 1, NestedMap = new() { ["outer"] = new() { ["b"] = 2, ["a"] = 1 } } };

        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void DictOfDict_GeneratedEquals_DifferentInnerValue_IsNotEqual()
    {
        var a = new SerializedRecordNested { Id = 1, NestedMap = new() { ["outer"] = new() { ["k"] = 42 } } };
        var b = new SerializedRecordNested { Id = 1, NestedMap = new() { ["outer"] = new() { ["k"] = 99 } } };

        Assert.False(a.Equals(b));
    }

    // ── List<Dict<string, int>>: outer list order enforced, inner dict order ignored ───────────────

    [Fact]
    public void ListOfDict_GeneratedEquals_ComparesInnerDictsStructurally()
    {
        // Separate inner Dictionary instances with the same content must be equal.
        var a = new SerializedRecordNested { Id = 1, Records = [new() { ["x"] = 1 }, new() { ["y"] = 2 }] };
        var b = new SerializedRecordNested { Id = 1, Records = [new() { ["x"] = 1 }, new() { ["y"] = 2 }] };

        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void ListOfDict_GeneratedEquals_OuterListOrderIsEnforced()
    {
        // SequenceEqualityComparer is position-sensitive for the outer list.
        var a = new SerializedRecordNested { Id = 1, Records = [new() { ["x"] = 1 }, new() { ["y"] = 2 }] };
        var b = new SerializedRecordNested { Id = 1, Records = [new() { ["y"] = 2 }, new() { ["x"] = 1 }] };

        Assert.False(a.Equals(b));
    }

    [Fact]
    public void ListOfDict_GeneratedEquals_InnerDictInsertionOrderIsIgnored()
    {
        var a = new SerializedRecordNested { Id = 1, Records = [new() { ["a"] = 1, ["b"] = 2 }] };
        var b = new SerializedRecordNested { Id = 1, Records = [new() { ["b"] = 2, ["a"] = 1 }] };

        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    // ── IReadOnlyDictionary<string, List<string>>: read-only variant, same structural rules ────────

    [Fact]
    public void ReadOnlyDictOfList_GeneratedEquals_ComparesInnerListsStructurally()
    {
        var a = new SerializedRecordNested
        {
            Id = 1,
            ReadOnlyTagGroups = new Dictionary<string, List<string>> { ["a"] = ["x", "y"], ["b"] = ["z"] }
        };
        var b = new SerializedRecordNested
        {
            Id = 1,
            ReadOnlyTagGroups = new Dictionary<string, List<string>> { ["b"] = ["z"], ["a"] = ["x", "y"] }
        };

        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    // ── Null / empty collection discrimination ────────────────────────────────────────────────────

    [Fact]
    public void GeneratedEquals_NullCollection_IsNotEqualToEmpty()
    {
        // GetHashCode returns 0 for null, 1 (seed) for empty — they must also not be Equal.
        var a = new SerializedRecordNested { Id = 1, TagGroups = null };
        var b = new SerializedRecordNested { Id = 1, TagGroups = [] };

        Assert.False(a.Equals(b));
    }

    [Fact]
    public void GeneratedEquals_BothNullCollections_AreEqual()
    {
        var a = new SerializedRecordNested { Id = 1, TagGroups = null };
        var b = new SerializedRecordNested { Id = 1, TagGroups = null };

        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}
