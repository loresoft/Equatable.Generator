using Equatable.Comparers;

namespace Equatable.Generator.Tests.Properties;

public class HashSetComparerProperties
{
    private static readonly HashSetEqualityComparer<string> Comparer = HashSetEqualityComparer<string>.Default;

    // FsCheck cannot auto-generate HashSet<T>; use string[] and convert.

    [Property]
    public Property Reflexivity(string[] items)
    {
        var set = new HashSet<string>(items);
        return Comparer.Equals(set, set).ToProperty();
    }

    [Property]
    public Property Symmetry(string[] xs, string[] ys)
    {
        var x = new HashSet<string>(xs);
        var y = new HashSet<string>(ys);
        return (Comparer.Equals(x, y) == Comparer.Equals(y, x)).ToProperty();
    }

    [Property]
    public Property HashIsInsertionOrderIndependent(string[] items)
    {
        var set = new HashSet<string>(items);
        var reversed = new HashSet<string>(items.Reverse());
        return (Comparer.GetHashCode(set) == Comparer.GetHashCode(reversed)).ToProperty();
    }

    [Property]
    public Property EqualSetsHaveSameHashCode(string[] items)
    {
        var set = new HashSet<string>(items);
        var copy = new HashSet<string>(items);
        return (Comparer.Equals(set, copy) && Comparer.GetHashCode(set) == Comparer.GetHashCode(copy)).ToProperty();
    }

    [Property]
    public Property ExtraElementMakesNotEqual(string[] items, string extra)
    {
        if (extra == null) return true.ToProperty().When(true);
        var set = new HashSet<string>(items);
        if (set.Contains(extra)) return true.ToProperty().When(true);
        var bigger = new HashSet<string>(set) { extra };
        return (!Comparer.Equals(set, bigger)).ToProperty();
    }

    [Property]
    public Property NullEqualsNull()
    {
        return Comparer.Equals(null, null).ToProperty();
    }
}
