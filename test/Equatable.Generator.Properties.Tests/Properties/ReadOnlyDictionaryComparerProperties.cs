using Equatable.Comparers;

namespace Equatable.Generator.Tests.Properties;

public class ReadOnlyDictionaryComparerProperties
{
    private static readonly ReadOnlyDictionaryEqualityComparer<string, int> Comparer = ReadOnlyDictionaryEqualityComparer<string, int>.Default;

    [Property]
    public Property Reflexivity(Dictionary<string, int> dict)
    {
        IReadOnlyDictionary<string, int> d = dict;
        return Comparer.Equals(d, d).ToProperty();
    }

    [Property]
    public Property Symmetry(Dictionary<string, int> x, Dictionary<string, int> y)
    {
        IReadOnlyDictionary<string, int> a = x;
        IReadOnlyDictionary<string, int> b = y;
        return (Comparer.Equals(a, b) == Comparer.Equals(b, a)).ToProperty();
    }

    [Property]
    public Property EqualImpliesSameHashCode(Dictionary<string, int> dict)
    {
        // two dictionaries with same entries in different insertion order must have equal hash
        IReadOnlyDictionary<string, int> a = dict;
        IReadOnlyDictionary<string, int> b = new Dictionary<string, int>(dict.Reverse());
        return (Comparer.Equals(a, b) == (Comparer.GetHashCode(a) == Comparer.GetHashCode(b))).ToProperty();
    }

    [Property]
    public Property HashIsInsertionOrderIndependent(Dictionary<string, int> dict)
    {
        IReadOnlyDictionary<string, int> a = dict;
        IReadOnlyDictionary<string, int> b = new Dictionary<string, int>(dict.Reverse());
        return (Comparer.GetHashCode(a) == Comparer.GetHashCode(b)).ToProperty();
    }

    [Property]
    public Property NullEqualsNull()
    {
        return Comparer.Equals(null, null).ToProperty();
    }

    [Property]
    public Property NullNotEqualsNonNull(Dictionary<string, int> dict)
    {
        IReadOnlyDictionary<string, int> d = dict;
        return (!Comparer.Equals(null, d) && !Comparer.Equals(d, null)).ToProperty();
    }

    [Property]
    public Property ExtraKeyMakesNotEqual(Dictionary<string, int> dict, string key, int value)
    {
        if (key == null) return true.ToProperty().When(true);
        // guard: key must not already be in dict
        if (dict.ContainsKey(key))
            return true.ToProperty().When(true); // vacuously true — skip this input

        IReadOnlyDictionary<string, int> a = dict;
        var bigger = new Dictionary<string, int>(dict) { [key] = value };
        IReadOnlyDictionary<string, int> b = bigger;

        return (!Comparer.Equals(a, b)).ToProperty();
    }
}
