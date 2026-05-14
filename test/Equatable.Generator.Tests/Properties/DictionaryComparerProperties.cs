using Equatable.Comparers;

namespace Equatable.Generator.Tests.Properties;

public class DictionaryComparerProperties
{
    private static readonly DictionaryEqualityComparer<string, int> Comparer = DictionaryEqualityComparer<string, int>.Default;

    // Hash contract with custom keyComparer: Equals(x,y) → GetHashCode(x) == GetHashCode(y).
    // This catches the bug where Equals used y.TryGetValue (dict's internal comparer) while
    // GetHashCode used KeyComparer — the two could disagree, violating the contract.
    private static readonly DictionaryEqualityComparer<string, int> CaseInsensitiveComparer =
        new(StringComparer.OrdinalIgnoreCase, EqualityComparer<int>.Default);

    [Property]
    public Property CustomKeyComparer_HashContract_EqualImpliesSameHash(Dictionary<string, int> dict)
    {
        // Build a copy with keys mapped to their upper-case equivalents — equal under OrdinalIgnoreCase
        var upper = new Dictionary<string, int>();
        foreach (var pair in dict)
        {
            var upperKey = pair.Key.ToUpperInvariant();
            upper[upperKey] = pair.Value;   // last writer wins if two keys collide under upper-case
        }

        // Only assert the contract when Equals says they are equal
        return Prop.When(
            CaseInsensitiveComparer.Equals(dict, upper),
            CaseInsensitiveComparer.GetHashCode(dict) == CaseInsensitiveComparer.GetHashCode(upper));
    }

    [Property]
    public Property Equals_Reflexivity_SameInstance_ReturnsTrue(Dictionary<string, int> dict)
    {
        return Prop.ToProperty(Comparer.Equals(dict, dict));
    }

    [Property]
    public Property Equals_Symmetry_AEqualsB_ImpliesBEqualsA(Dictionary<string, int> x, Dictionary<string, int> y)
    {
        return Prop.ToProperty(Comparer.Equals(x, y) == Comparer.Equals(y, x));
    }

    [Property]
    public Property HashCode_InsertionOrderIndependent(Dictionary<string, int> dict)
    {
        var reversed = new Dictionary<string, int>(dict.Reverse());
        return Prop.ToProperty(Comparer.GetHashCode(dict) == Comparer.GetHashCode(reversed));
    }

    [Property]
    public Property HashCode_EqualDictionaries_HaveSameHash(Dictionary<string, int> dict)
    {
        var copy = new Dictionary<string, int>(dict);
        return Prop.ToProperty(Comparer.Equals(dict, copy) && Comparer.GetHashCode(dict) == Comparer.GetHashCode(copy));
    }

    [Property]
    public Property Equals_DifferentValue_ReturnsFalse(Dictionary<string, int> dict, string key, int v1, int v2)
    {
        if (v1 == v2)
            return Prop.When(true, true);

        var a = new Dictionary<string, int>(dict) { [key] = v1 };
        var b = new Dictionary<string, int>(dict) { [key] = v2 };

        // different values must NOT be equal
        return Prop.ToProperty(!Comparer.Equals(a, b));
    }
}
