using System.Collections.Generic;
using Equatable.Attributes.MessagePack;
using MessagePack;

namespace Equatable.Entities;

[MessagePackObject]
[MessagePackEquatable]
public partial class SerializedRecordNested
{
    [Key(0)]
    public int Id { get; set; }

    // Dict<K, List<V>> — inferred: DictionaryEqualityComparer(…, SequenceEqualityComparer<int>.Default)
    [Key(1)]
    public Dictionary<string, List<int>>? TagGroups { get; set; }

    // Dict<K, Dict<K2, V>> — inferred: DictionaryEqualityComparer(…, DictionaryEqualityComparer(…))
    [Key(2)]
    public Dictionary<string, Dictionary<string, int>>? NestedMap { get; set; }

    // List<Dict<K, V>> — inferred: SequenceEqualityComparer(DictionaryEqualityComparer(…))
    [Key(3)]
    public List<Dictionary<string, int>>? Records { get; set; }

    // IReadOnlyDictionary<K, List<V>> — inferred: ReadOnlyDictionaryEqualityComparer(…, SequenceEqualityComparer<string>.Default)
    [Key(4)]
    public IReadOnlyDictionary<string, List<string>>? ReadOnlyTagGroups { get; set; }
}
