using Equatable.Entities;

namespace Equatable.Generator.Tests.Properties;

/// <summary>
/// Property-based tests for auto-composed nested collection comparers.
/// Covers all meaningful 2-level and 3-level combinations of Dict / List / HashSet.
/// Convention per shape:
///   - Dict outer → insertion order must not matter
///   - List outer → insertion order MUST matter
///   - HashSet outer → element order must not matter
///   - List/Sequence inner → element order matters
///   - Dict/HashSet inner → element order does not matter
/// </summary>
[Properties(Arbitrary = new[] { typeof(Arbitraries) })]
public class NestedCollectionsProperties
{
    // ══════════════════════════════════════════════════════════════════════
    // 2-level: Dict<K, List<V>>
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property DictOfLists_EqualWhenSameContent(Dictionary<string, List<int>> raw)
    {
        var a = new NestedCollections { DictOfLists = raw };
        var b = new NestedCollections { DictOfLists = raw.ToDictionary(kv => kv.Key, kv => new List<int>(kv.Value)) };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property DictOfLists_OuterInsertionOrderDoesNotMatter(Dictionary<string, List<int>> raw)
    {
        var a = new NestedCollections { DictOfLists = raw };
        var b = new NestedCollections { DictOfLists = raw.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value) };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property DictOfLists_HashIsInsertionOrderIndependent(Dictionary<string, List<int>> raw)
    {
        var a = new NestedCollections { DictOfLists = raw };
        var b = new NestedCollections { DictOfLists = raw.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value) };
        return Prop.ToProperty((a.GetHashCode() == b.GetHashCode()));
    }

    [Property]
    public Property DictOfLists_InnerOrderMatters(string key, int v1, int v2)
    {
        if (v1 == v2) return Prop.When(true, true);
        var a = new NestedCollections { DictOfLists = new Dictionary<string, List<int>> { [key] = [v1, v2] } };
        var b = new NestedCollections { DictOfLists = new Dictionary<string, List<int>> { [key] = [v2, v1] } };
        return Prop.ToProperty((!a.Equals(b)));
    }

    [Property]
    public Property DictOfLists_EqualImpliesSameHash(Dictionary<string, List<int>> raw)
    {
        var a = new NestedCollections { DictOfLists = raw };
        var b = new NestedCollections { DictOfLists = raw.ToDictionary(kv => kv.Key, kv => new List<int>(kv.Value)) };
        return Prop.ToProperty((a.Equals(b) && a.GetHashCode() == b.GetHashCode()));
    }

