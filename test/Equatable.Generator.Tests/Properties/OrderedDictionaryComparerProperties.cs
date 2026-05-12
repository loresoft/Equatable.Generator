using System.Collections.Generic;
using System.Linq;
using Equatable.Comparers;

namespace Equatable.Generator.Tests.Properties;

public class OrderedDictionaryComparerProperties
{
    private static readonly OrderedDictionaryEqualityComparer<string, int> Comparer
        = OrderedDictionaryEqualityComparer<string, int>.Default;

    [Property]
    public Property Reflexivity(Dictionary<string, int> dict)
    {
        return Prop.ToProperty(Comparer.Equals(dict, dict));
    }

    [Property]
    public Property Symmetry(Dictionary<string, int> x, Dictionary<string, int> y)
    {
        return Prop.ToProperty(Comparer.Equals(x, y) == Comparer.Equals(y, x));
    }

    [Property]
    public Property HashIsInsertionOrderIndependent(Dictionary<string, int> dict)
    {
        var reversed = new Dictionary<string, int>(dict.Reverse());
        return Prop.ToProperty(Comparer.GetHashCode(dict) == Comparer.GetHashCode(reversed));
    }

    [Property]
    public Property EqualImpliesSameHashCode(Dictionary<string, int> dict)
    {
        var reversed = new Dictionary<string, int>(dict.Reverse());
        return Prop.ToProperty(Comparer.Equals(dict, reversed) && Comparer.GetHashCode(dict) == Comparer.GetHashCode(reversed));
    }

    [Property]
    public Property DifferentValueMakesNotEqual(Dictionary<string, int> dict, string key, int v1, int v2)
    {
        if (v1 == v2)
            return Prop.When(true, true);

        var a = new Dictionary<string, int>(dict) { [key] = v1 };
        var b = new Dictionary<string, int>(dict) { [key] = v2 };

        return Prop.ToProperty(!Comparer.Equals(a, b));
    }
}
