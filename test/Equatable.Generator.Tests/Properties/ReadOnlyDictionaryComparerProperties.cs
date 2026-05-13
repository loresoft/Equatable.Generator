using Equatable.Comparers;

namespace Equatable.Generator.Tests.Properties;

public class ReadOnlyDictionaryComparerProperties
{
    private static readonly ReadOnlyDictionaryEqualityComparer<string, int> Comparer = ReadOnlyDictionaryEqualityComparer<string, int>.Default;

    [Property]
    public Property Equals_Reflexivity_SameInstance_ReturnsTrue(Dictionary<string, int> dict)
    {
        IReadOnlyDictionary<string, int> d = dict;
        return Prop.ToProperty(Comparer.Equals(d, d));
    }

    [Property]
    public Property Equals_Symmetry_AEqualsB_ImpliesBEqualsA(Dictionary<string, int> x, Dictionary<string, int> y)
    {
        IReadOnlyDictionary<string, int> a = x;
        IReadOnlyDictionary<string, int> b = y;
        return Prop.ToProperty(Comparer.Equals(a, b) == Comparer.Equals(b, a));
    }

    [Property]
    public Property HashCode_EqualDictionaries_HaveSameHash(Dictionary<string, int> dict)
    {
        // equal → same hash (one direction only — hash collisions can make unequal produce same hash)
        IReadOnlyDictionary<string, int> a = dict;
        IReadOnlyDictionary<string, int> b = new Dictionary<string, int>(dict.Reverse());
        return Prop.When(Comparer.Equals(a, b), Comparer.GetHashCode(a) == Comparer.GetHashCode(b));
    }

    [Property]
    public Property HashCode_InsertionOrderIndependent(Dictionary<string, int> dict)
    {
        IReadOnlyDictionary<string, int> a = dict;
        IReadOnlyDictionary<string, int> b = new Dictionary<string, int>(dict.Reverse());
        return Prop.ToProperty(Comparer.GetHashCode(a) == Comparer.GetHashCode(b));
    }

    [Property]
    public Property Equals_BothNull_ReturnsTrue()
    {
        return Prop.ToProperty(Comparer.Equals(null, null));
    }

    [Property]
    public Property Equals_OneNull_ReturnsFalse(Dictionary<string, int> dict)
    {
        IReadOnlyDictionary<string, int> d = dict;
        return Prop.ToProperty(!Comparer.Equals(null, d) && !Comparer.Equals(d, null));
    }

    [Property]
    public Property Equals_DictionaryWithExtraKey_ReturnsFalse(Dictionary<string, int> dict, string key, int value)
    {
        // guard: key must not already be in dict
        if (dict.ContainsKey(key))
            return Prop.When(true, true); // vacuously true — skip this input

        IReadOnlyDictionary<string, int> a = dict;
        var bigger = new Dictionary<string, int>(dict) { [key] = value };
        IReadOnlyDictionary<string, int> b = bigger;

        return Prop.ToProperty(!Comparer.Equals(a, b));
    }
}