    // ══════════════════════════════════════════════════════════════════════
    // 2-level: Dict<K, HashSet<V>>
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property DictOfSets_EqualWhenSameContent(Dictionary<string, HashSet<int>> raw)
    {
        var a = new NestedCollections { DictOfSets = raw };
        var b = new NestedCollections { DictOfSets = raw.ToDictionary(kv => kv.Key, kv => new HashSet<int>(kv.Value)) };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property DictOfSets_OuterInsertionOrderDoesNotMatter(Dictionary<string, HashSet<int>> raw)
    {
        var a = new NestedCollections { DictOfSets = raw };
        var b = new NestedCollections { DictOfSets = raw.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value) };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property DictOfSets_InnerOrderDoesNotMatter(string key, int v1, int v2)
    {
        // HashSet — insertion order must not matter (even if values differ)
        var s1 = new HashSet<int> { v1, v2 };
        var s2 = new HashSet<int> { v2, v1 };
        var a = new NestedCollections { DictOfSets = new Dictionary<string, HashSet<int>> { [key] = s1 } };
        var b = new NestedCollections { DictOfSets = new Dictionary<string, HashSet<int>> { [key] = s2 } };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property DictOfSets_HashIsInsertionOrderIndependent(Dictionary<string, HashSet<int>> raw)
    {
        var a = new NestedCollections { DictOfSets = raw };
        var b = new NestedCollections { DictOfSets = raw.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value) };
        return Prop.ToProperty((a.GetHashCode() == b.GetHashCode()));
    }

    // ══════════════════════════════════════════════════════════════════════
    // 2-level: Dict<K, Dict<K2, V>>
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property DictOfDicts_EqualWhenSameContent(Dictionary<string, Dictionary<string, int>> raw)
    {
        var a = new NestedCollections { DictOfDicts = raw };
        var b = new NestedCollections { DictOfDicts = raw.ToDictionary(kv => kv.Key, kv => new Dictionary<string, int>(kv.Value)) };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property DictOfDicts_OuterInsertionOrderDoesNotMatter(Dictionary<string, Dictionary<string, int>> raw)
    {
        var a = new NestedCollections { DictOfDicts = raw };
        var b = new NestedCollections { DictOfDicts = raw.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value) };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property DictOfDicts_InnerInsertionOrderDoesNotMatter(Dictionary<string, Dictionary<string, int>> raw)
    {
        var reversed = raw.ToDictionary(kv => kv.Key, kv => kv.Value.Reverse().ToDictionary(p => p.Key, p => p.Value));
        var a = new NestedCollections { DictOfDicts = raw };
        var b = new NestedCollections { DictOfDicts = reversed };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property DictOfDicts_EqualImpliesSameHash(Dictionary<string, Dictionary<string, int>> raw)
    {
        var a = new NestedCollections { DictOfDicts = raw };
        var b = new NestedCollections { DictOfDicts = raw.ToDictionary(kv => kv.Key, kv => new Dictionary<string, int>(kv.Value)) };
        return Prop.ToProperty((a.Equals(b) && a.GetHashCode() == b.GetHashCode()));
    }

    // ══════════════════════════════════════════════════════════════════════
    // 2-level: List<Dict<K,V>>
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property ListOfDicts_EqualWhenSameContent(List<Dictionary<string, int>> items)
    {
        var a = new NestedCollections { ListOfDicts = items };
        var b = new NestedCollections { ListOfDicts = items.Select(d => new Dictionary<string, int>(d)).ToList() };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property ListOfDicts_InnerInsertionOrderDoesNotMatter(List<Dictionary<string, int>> items)
    {
        var reversed = items.Select(d => d.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value)).ToList();
        var a = new NestedCollections { ListOfDicts = items };
        var b = new NestedCollections { ListOfDicts = reversed };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property ListOfDicts_OuterOrderMatters(Dictionary<string, int> d1, Dictionary<string, int> d2)
    {
        if (d1.SequenceEqual(d2)) return Prop.When(true, true);
        var a = new NestedCollections { ListOfDicts = [d1, d2] };
        var b = new NestedCollections { ListOfDicts = [d2, d1] };
        return Prop.ToProperty((!a.Equals(b)));
    }

    [Property]
    public Property ListOfDicts_EqualImpliesSameHash(List<Dictionary<string, int>> items)
    {
        var a = new NestedCollections { ListOfDicts = items };
        var b = new NestedCollections { ListOfDicts = items.Select(d => new Dictionary<string, int>(d)).ToList() };
        return Prop.ToProperty((a.Equals(b) && a.GetHashCode() == b.GetHashCode()));
    }

    // ══════════════════════════════════════════════════════════════════════
    // 2-level: List<HashSet<V>>
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property ListOfSets_EqualWhenSameContent(List<HashSet<int>> items)
    {
        var a = new NestedCollections { ListOfSets = items };
        var b = new NestedCollections { ListOfSets = items.Select(s => new HashSet<int>(s)).ToList() };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property ListOfSets_InnerOrderDoesNotMatter(List<HashSet<int>> items)
    {
        var a = new NestedCollections { ListOfSets = items };
        var b = new NestedCollections { ListOfSets = items.Select(s => new HashSet<int>(s.Reverse())).ToList() };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property ListOfSets_OuterOrderMatters(HashSet<int> s1, HashSet<int> s2)
    {
        // Two distinct non-equal sets — swapping them must break equality
        if (s1.SetEquals(s2)) return Prop.When(true, true);
        var a = new NestedCollections { ListOfSets = [s1, s2] };
        var b = new NestedCollections { ListOfSets = [s2, s1] };
        return Prop.ToProperty((!a.Equals(b)));
    }

    // ══════════════════════════════════════════════════════════════════════
    // 2-level: List<List<V>>
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property ListOfLists_EqualWhenSameContent(List<List<int>> items)
    {
        var a = new NestedCollections { ListOfLists = items };
        var b = new NestedCollections { ListOfLists = items.Select(l => new List<int>(l)).ToList() };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property ListOfLists_OuterOrderMatters(List<int> l1, List<int> l2)
    {
        if (l1.SequenceEqual(l2)) return Prop.When(true, true);
        var a = new NestedCollections { ListOfLists = [l1, l2] };
        var b = new NestedCollections { ListOfLists = [l2, l1] };
        return Prop.ToProperty((!a.Equals(b)));
    }

    [Property]
    public Property ListOfLists_InnerOrderMatters(string outerTag, int v1, int v2)
    {
        // inner lists are order-sensitive
        if (v1 == v2) return Prop.When(true, true);
        var a = new NestedCollections { ListOfLists = [[v1, v2]] };
        var b = new NestedCollections { ListOfLists = [[v2, v1]] };
        return Prop.ToProperty((!a.Equals(b)));
    }

    [Property]
    public Property ListOfLists_EqualImpliesSameHash(List<List<int>> items)
    {
        var a = new NestedCollections { ListOfLists = items };
        var b = new NestedCollections { ListOfLists = items.Select(l => new List<int>(l)).ToList() };
        return Prop.ToProperty((a.Equals(b) && a.GetHashCode() == b.GetHashCode()));
    }

    // ══════════════════════════════════════════════════════════════════════
    // 2-level: HashSet<List<V>>
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property SetOfLists_EqualWhenSameReferences(List<List<int>> items)
    {
        // HashSet<List<int>> uses reference equality for List<int> elements — same refs must be equal
        var a = new NestedCollections { SetOfLists = new HashSet<List<int>>(items) };
        var b = new NestedCollections { SetOfLists = new HashSet<List<int>>(((IEnumerable<List<int>>)items).Reverse()) };
        return Prop.ToProperty(a.Equals(b));
    }

    // ══════════════════════════════════════════════════════════════════════
    // 2-level: HashSet<Dict<K,V>>
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property SetOfDicts_EqualWhenSameReferences(List<Dictionary<string, int>> items)
    {
        // HashSet<Dictionary<string,int>> uses reference equality for dict elements — same refs must be equal
        var a = new NestedCollections { SetOfDicts = new HashSet<Dictionary<string, int>>(items) };
        var b = new NestedCollections { SetOfDicts = new HashSet<Dictionary<string, int>>(((IEnumerable<Dictionary<string, int>>)items).Reverse()) };
        return Prop.ToProperty(a.Equals(b));
    }

    // ══════════════════════════════════════════════════════════════════════
    // 3-level: Dict<K, Dict<K2, List<V>>>
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property ThreeLevelNested_EqualWhenSameContent(Dictionary<string, Dictionary<string, List<int>>> raw)
    {
        var copy = raw.ToDictionary(o => o.Key, o => o.Value.ToDictionary(i => i.Key, i => new List<int>(i.Value)));
        var a = new NestedCollections { ThreeLevelNested = raw };
        var b = new NestedCollections { ThreeLevelNested = copy };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property ThreeLevelNested_OuterInsertionOrderDoesNotMatter(Dictionary<string, Dictionary<string, List<int>>> raw)
    {
        var a = new NestedCollections { ThreeLevelNested = raw };
        var b = new NestedCollections { ThreeLevelNested = raw.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value) };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property ThreeLevelNested_MiddleInsertionOrderDoesNotMatter(Dictionary<string, Dictionary<string, List<int>>> raw)
    {
        var reversed = raw.ToDictionary(o => o.Key, o => o.Value.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value));
        var a = new NestedCollections { ThreeLevelNested = raw };
        var b = new NestedCollections { ThreeLevelNested = reversed };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property ThreeLevelNested_InnermostOrderMatters(string outerKey, string innerKey, int v1, int v2)
    {
        if (v1 == v2) return Prop.When(true, true);
        var a = new NestedCollections
        {
            ThreeLevelNested = new Dictionary<string, Dictionary<string, List<int>>>
            {
                [outerKey] = new Dictionary<string, List<int>> { [innerKey] = [v1, v2] }
            }
        };
        var b = new NestedCollections
        {
            ThreeLevelNested = new Dictionary<string, Dictionary<string, List<int>>>
            {
                [outerKey] = new Dictionary<string, List<int>> { [innerKey] = [v2, v1] }
            }
        };
        return Prop.ToProperty((!a.Equals(b)));
    }

    [Property]
    public Property ThreeLevelNested_EqualImpliesSameHash(Dictionary<string, Dictionary<string, List<int>>> raw)
    {
        var copy = raw.ToDictionary(o => o.Key, o => o.Value.ToDictionary(i => i.Key, i => new List<int>(i.Value)));
        var a = new NestedCollections { ThreeLevelNested = raw };
        var b = new NestedCollections { ThreeLevelNested = copy };
        return Prop.ToProperty((a.Equals(b) && a.GetHashCode() == b.GetHashCode()));
    }

    // ══════════════════════════════════════════════════════════════════════
    // 3-level: Dict<K, List<HashSet<V>>>
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property DictOfListOfSets_EqualWhenSameContent(Dictionary<string, List<HashSet<int>>> raw)
    {
        var copy = raw.ToDictionary(kv => kv.Key, kv => kv.Value.Select(s => new HashSet<int>(s)).ToList());
        var a = new NestedCollections { DictOfListOfSets = raw };
        var b = new NestedCollections { DictOfListOfSets = copy };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property DictOfListOfSets_OuterInsertionOrderDoesNotMatter(Dictionary<string, List<HashSet<int>>> raw)
    {
        var a = new NestedCollections { DictOfListOfSets = raw };
        var b = new NestedCollections { DictOfListOfSets = raw.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value) };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property DictOfListOfSets_MiddleOrderMatters(string key, HashSet<int> s1, HashSet<int> s2)
    {
        // middle is List — position matters
        if (s1.SetEquals(s2)) return Prop.When(true, true);
        var a = new NestedCollections { DictOfListOfSets = new Dictionary<string, List<HashSet<int>>> { [key] = [s1, s2] } };
        var b = new NestedCollections { DictOfListOfSets = new Dictionary<string, List<HashSet<int>>> { [key] = [s2, s1] } };
        return Prop.ToProperty((!a.Equals(b)));
    }

    [Property]
    public Property DictOfListOfSets_InnermostOrderDoesNotMatter(string key, int v1, int v2)
    {
        // innermost is HashSet — order must not matter
        var a = new NestedCollections
        {
            DictOfListOfSets = new Dictionary<string, List<HashSet<int>>>
            { [key] = [new HashSet<int> { v1, v2 }] }
        };
        var b = new NestedCollections
        {
            DictOfListOfSets = new Dictionary<string, List<HashSet<int>>>
            { [key] = [new HashSet<int> { v2, v1 }] }
        };
        return Prop.ToProperty(a.Equals(b));
    }

    // ══════════════════════════════════════════════════════════════════════
    // 3-level: Dict<K, List<Dict<K2, V>>>
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property DictOfListOfDicts_EqualWhenSameContent(Dictionary<string, List<Dictionary<string, int>>> raw)
    {
        var copy = raw.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.Select(d => new Dictionary<string, int>(d)).ToList());
        var a = new NestedCollections { DictOfListOfDicts = raw };
        var b = new NestedCollections { DictOfListOfDicts = copy };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property DictOfListOfDicts_OuterInsertionOrderDoesNotMatter(Dictionary<string, List<Dictionary<string, int>>> raw)
    {
        var a = new NestedCollections { DictOfListOfDicts = raw };
        var b = new NestedCollections { DictOfListOfDicts = raw.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value) };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property DictOfListOfDicts_MiddleOrderMatters(string key, Dictionary<string, int> d1, Dictionary<string, int> d2)
    {
        // middle is List — position matters
        if (d1.SequenceEqual(d2)) return Prop.When(true, true);
        var a = new NestedCollections { DictOfListOfDicts = new Dictionary<string, List<Dictionary<string, int>>> { [key] = [d1, d2] } };
        var b = new NestedCollections { DictOfListOfDicts = new Dictionary<string, List<Dictionary<string, int>>> { [key] = [d2, d1] } };
        return Prop.ToProperty((!a.Equals(b)));
    }

    [Property]
    public Property DictOfListOfDicts_InnermostInsertionOrderDoesNotMatter(Dictionary<string, List<Dictionary<string, int>>> raw)
    {
        var copy = raw.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.Select(d => d.Reverse().ToDictionary(p => p.Key, p => p.Value)).ToList());
        var a = new NestedCollections { DictOfListOfDicts = raw };
        var b = new NestedCollections { DictOfListOfDicts = copy };
        return Prop.ToProperty(a.Equals(b));
    }

    // ══════════════════════════════════════════════════════════════════════
    // 3-level: List<Dict<K, List<V>>>
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property ListOfDictOfLists_EqualWhenSameContent(List<Dictionary<string, List<int>>> items)
    {
        var copy = items.Select(d => d.ToDictionary(kv => kv.Key, kv => new List<int>(kv.Value))).ToList();
        var a = new NestedCollections { ListOfDictOfLists = items };
        var b = new NestedCollections { ListOfDictOfLists = copy };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property ListOfDictOfLists_OuterOrderMatters(Dictionary<string, List<int>> d1, Dictionary<string, List<int>> d2)
    {
        // outer is List — position matters
        Func<Dictionary<string, List<int>>, Dictionary<string, List<int>>, bool> sameContent =
            (x, y) => x.Count == y.Count && x.All(kv => y.TryGetValue(kv.Key, out var v) && kv.Value.SequenceEqual(v));

        if (sameContent(d1, d2)) return Prop.When(true, true);
        var a = new NestedCollections { ListOfDictOfLists = [d1, d2] };
        var b = new NestedCollections { ListOfDictOfLists = [d2, d1] };
        return Prop.ToProperty((!a.Equals(b)));
    }

    [Property]
    public Property ListOfDictOfLists_MiddleInsertionOrderDoesNotMatter(List<Dictionary<string, List<int>>> items)
    {
        var reversed = items.Select(d => d.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value)).ToList();
        var a = new NestedCollections { ListOfDictOfLists = items };
        var b = new NestedCollections { ListOfDictOfLists = reversed };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property ListOfDictOfLists_InnermostOrderMatters(string key, int v1, int v2)
    {
        // innermost is List — position matters
        if (v1 == v2) return Prop.When(true, true);
        var a = new NestedCollections { ListOfDictOfLists = [new Dictionary<string, List<int>> { [key] = [v1, v2] }] };
        var b = new NestedCollections { ListOfDictOfLists = [new Dictionary<string, List<int>> { [key] = [v2, v1] }] };
        return Prop.ToProperty((!a.Equals(b)));
    }

    // ══════════════════════════════════════════════════════════════════════
    // 3-level: List<Dict<K, HashSet<V>>>
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property ListOfDictOfSets_EqualWhenSameContent(List<Dictionary<string, HashSet<int>>> items)
    {
        var copy = items.Select(d => d.ToDictionary(kv => kv.Key, kv => new HashSet<int>(kv.Value))).ToList();
        var a = new NestedCollections { ListOfDictOfSets = items };
        var b = new NestedCollections { ListOfDictOfSets = copy };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property ListOfDictOfSets_OuterOrderMatters(Dictionary<string, HashSet<int>> d1, Dictionary<string, HashSet<int>> d2)
    {
        Func<Dictionary<string, HashSet<int>>, Dictionary<string, HashSet<int>>, bool> sameContent =
            (x, y) => x.Count == y.Count && x.All(kv => y.TryGetValue(kv.Key, out var v) && kv.Value.SetEquals(v));

        if (sameContent(d1, d2)) return Prop.When(true, true);
        var a = new NestedCollections { ListOfDictOfSets = [d1, d2] };
        var b = new NestedCollections { ListOfDictOfSets = [d2, d1] };
        return Prop.ToProperty((!a.Equals(b)));
    }

    [Property]
    public Property ListOfDictOfSets_MiddleInsertionOrderDoesNotMatter(List<Dictionary<string, HashSet<int>>> items)
    {
        var reversed = items.Select(d => d.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value)).ToList();
        var a = new NestedCollections { ListOfDictOfSets = items };
        var b = new NestedCollections { ListOfDictOfSets = reversed };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property ListOfDictOfSets_InnermostOrderDoesNotMatter(string key, int v1, int v2)
    {
        // innermost is HashSet — insertion order must not matter
        var a = new NestedCollections
        {
            ListOfDictOfSets = [new Dictionary<string, HashSet<int>> { [key] = new HashSet<int> { v1, v2 } }]
        };
        var b = new NestedCollections
        {
            ListOfDictOfSets = [new Dictionary<string, HashSet<int>> { [key] = new HashSet<int> { v2, v1 } }]
        };
        return Prop.ToProperty(a.Equals(b));
    }

    // ══════════════════════════════════════════════════════════════════════
    // 3-level: List<List<Dict<K, V>>>
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property ListOfListOfDicts_EqualWhenSameContent(List<List<Dictionary<string, int>>> items)
    {
        var copy = items.Select(l => l.Select(d => new Dictionary<string, int>(d)).ToList()).ToList();
        var a = new NestedCollections { ListOfListOfDicts = items };
        var b = new NestedCollections { ListOfListOfDicts = copy };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property ListOfListOfDicts_OuterOrderMatters(List<Dictionary<string, int>> l1, List<Dictionary<string, int>> l2)
    {
        Func<List<Dictionary<string, int>>, List<Dictionary<string, int>>, bool> sameContent =
            (x, y) => x.Count == y.Count &&
                       x.Zip(y).All(pair => pair.First.SequenceEqual(pair.Second));

        if (sameContent(l1, l2)) return Prop.When(true, true);
        var a = new NestedCollections { ListOfListOfDicts = [l1, l2] };
        var b = new NestedCollections { ListOfListOfDicts = [l2, l1] };
        return Prop.ToProperty((!a.Equals(b)));
    }

    [Property]
    public Property ListOfListOfDicts_MiddleOrderMatters(Dictionary<string, int> d1, Dictionary<string, int> d2)
    {
        // middle is also List — position matters
        if (d1.SequenceEqual(d2)) return Prop.When(true, true);
        var a = new NestedCollections { ListOfListOfDicts = [[d1, d2]] };
        var b = new NestedCollections { ListOfListOfDicts = [[d2, d1]] };
        return Prop.ToProperty((!a.Equals(b)));
    }

    [Property]
    public Property ListOfListOfDicts_InnermostInsertionOrderDoesNotMatter(List<List<Dictionary<string, int>>> items)
    {
        var copy = items.Select(l => l.Select(d => d.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value)).ToList()).ToList();
        var a = new NestedCollections { ListOfListOfDicts = items };
        var b = new NestedCollections { ListOfListOfDicts = copy };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property ListOfListOfDicts_EqualImpliesSameHash(List<List<Dictionary<string, int>>> items)
    {
        var copy = items.Select(l => l.Select(d => new Dictionary<string, int>(d)).ToList()).ToList();
        var a = new NestedCollections { ListOfListOfDicts = items };
        var b = new NestedCollections { ListOfListOfDicts = copy };
        return Prop.ToProperty((a.Equals(b) && a.GetHashCode() == b.GetHashCode()));
    }

    // ══════════════════════════════════════════════════════════════════════
    // Arrays: int[]
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property FlatArray_EqualWhenSameContent(int[] arr)
    {
        var a = new NestedCollections { FlatArray = arr };
        var b = new NestedCollections { FlatArray = (int[])arr.Clone() };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property FlatArray_OrderMatters(int v1, int v2)
    {
        if (v1 == v2) return Prop.When(true, true);
        var a = new NestedCollections { FlatArray = [v1, v2] };
        var b = new NestedCollections { FlatArray = [v2, v1] };
        return Prop.ToProperty((!a.Equals(b)));
    }

    [Property]
    public Property FlatArray_EqualImpliesSameHash(int[] arr)
    {
        var a = new NestedCollections { FlatArray = arr };
        var b = new NestedCollections { FlatArray = (int[])arr.Clone() };
        return Prop.ToProperty((a.Equals(b) && a.GetHashCode() == b.GetHashCode()));
    }

    // ══════════════════════════════════════════════════════════════════════
    // Arrays: int[][] (array of arrays)
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property ArrayOfArrays_EqualWhenSameContent(int[][] arr)
    {
        var a = new NestedCollections { ArrayOfArrays = arr };
        var b = new NestedCollections { ArrayOfArrays = arr.Select(inner => (int[])inner.Clone()).ToArray() };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property ArrayOfArrays_OuterOrderMatters(int[] inner1, int[] inner2)
    {
        if (inner1.SequenceEqual(inner2)) return Prop.When(true, true);
        var a = new NestedCollections { ArrayOfArrays = [inner1, inner2] };
        var b = new NestedCollections { ArrayOfArrays = [inner2, inner1] };
        return Prop.ToProperty((!a.Equals(b)));
    }

    [Property]
    public Property ArrayOfArrays_InnerOrderMatters(int v1, int v2)
    {
        if (v1 == v2) return Prop.When(true, true);
        var a = new NestedCollections { ArrayOfArrays = [[v1, v2]] };
        var b = new NestedCollections { ArrayOfArrays = [[v2, v1]] };
        return Prop.ToProperty((!a.Equals(b)));
    }

    // ══════════════════════════════════════════════════════════════════════
    // Arrays: Dictionary<string, int>[] (array of dicts)
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property ArrayOfDicts_EqualWhenSameContent(Dictionary<string, int>[] arr)
    {
        var a = new NestedCollections { ArrayOfDicts = arr };
        var b = new NestedCollections { ArrayOfDicts = arr.Select(d => new Dictionary<string, int>(d)).ToArray() };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property ArrayOfDicts_OuterOrderMatters(Dictionary<string, int> d1, Dictionary<string, int> d2)
    {
        if (d1.SequenceEqual(d2)) return Prop.When(true, true);
        var a = new NestedCollections { ArrayOfDicts = [d1, d2] };
        var b = new NestedCollections { ArrayOfDicts = [d2, d1] };
        return Prop.ToProperty((!a.Equals(b)));
    }

    [Property]
    public Property ArrayOfDicts_InnerInsertionOrderDoesNotMatter(Dictionary<string, int>[] arr)
    {
        var a = new NestedCollections { ArrayOfDicts = arr };
        var b = new NestedCollections { ArrayOfDicts = arr.Select(d => d.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value)).ToArray() };
        return Prop.ToProperty(a.Equals(b));
    }

    // ══════════════════════════════════════════════════════════════════════
    // Arrays: Dictionary<string, int[]> (dict of arrays)
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property DictOfArrays_EqualWhenSameContent(Dictionary<string, int[]> raw)
    {
        var a = new NestedCollections { DictOfArrays = raw };
        var b = new NestedCollections { DictOfArrays = raw.ToDictionary(kv => kv.Key, kv => (int[])kv.Value.Clone()) };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property DictOfArrays_OuterInsertionOrderDoesNotMatter(Dictionary<string, int[]> raw)
    {
        var a = new NestedCollections { DictOfArrays = raw };
        var b = new NestedCollections { DictOfArrays = raw.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value) };
        return Prop.ToProperty(a.Equals(b));
    }

    [Property]
    public Property DictOfArrays_InnerOrderMatters(string key, int v1, int v2)
    {
        if (v1 == v2) return Prop.When(true, true);
        var a = new NestedCollections { DictOfArrays = new Dictionary<string, int[]> { [key] = [v1, v2] } };
        var b = new NestedCollections { DictOfArrays = new Dictionary<string, int[]> { [key] = [v2, v1] } };
        return Prop.ToProperty((!a.Equals(b)));
    }

    [Property]
    public Property DictOfArrays_EqualImpliesSameHash(Dictionary<string, int[]> raw)
    {
        var a = new NestedCollections { DictOfArrays = raw };
        var b = new NestedCollections { DictOfArrays = raw.ToDictionary(kv => kv.Key, kv => (int[])kv.Value.Clone()) };
        return Prop.ToProperty((a.Equals(b) && a.GetHashCode() == b.GetHashCode()));
    }
}
