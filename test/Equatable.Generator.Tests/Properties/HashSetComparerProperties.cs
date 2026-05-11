using Equatable.Comparers;

namespace Equatable.Generator.Tests.Properties;

public class HashSetComparerProperties
{
    private static readonly HashSetEqualityComparer<string> Comparer = HashSetEqualityComparer<string>.Default;

    [Property]
    public Property Reflexivity(HashSet<string> set)
    {
        return Comparer.Equals(set, set).ToProperty();
    }

    [Property]
    public Property Symmetry(HashSet<string> x, HashSet<string> y)
    {
        return (Comparer.Equals(x, y) == Comparer.Equals(y, x)).ToProperty();
    }

    [Property]
    public Property HashIsInsertionOrderIndependent(HashSet<string> set)
    {
        // build same set from reversed list — HashSet has no guaranteed iteration order,
        // but two sets with identical elements must have equal hash regardless of add order
        var reversed = new HashSet<string>(set.Reverse());
        return (Comparer.GetHashCode(set) == Comparer.GetHashCode(reversed)).ToProperty();
    }

    [Property]
    public Property EqualSetsHaveSameHashCode(HashSet<string> set)
    {
        var copy = new HashSet<string>(set);
        return (Comparer.Equals(set, copy) && Comparer.GetHashCode(set) == Comparer.GetHashCode(copy)).ToProperty();
    }

    [Property]
    public Property ExtraElementMakesNotEqual(HashSet<string> set, string extra)
    {
        if (set.Contains(extra))
            return true.ToProperty().When(true);

        var bigger = new HashSet<string>(set) { extra };
        return (!Comparer.Equals(set, bigger)).ToProperty();
    }

    [Property]
    public Property NullEqualsNull()
    {
        return Comparer.Equals(null, null).ToProperty();
    }
}
