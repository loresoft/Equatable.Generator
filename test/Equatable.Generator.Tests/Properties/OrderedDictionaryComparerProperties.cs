using System.Collections.Generic;
using System.Linq;
using Equatable.Comparers;

namespace Equatable.Generator.Tests.Properties;

public class OrderedDictionaryComparerProperties
{
    private static readonly OrderedDictionaryEqualityComparer<string, int> Comparer
        = OrderedDictionaryEqualityComparer<string, int>.Default;

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
        // equal → same hash (one direction only — hash collisions can make unequal produce same hash)
        var reversed = new Dictionary<string, int>(dict.Reverse());
        return Prop.When(Comparer.Equals(dict, reversed), Comparer.GetHashCode(dict) == Comparer.GetHashCode(reversed));
    }

    [Property]
    public Property Equals_DifferentValue_ReturnsFalse(Dictionary<string, int> dict, string key, int v1, int v2)
    {
        if (v1 == v2)
            return Prop.When(true, true);

        var a = new Dictionary<string, int>(dict) { [key] = v1 };
        var b = new Dictionary<string, int>(dict) { [key] = v2 };

        return Prop.ToProperty(!Comparer.Equals(a, b));
    }
}
