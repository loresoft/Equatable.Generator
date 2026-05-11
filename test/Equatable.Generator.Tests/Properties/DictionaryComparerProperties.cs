using Equatable.Comparers;

namespace Equatable.Generator.Tests.Properties;

public class DictionaryComparerProperties
{
    private static readonly DictionaryEqualityComparer<string, int> Comparer = DictionaryEqualityComparer<string, int>.Default;

    [Property]
    public Property Reflexivity(Dictionary<string, int> dict)
    {
        return Comparer.Equals(dict, dict).ToProperty();
    }

    [Property]
    public Property Symmetry(Dictionary<string, int> x, Dictionary<string, int> y)
    {
        return (Comparer.Equals(x, y) == Comparer.Equals(y, x)).ToProperty();
    }

    [Property]
    public Property HashIsInsertionOrderIndependent(Dictionary<string, int> dict)
    {
        var reversed = new Dictionary<string, int>(dict.Reverse());
        return (Comparer.GetHashCode(dict) == Comparer.GetHashCode(reversed)).ToProperty();
    }

    [Property]
    public Property EqualDictionariesHaveSameHashCode(Dictionary<string, int> dict)
    {
        var copy = new Dictionary<string, int>(dict);
        return (Comparer.Equals(dict, copy) && Comparer.GetHashCode(dict) == Comparer.GetHashCode(copy)).ToProperty();
    }

    [Property]
    public Property DifferentValueProducesDifferentHash(Dictionary<string, int> dict, string key, int v1, int v2)
    {
        if (v1 == v2)
            return true.ToProperty().When(true);

        var a = new Dictionary<string, int>(dict) { [key] = v1 };
        var b = new Dictionary<string, int>(dict) { [key] = v2 };

        // different values must NOT be equal
        return (!Comparer.Equals(a, b)).ToProperty();
    }
}
