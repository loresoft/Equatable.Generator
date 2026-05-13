using System.Collections.Generic;
using System.Runtime.Serialization;
using Equatable.Attributes.DataContract;

namespace Equatable.Entities;

[DataContract]
[DataContractEquatable]
public partial class OrderDataContractNested
{
    [DataMember(Order = 0)]
    public int Id { get; set; }

    // Dict<K, List<V>> — inferred: DictionaryEqualityComparer(…, SequenceEqualityComparer<int>.Default)
    [DataMember(Order = 1)]
    public Dictionary<string, List<int>>? TagGroups { get; set; }

    // Dict<K, Dict<K2, V>> — inferred: DictionaryEqualityComparer(…, DictionaryEqualityComparer(…))
    [DataMember(Order = 2)]
    public Dictionary<string, Dictionary<string, int>>? NestedMap { get; set; }

    // List<Dict<K, V>> — inferred: SequenceEqualityComparer(DictionaryEqualityComparer(…))
    [DataMember(Order = 3)]
    public List<Dictionary<string, int>>? Records { get; set; }

    // IReadOnlyDictionary<K, List<V>> — inferred: ReadOnlyDictionaryEqualityComparer(…, SequenceEqualityComparer<string>.Default)
    [DataMember(Order = 4)]
    public IReadOnlyDictionary<string, List<string>>? ReadOnlyTagGroups { get; set; }
}
