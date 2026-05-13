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
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property DictOfLists_OuterInsertionOrderDoesNotMatter(Dictionary<string, List<int>> raw)
    {
        var a = new NestedCollections { DictOfLists = raw };
        var b = new NestedCollections { DictOfLists = raw.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value) };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property DictOfLists_HashIsInsertionOrderIndependent(Dictionary<string, List<int>> raw)
    {
        var a = new NestedCollections { DictOfLists = raw };
        var b = new NestedCollections { DictOfLists = raw.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value) };
        return (a.GetHashCode() == b.GetHashCode()).ToProperty();
    }

    [Property]
    public Property DictOfLists_InnerOrderMatters(string key, int v1, int v2)
    {
        if (key == null || v1 == v2) return true.ToProperty().When(true);
        var a = new NestedCollections { DictOfLists = new Dictionary<string, List<int>> { [key] = [v1, v2] } };
        var b = new NestedCollections { DictOfLists = new Dictionary<string, List<int>> { [key] = [v2, v1] } };
        return (!a.Equals(b)).ToProperty();
    }

    [Property]
    public Property DictOfLists_EqualImpliesSameHash(Dictionary<string, List<int>> raw)
    {
        var a = new NestedCollections { DictOfLists = raw };
        var b = new NestedCollections { DictOfLists = raw.ToDictionary(kv => kv.Key, kv => new List<int>(kv.Value)) };
        return (a.Equals(b) && a.GetHashCode() == b.GetHashCode()).ToProperty();
    }

    // ══════════════════════════════════════════════════════════════════════
    // 2-level: Dict<K, HashSet<V>>
    // ══════════════════════════════════════════════════════════════════════

    // FsCheck cannot generate HashSet<T>; use Dictionary<string, int[]> and convert values.

    [Property]
    public Property DictOfSets_EqualWhenSameContent(Dictionary<string, int[]> raw)
    {
        var a = new NestedCollections { DictOfSets = raw.ToDictionary(kv => kv.Key, kv => new HashSet<int>(kv.Value)) };
        var b = new NestedCollections { DictOfSets = raw.ToDictionary(kv => kv.Key, kv => new HashSet<int>(kv.Value)) };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property DictOfSets_OuterInsertionOrderDoesNotMatter(Dictionary<string, int[]> raw)
    {
        var sets = raw.ToDictionary(kv => kv.Key, kv => new HashSet<int>(kv.Value));
        var a = new NestedCollections { DictOfSets = sets };
        var b = new NestedCollections { DictOfSets = sets.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value) };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property DictOfSets_InnerOrderDoesNotMatter(string key, int v1, int v2)
    {
        if (key == null) return true.ToProperty().When(true);
        // HashSet — insertion order must not matter (even if values differ)
        var s1 = new HashSet<int> { v1, v2 };
        var s2 = new HashSet<int> { v2, v1 };
        var a = new NestedCollections { DictOfSets = new Dictionary<string, HashSet<int>> { [key] = s1 } };
        var b = new NestedCollections { DictOfSets = new Dictionary<string, HashSet<int>> { [key] = s2 } };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property DictOfSets_HashIsInsertionOrderIndependent(Dictionary<string, int[]> raw)
    {
        var sets = raw.ToDictionary(kv => kv.Key, kv => new HashSet<int>(kv.Value));
        var a = new NestedCollections { DictOfSets = sets };
        var b = new NestedCollections { DictOfSets = sets.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value) };
        return (a.GetHashCode() == b.GetHashCode()).ToProperty();
    }

    // ══════════════════════════════════════════════════════════════════════
    // 2-level: Dict<K, Dict<K2, V>>
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property DictOfDicts_EqualWhenSameContent(Dictionary<string, Dictionary<string, int>> raw)
    {
        var a = new NestedCollections { DictOfDicts = raw };
        var b = new NestedCollections { DictOfDicts = raw.ToDictionary(kv => kv.Key, kv => new Dictionary<string, int>(kv.Value)) };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property DictOfDicts_OuterInsertionOrderDoesNotMatter(Dictionary<string, Dictionary<string, int>> raw)
    {
        var a = new NestedCollections { DictOfDicts = raw };
        var b = new NestedCollections { DictOfDicts = raw.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value) };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property DictOfDicts_InnerInsertionOrderDoesNotMatter(Dictionary<string, Dictionary<string, int>> raw)
    {
        var reversed = raw.ToDictionary(kv => kv.Key, kv => kv.Value.Reverse().ToDictionary(p => p.Key, p => p.Value));
        var a = new NestedCollections { DictOfDicts = raw };
        var b = new NestedCollections { DictOfDicts = reversed };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property DictOfDicts_EqualImpliesSameHash(Dictionary<string, Dictionary<string, int>> raw)
    {
        var a = new NestedCollections { DictOfDicts = raw };
        var b = new NestedCollections { DictOfDicts = raw.ToDictionary(kv => kv.Key, kv => new Dictionary<string, int>(kv.Value)) };
        return (a.Equals(b) && a.GetHashCode() == b.GetHashCode()).ToProperty();
    }

    // ══════════════════════════════════════════════════════════════════════
    // 2-level: List<Dict<K,V>>
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property ListOfDicts_EqualWhenSameContent(List<Dictionary<string, int>> items)
    {
        var a = new NestedCollections { ListOfDicts = items };
        var b = new NestedCollections { ListOfDicts = items.Select(d => new Dictionary<string, int>(d)).ToList() };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property ListOfDicts_InnerInsertionOrderDoesNotMatter(List<Dictionary<string, int>> items)
    {
        var reversed = items.Select(d => d.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value)).ToList();
        var a = new NestedCollections { ListOfDicts = items };
        var b = new NestedCollections { ListOfDicts = reversed };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property ListOfDicts_OuterOrderMatters(Dictionary<string, int> d1, Dictionary<string, int> d2)
    {
        if (d1.SequenceEqual(d2)) return true.ToProperty().When(true);
        var a = new NestedCollections { ListOfDicts = [d1, d2] };
        var b = new NestedCollections { ListOfDicts = [d2, d1] };
        return (!a.Equals(b)).ToProperty();
    }

    [Property]
    public Property ListOfDicts_EqualImpliesSameHash(List<Dictionary<string, int>> items)
    {
        var a = new NestedCollections { ListOfDicts = items };
        var b = new NestedCollections { ListOfDicts = items.Select(d => new Dictionary<string, int>(d)).ToList() };
        return (a.Equals(b) && a.GetHashCode() == b.GetHashCode()).ToProperty();
    }

    // ══════════════════════════════════════════════════════════════════════
    // 2-level: List<HashSet<V>>
    // ══════════════════════════════════════════════════════════════════════

    // FsCheck cannot generate HashSet<T>; use int[][] and convert each inner array to HashSet<int>.

    [Property]
    public Property ListOfSets_EqualWhenSameContent(int[][] raw)
    {
        var a = new NestedCollections { ListOfSets = raw.Select(x => new HashSet<int>(x)).ToList() };
        var b = new NestedCollections { ListOfSets = raw.Select(x => new HashSet<int>(x)).ToList() };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property ListOfSets_InnerOrderMatters(int[][] raw)
    {
        // [SequenceEquality] is explicit: propagates to nested HashSet<int> → inner order is now significant.
        var a = new NestedCollections { ListOfSets = raw.Select(x => new HashSet<int>(x)).ToList() };
        var b = new NestedCollections { ListOfSets = raw.Select(x => new HashSet<int>(x.Reverse())).ToList() };
        // If any inner array has >1 distinct elements and their order changes, the sets differ.
        var anyReversalChangesOrder = raw.Any(x => x.Distinct().Count() > 1 && !x.SequenceEqual(x.Reverse()));
        return Prop.When(anyReversalChangesOrder, !a.Equals(b));
    }

    [Property]
    public Property ListOfSets_OuterOrderMatters(int[] xs, int[] ys)
    {
        var s1 = new HashSet<int>(xs);
        var s2 = new HashSet<int>(ys);
        // two distinct non-equal sets — swapping them must break equality
        if (s1.SetEquals(s2)) return true.ToProperty().When(true);
        var a = new NestedCollections { ListOfSets = [s1, s2] };
        var b = new NestedCollections { ListOfSets = [s2, s1] };
        return (!a.Equals(b)).ToProperty();
    }

    // ══════════════════════════════════════════════════════════════════════
    // 2-level: List<List<V>>
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property ListOfLists_EqualWhenSameContent(List<List<int>> items)
    {
        var a = new NestedCollections { ListOfLists = items };
        var b = new NestedCollections { ListOfLists = items.Select(l => new List<int>(l)).ToList() };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property ListOfLists_OuterOrderMatters(List<int> l1, List<int> l2)
    {
        if (l1.SequenceEqual(l2)) return true.ToProperty().When(true);
        var a = new NestedCollections { ListOfLists = [l1, l2] };
        var b = new NestedCollections { ListOfLists = [l2, l1] };
        return (!a.Equals(b)).ToProperty();
    }

    [Property]
    public Property ListOfLists_InnerOrderMatters(string outerTag, int v1, int v2)
    {
        // inner lists are order-sensitive
        if (v1 == v2) return true.ToProperty().When(true);
        var a = new NestedCollections { ListOfLists = [[v1, v2]] };
        var b = new NestedCollections { ListOfLists = [[v2, v1]] };
        return (!a.Equals(b)).ToProperty();
    }

    [Property]
    public Property ListOfLists_EqualImpliesSameHash(List<List<int>> items)
    {
        var a = new NestedCollections { ListOfLists = items };
        var b = new NestedCollections { ListOfLists = items.Select(l => new List<int>(l)).ToList() };
        return (a.Equals(b) && a.GetHashCode() == b.GetHashCode()).ToProperty();
    }

    // ══════════════════════════════════════════════════════════════════════
    // 2-level: HashSet<List<V>>
    // ══════════════════════════════════════════════════════════════════════

    // Note: HashSet<List<int>> uses reference equality for List<int> elements (List<int> does not implement
    // IEquatable). Two distinct List<int> instances with the same content are NOT equal in a HashSet.
    // The [HashSetEquality] property correctly reflects this: same-reference sets are equal.

    [Property]
    public Property SetOfLists_SameReferenceSetIsEqual(List<int> l1, List<int> l2)
    {
        var items = new List<List<int>> { l1, l2 };
        var set = new HashSet<List<int>>(items);
        var a = new NestedCollections { SetOfLists = set };
        var b = new NestedCollections { SetOfLists = set }; // same reference
        return a.Equals(b).ToProperty();
    }

    // ══════════════════════════════════════════════════════════════════════
    // 2-level: HashSet<Dict<K,V>>
    // ══════════════════════════════════════════════════════════════════════

    // Same caveat: Dictionary<string,int> uses reference equality inside a HashSet.

    [Property]
    public Property SetOfDicts_SameReferenceSetIsEqual(Dictionary<string, int> d1, Dictionary<string, int> d2)
    {
        var set = new HashSet<Dictionary<string, int>> { d1, d2 };
        var a = new NestedCollections { SetOfDicts = set };
        var b = new NestedCollections { SetOfDicts = set }; // same reference
        return a.Equals(b).ToProperty();
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
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property ThreeLevelNested_OuterInsertionOrderDoesNotMatter(Dictionary<string, Dictionary<string, List<int>>> raw)
    {
        var a = new NestedCollections { ThreeLevelNested = raw };
        var b = new NestedCollections { ThreeLevelNested = raw.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value) };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property ThreeLevelNested_MiddleInsertionOrderDoesNotMatter(Dictionary<string, Dictionary<string, List<int>>> raw)
    {
        var reversed = raw.ToDictionary(o => o.Key, o => o.Value.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value));
        var a = new NestedCollections { ThreeLevelNested = raw };
        var b = new NestedCollections { ThreeLevelNested = reversed };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property ThreeLevelNested_InnermostOrderMatters(string outerKey, string innerKey, int v1, int v2)
    {
        if (outerKey == null || innerKey == null || v1 == v2) return true.ToProperty().When(true);
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
        return (!a.Equals(b)).ToProperty();
    }

    [Property]
    public Property ThreeLevelNested_EqualImpliesSameHash(Dictionary<string, Dictionary<string, List<int>>> raw)
    {
        var copy = raw.ToDictionary(o => o.Key, o => o.Value.ToDictionary(i => i.Key, i => new List<int>(i.Value)));
        var a = new NestedCollections { ThreeLevelNested = raw };
        var b = new NestedCollections { ThreeLevelNested = copy };
        return (a.Equals(b) && a.GetHashCode() == b.GetHashCode()).ToProperty();
    }

    // ══════════════════════════════════════════════════════════════════════
    // 3-level: Dict<K, List<HashSet<V>>>
    // ══════════════════════════════════════════════════════════════════════

    // FsCheck cannot generate HashSet<T>; use Dictionary<string, int[][]> and convert.

    [Property]
    public Property DictOfListOfSets_EqualWhenSameContent(Dictionary<string, int[][]> raw)
    {
        var a = new NestedCollections { DictOfListOfSets = raw.ToDictionary(kv => kv.Key, kv => kv.Value.Select(s => new HashSet<int>(s)).ToList()) };
        var b = new NestedCollections { DictOfListOfSets = raw.ToDictionary(kv => kv.Key, kv => kv.Value.Select(s => new HashSet<int>(s)).ToList()) };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property DictOfListOfSets_OuterInsertionOrderDoesNotMatter(Dictionary<string, int[][]> raw)
    {
        var sets = raw.ToDictionary(kv => kv.Key, kv => kv.Value.Select(s => new HashSet<int>(s)).ToList());
        var a = new NestedCollections { DictOfListOfSets = sets };
        var b = new NestedCollections { DictOfListOfSets = sets.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value) };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property DictOfListOfSets_MiddleOrderMatters(string key, int[] xs, int[] ys)
    {
        if (key == null) return true.ToProperty().When(true);
        var s1 = new HashSet<int>(xs);
        var s2 = new HashSet<int>(ys);
        // middle is List — position matters
        if (s1.SetEquals(s2)) return true.ToProperty().When(true);
        var a = new NestedCollections { DictOfListOfSets = new Dictionary<string, List<HashSet<int>>> { [key] = [s1, s2] } };
        var b = new NestedCollections { DictOfListOfSets = new Dictionary<string, List<HashSet<int>>> { [key] = [s2, s1] } };
        return (!a.Equals(b)).ToProperty();
    }

    [Property]
    public Property DictOfListOfSets_InnermostOrderDoesNotMatter(string key, int v1, int v2)
    {
        if (key == null) return true.ToProperty().When(true);
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
        return a.Equals(b).ToProperty();
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
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property DictOfListOfDicts_OuterInsertionOrderDoesNotMatter(Dictionary<string, List<Dictionary<string, int>>> raw)
    {
        var a = new NestedCollections { DictOfListOfDicts = raw };
        var b = new NestedCollections { DictOfListOfDicts = raw.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value) };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property DictOfListOfDicts_MiddleOrderMatters(string key, Dictionary<string, int> d1, Dictionary<string, int> d2)
    {
        if (key == null) return true.ToProperty().When(true);
        // middle is List — position matters
        if (d1.SequenceEqual(d2)) return true.ToProperty().When(true);
        var a = new NestedCollections { DictOfListOfDicts = new Dictionary<string, List<Dictionary<string, int>>> { [key] = [d1, d2] } };
        var b = new NestedCollections { DictOfListOfDicts = new Dictionary<string, List<Dictionary<string, int>>> { [key] = [d2, d1] } };
        return (!a.Equals(b)).ToProperty();
    }

    [Property]
    public Property DictOfListOfDicts_InnermostInsertionOrderDoesNotMatter(Dictionary<string, List<Dictionary<string, int>>> raw)
    {
        var copy = raw.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.Select(d => d.Reverse().ToDictionary(p => p.Key, p => p.Value)).ToList());
        var a = new NestedCollections { DictOfListOfDicts = raw };
        var b = new NestedCollections { DictOfListOfDicts = copy };
        return a.Equals(b).ToProperty();
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
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property ListOfDictOfLists_OuterOrderMatters(Dictionary<string, List<int>> d1, Dictionary<string, List<int>> d2)
    {
        // outer is List — position matters
        Func<Dictionary<string, List<int>>, Dictionary<string, List<int>>, bool> sameContent =
            (x, y) => x.Count == y.Count && x.All(kv => y.TryGetValue(kv.Key, out var v) && kv.Value.SequenceEqual(v));

        if (sameContent(d1, d2)) return true.ToProperty().When(true);
        var a = new NestedCollections { ListOfDictOfLists = [d1, d2] };
        var b = new NestedCollections { ListOfDictOfLists = [d2, d1] };
        return (!a.Equals(b)).ToProperty();
    }

    [Property]
    public Property ListOfDictOfLists_MiddleInsertionOrderDoesNotMatter(List<Dictionary<string, List<int>>> items)
    {
        var reversed = items.Select(d => d.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value)).ToList();
        var a = new NestedCollections { ListOfDictOfLists = items };
        var b = new NestedCollections { ListOfDictOfLists = reversed };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property ListOfDictOfLists_InnermostOrderMatters(string key, int v1, int v2)
    {
        if (key == null || v1 == v2) return true.ToProperty().When(true);
        var a = new NestedCollections { ListOfDictOfLists = [new Dictionary<string, List<int>> { [key] = [v1, v2] }] };
        var b = new NestedCollections { ListOfDictOfLists = [new Dictionary<string, List<int>> { [key] = [v2, v1] }] };
        return (!a.Equals(b)).ToProperty();
    }

    // ══════════════════════════════════════════════════════════════════════
    // 3-level: List<Dict<K, HashSet<V>>>
    // ══════════════════════════════════════════════════════════════════════

    // FsCheck cannot generate HashSet<T>; use Dictionary<string, int[]> and convert values to HashSet<int>

    [Property]
    public Property ListOfDictOfSets_EqualWhenSameContent(List<Dictionary<string, int[]>> raw)
    {
        var items = raw.Select(d => d.ToDictionary(kv => kv.Key, kv => new HashSet<int>(kv.Value))).ToList();
        var copy  = raw.Select(d => d.ToDictionary(kv => kv.Key, kv => new HashSet<int>(kv.Value))).ToList();
        var a = new NestedCollections { ListOfDictOfSets = items };
        var b = new NestedCollections { ListOfDictOfSets = copy };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property ListOfDictOfSets_OuterOrderMatters(Dictionary<string, int[]> raw1, Dictionary<string, int[]> raw2)
    {
        var d1 = raw1.ToDictionary(kv => kv.Key, kv => new HashSet<int>(kv.Value));
        var d2 = raw2.ToDictionary(kv => kv.Key, kv => new HashSet<int>(kv.Value));
        Func<Dictionary<string, HashSet<int>>, Dictionary<string, HashSet<int>>, bool> sameContent =
            (x, y) => x.Count == y.Count && x.All(kv => y.TryGetValue(kv.Key, out var v) && kv.Value.SetEquals(v));

        if (sameContent(d1, d2)) return true.ToProperty().When(true);
        var a = new NestedCollections { ListOfDictOfSets = [d1, d2] };
        var b = new NestedCollections { ListOfDictOfSets = [d2, d1] };
        return (!a.Equals(b)).ToProperty();
    }

    [Property]
    public Property ListOfDictOfSets_MiddleInsertionOrderDoesNotMatter(List<Dictionary<string, int[]>> raw)
    {
        var items    = raw.Select(d => d.ToDictionary(kv => kv.Key, kv => new HashSet<int>(kv.Value))).ToList();
        var reversed = raw.Select(d => d.Reverse().ToDictionary(kv => kv.Key, kv => new HashSet<int>(kv.Value))).ToList();
        var a = new NestedCollections { ListOfDictOfSets = items };
        var b = new NestedCollections { ListOfDictOfSets = reversed };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property ListOfDictOfSets_InnermostOrderDoesNotMatter(string key, int v1, int v2)
    {
        if (key == null) return true.ToProperty().When(true);
        // innermost is HashSet — insertion order must not matter
        var a = new NestedCollections
        {
            ListOfDictOfSets = [new Dictionary<string, HashSet<int>> { [key] = new HashSet<int> { v1, v2 } }]
        };
        var b = new NestedCollections
        {
            ListOfDictOfSets = [new Dictionary<string, HashSet<int>> { [key] = new HashSet<int> { v2, v1 } }]
        };
        return a.Equals(b).ToProperty();
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
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property ListOfListOfDicts_OuterOrderMatters(List<Dictionary<string, int>> l1, List<Dictionary<string, int>> l2)
    {
        Func<List<Dictionary<string, int>>, List<Dictionary<string, int>>, bool> sameContent =
            (x, y) => x.Count == y.Count &&
                       x.Zip(y).All(pair => pair.First.SequenceEqual(pair.Second));

        if (sameContent(l1, l2)) return true.ToProperty().When(true);
        var a = new NestedCollections { ListOfListOfDicts = [l1, l2] };
        var b = new NestedCollections { ListOfListOfDicts = [l2, l1] };
        return (!a.Equals(b)).ToProperty();
    }

    [Property]
    public Property ListOfListOfDicts_MiddleOrderMatters(Dictionary<string, int> d1, Dictionary<string, int> d2)
    {
        // middle is also List — position matters
        if (d1.SequenceEqual(d2)) return true.ToProperty().When(true);
        var a = new NestedCollections { ListOfListOfDicts = [[d1, d2]] };
        var b = new NestedCollections { ListOfListOfDicts = [[d2, d1]] };
        return (!a.Equals(b)).ToProperty();
    }

    [Property]
    public Property ListOfListOfDicts_InnermostInsertionOrderDoesNotMatter(List<List<Dictionary<string, int>>> items)
    {
        var copy = items.Select(l => l.Select(d => d.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value)).ToList()).ToList();
        var a = new NestedCollections { ListOfListOfDicts = items };
        var b = new NestedCollections { ListOfListOfDicts = copy };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property ListOfListOfDicts_EqualImpliesSameHash(List<List<Dictionary<string, int>>> items)
    {
        var copy = items.Select(l => l.Select(d => new Dictionary<string, int>(d)).ToList()).ToList();
        var a = new NestedCollections { ListOfListOfDicts = items };
        var b = new NestedCollections { ListOfListOfDicts = copy };
        return (a.Equals(b) && a.GetHashCode() == b.GetHashCode()).ToProperty();
    }

    // ══════════════════════════════════════════════════════════════════════
    // Arrays: int[]
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property FlatArray_EqualWhenSameContent(int[] arr)
    {
        var a = new NestedCollections { FlatArray = arr };
        var b = new NestedCollections { FlatArray = (int[])arr.Clone() };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property FlatArray_OrderMatters(int v1, int v2)
    {
        if (v1 == v2) return true.ToProperty().When(true);
        var a = new NestedCollections { FlatArray = [v1, v2] };
        var b = new NestedCollections { FlatArray = [v2, v1] };
        return (!a.Equals(b)).ToProperty();
    }

    [Property]
    public Property FlatArray_EqualImpliesSameHash(int[] arr)
    {
        var a = new NestedCollections { FlatArray = arr };
        var b = new NestedCollections { FlatArray = (int[])arr.Clone() };
        return (a.Equals(b) && a.GetHashCode() == b.GetHashCode()).ToProperty();
    }

    // ══════════════════════════════════════════════════════════════════════
    // Arrays: int[][] (array of arrays)
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property ArrayOfArrays_EqualWhenSameContent(int[][] arr)
    {
        var a = new NestedCollections { ArrayOfArrays = arr };
        var b = new NestedCollections { ArrayOfArrays = arr.Select(inner => (int[])inner.Clone()).ToArray() };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property ArrayOfArrays_OuterOrderMatters(int[] inner1, int[] inner2)
    {
        if (inner1.SequenceEqual(inner2)) return true.ToProperty().When(true);
        var a = new NestedCollections { ArrayOfArrays = [inner1, inner2] };
        var b = new NestedCollections { ArrayOfArrays = [inner2, inner1] };
        return (!a.Equals(b)).ToProperty();
    }

    [Property]
    public Property ArrayOfArrays_InnerOrderMatters(int v1, int v2)
    {
        if (v1 == v2) return true.ToProperty().When(true);
        var a = new NestedCollections { ArrayOfArrays = [[v1, v2]] };
        var b = new NestedCollections { ArrayOfArrays = [[v2, v1]] };
        return (!a.Equals(b)).ToProperty();
    }

    // ══════════════════════════════════════════════════════════════════════
    // Arrays: Dictionary<string, int>[] (array of dicts)
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property ArrayOfDicts_EqualWhenSameContent(Dictionary<string, int>[] arr)
    {
        var a = new NestedCollections { ArrayOfDicts = arr };
        var b = new NestedCollections { ArrayOfDicts = arr.Select(d => new Dictionary<string, int>(d)).ToArray() };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property ArrayOfDicts_OuterOrderMatters(Dictionary<string, int> d1, Dictionary<string, int> d2)
    {
        if (d1.SequenceEqual(d2)) return true.ToProperty().When(true);
        var a = new NestedCollections { ArrayOfDicts = [d1, d2] };
        var b = new NestedCollections { ArrayOfDicts = [d2, d1] };
        return (!a.Equals(b)).ToProperty();
    }

    [Property]
    public Property ArrayOfDicts_InnerInsertionOrderDoesNotMatter(Dictionary<string, int>[] arr)
    {
        var a = new NestedCollections { ArrayOfDicts = arr };
        var b = new NestedCollections { ArrayOfDicts = arr.Select(d => d.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value)).ToArray() };
        return a.Equals(b).ToProperty();
    }

    // ══════════════════════════════════════════════════════════════════════
    // Arrays: Dictionary<string, int[]> (dict of arrays)
    // ══════════════════════════════════════════════════════════════════════

    [Property]
    public Property DictOfArrays_EqualWhenSameContent(Dictionary<string, int[]> raw)
    {
        var a = new NestedCollections { DictOfArrays = raw };
        var b = new NestedCollections { DictOfArrays = raw.ToDictionary(kv => kv.Key, kv => (int[])kv.Value.Clone()) };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property DictOfArrays_OuterInsertionOrderDoesNotMatter(Dictionary<string, int[]> raw)
    {
        var a = new NestedCollections { DictOfArrays = raw };
        var b = new NestedCollections { DictOfArrays = raw.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value) };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property DictOfArrays_InnerOrderMatters(string key, int v1, int v2)
    {
        if (key == null || v1 == v2) return true.ToProperty().When(true);
        var a = new NestedCollections { DictOfArrays = new Dictionary<string, int[]> { [key] = [v1, v2] } };
        var b = new NestedCollections { DictOfArrays = new Dictionary<string, int[]> { [key] = [v2, v1] } };
        return (!a.Equals(b)).ToProperty();
    }

    [Property]
    public Property DictOfArrays_EqualImpliesSameHash(Dictionary<string, int[]> raw)
    {
        var a = new NestedCollections { DictOfArrays = raw };
        var b = new NestedCollections { DictOfArrays = raw.ToDictionary(kv => kv.Key, kv => (int[])kv.Value.Clone()) };
        return (a.Equals(b) && a.GetHashCode() == b.GetHashCode()).ToProperty();
    }
}
