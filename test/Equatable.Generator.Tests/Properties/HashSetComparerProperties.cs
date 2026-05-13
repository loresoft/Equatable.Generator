using Equatable.Comparers;

namespace Equatable.Generator.Tests.Properties;

[Properties(Arbitrary = new[] { typeof(Arbitraries) })]
public class HashSetComparerProperties
{
    private static readonly HashSetEqualityComparer<string> Comparer = HashSetEqualityComparer<string>.Default;

    [Property]
    public Property Equals_Reflexivity_SameInstance_ReturnsTrue(HashSet<string> set)
    {
        return Prop.ToProperty(Comparer.Equals(set, set));
    }

    [Property]
    public Property Equals_Symmetry_AEqualsB_ImpliesBEqualsA(HashSet<string> x, HashSet<string> y)
    {
        return Prop.ToProperty(Comparer.Equals(x, y) == Comparer.Equals(y, x));
    }

    [Property]
    public Property HashCode_InsertionOrderIndependent(HashSet<string> set)
    {
        // build same set from reversed list — HashSet has no guaranteed iteration order,
        // but two sets with identical elements must have equal hash regardless of add order
        var reversed = new HashSet<string>(set.Reverse());
        return Prop.ToProperty(Comparer.GetHashCode(set) == Comparer.GetHashCode(reversed));
    }

    [Property]
    public Property HashCode_EqualSets_HaveSameHash(HashSet<string> set)
    {
        var copy = new HashSet<string>(set);
        return Prop.ToProperty(Comparer.Equals(set, copy) && Comparer.GetHashCode(set) == Comparer.GetHashCode(copy));
    }

    [Property]
    public Property Equals_SupersetOfElements_ReturnsFalse(HashSet<string> set, string extra)
    {
        if (set.Contains(extra))
            return Prop.When(true, true);

        var bigger = new HashSet<string>(set) { extra };
        return Prop.ToProperty(!Comparer.Equals(set, bigger));
    }

    [Property]
    public Property Equals_BothNull_ReturnsTrue()
    {
        return Prop.ToProperty(Comparer.Equals(null, null));
    }
}
