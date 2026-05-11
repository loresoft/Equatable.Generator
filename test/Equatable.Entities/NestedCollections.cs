using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

/// <summary>
/// Covers the cross-product of outer × inner collection shapes for auto-composed comparers.
/// All combinations up to 3 levels of nesting.
/// </summary>
[Equatable]
public partial class NestedCollections
{
    // ── 2-level: Dict outer ────────────────────────────────────────────────

    // Dict<K, List<V>>
    [DictionaryEquality]
    public Dictionary<string, List<int>>? DictOfLists { get; set; }

    // Dict<K, HashSet<V>>
    [DictionaryEquality]
    public Dictionary<string, HashSet<int>>? DictOfSets { get; set; }

    // Dict<K, Dict<K2, V>>
    [DictionaryEquality]
    public Dictionary<string, Dictionary<string, int>>? DictOfDicts { get; set; }

    // ── 2-level: List outer ────────────────────────────────────────────────

    // List<Dict<K,V>>
    [SequenceEquality]
    public List<Dictionary<string, int>>? ListOfDicts { get; set; }

    // List<HashSet<V>>
    [SequenceEquality]
    public List<HashSet<int>>? ListOfSets { get; set; }

    // List<List<V>>
    [SequenceEquality]
    public List<List<int>>? ListOfLists { get; set; }

    // ── 2-level: HashSet outer ────────────────────────────────────────────

    // HashSet<List<V>> — set of sequences (unusual but valid)
    [HashSetEquality]
    public HashSet<List<int>>? SetOfLists { get; set; }

    // HashSet<Dict<K,V>>
    [HashSetEquality]
    public HashSet<Dictionary<string, int>>? SetOfDicts { get; set; }

    // ── 3-level ────────────────────────────────────────────────────────────

    // Dict<K, Dict<K2, List<V>>>
    [DictionaryEquality]
    public Dictionary<string, Dictionary<string, List<int>>>? ThreeLevelNested { get; set; }

    // Dict<K, List<HashSet<V>>>
    [DictionaryEquality]
    public Dictionary<string, List<HashSet<int>>>? DictOfListOfSets { get; set; }

    // Dict<K, List<Dict<K2, V>>>
    [DictionaryEquality]
    public Dictionary<string, List<Dictionary<string, int>>>? DictOfListOfDicts { get; set; }

    // List<Dict<K, List<V>>>
    [SequenceEquality]
    public List<Dictionary<string, List<int>>>? ListOfDictOfLists { get; set; }

    // List<Dict<K, HashSet<V>>>
    [SequenceEquality]
    public List<Dictionary<string, HashSet<int>>>? ListOfDictOfSets { get; set; }

    // List<List<Dict<K, V>>>
    [SequenceEquality]
    public List<List<Dictionary<string, int>>>? ListOfListOfDicts { get; set; }

    // ── Arrays ─────────────────────────────────────────────────────────────

    // int[] — plain array
    [SequenceEquality]
    public int[]? FlatArray { get; set; }

    // int[][] — array of arrays
    [SequenceEquality]
    public int[][]? ArrayOfArrays { get; set; }

    // Dictionary<string, int>[] — array of dicts
    [SequenceEquality]
    public Dictionary<string, int>[]? ArrayOfDicts { get; set; }

    // int[][] as a value in a dict
    [DictionaryEquality]
    public Dictionary<string, int[]>? DictOfArrays { get; set; }
}
