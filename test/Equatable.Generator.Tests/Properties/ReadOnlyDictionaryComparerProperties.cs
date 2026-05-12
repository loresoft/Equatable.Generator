using Equatable.Comparers;

namespace Equatable.Generator.Tests.Properties;

public class ReadOnlyDictionaryComparerProperties
{
    private static readonly ReadOnlyDictionaryEqualityComparer<string, int> Comparer = ReadOnlyDictionaryEqualityComparer<string, int>.Default;

    [Property]
    public Property Reflexivity(Dictionary<string, int> dict)
    {
        IReadOnlyDictionary<string, int> d = dict;
        return Prop.ToProperty(Comparer.Equals(d, d));
    }

    [Property]
    public Property Symmetry(Dictionary<string, int> x, Dictionary<string, int> y)
    {
        IReadOnlyDictionary<string, int> a = x;
        IReadOnlyDictionary<string, int> b = y;
        return Prop.ToProperty(Comparer.Equals(a, b) == Comparer.Equals(b, a));
    }

    [Property]
    public Property EqualImpliesSameHashCode(Dictionary<string, int> dict)
    {
        // two dictionaries with same entries in different insertion order must have equal hash
        IReadOnlyDictionary<string, int> a = dict;
        IReadOnlyDictionary<string, int> b = new Dictionary<string, int>(dict.Reverse());
        return Prop.ToProperty(Comparer.Equals(a, b) == (Comparer.GetHashCode(a) == Comparer.GetHashCode(b)));
    }

    [Property]
    public Property HashIsInsertionOrderIndependent(Dictionary<string, int> dict)
    {
        IReadOnlyDictionary<string, int> a = dict;
        IReadOnlyDictionary<string, int> b = new Dictionary<string, int>(dict.Reverse());
        return Prop.ToProperty(Comparer.GetHashCode(a) == Comparer.GetHashCode(b));
    }

    [Property]
    public Property NullEqualsNull()
    {
        return Prop.ToProperty(Comparer.Equals(null, null));
    }

    [Property]
    public Property NullNotEqualsNonNull(Dictionary<string, int> dict)
    {
        IReadOnlyDictionary<string, int> d = dict;
        return Prop.ToProperty(!Comparer.Equals(null, d) && !Comparer.Equals(d, null));
    }

    [Property]
    public Property ExtraKeyMakesNotEqual(Dictionary<string, int> dict, string key, int value)
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
