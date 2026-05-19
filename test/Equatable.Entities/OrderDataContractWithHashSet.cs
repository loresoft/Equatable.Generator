using System.Collections.Generic;
using System.Runtime.Serialization;
using Equatable.Attributes.DataContract;

namespace Equatable.Entities;

[DataContract]
[DataContractEquatable]
public partial class OrderDataContractWithHashSet
{
    [DataMember(Order = 0)]
    public int Id { get; set; }

    // HashSet<T> — no explicit attribute; InferCollectionComparer must choose HashSetEquality
    [DataMember(Order = 1)]
    public HashSet<string>? Tags { get; set; }

    // IReadOnlySet<T> — covers the IReadOnlySet interface path
    [DataMember(Order = 2)]
    public IReadOnlySet<int>? Codes { get; set; }
}
