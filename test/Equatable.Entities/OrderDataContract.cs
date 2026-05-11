using System.Collections.Generic;
using System.Runtime.Serialization;
using Equatable.Attributes;

namespace Equatable.Entities;

[DataContract]
[DataContractEquatable]
public partial class OrderDataContract
{
    [DataMember(Order = 0)]
    public int Id { get; set; }

    [DataMember(Order = 1)]
    public string? Name { get; set; }

    // not included — no [DataMember]
    public string? InternalNote { get; set; }

    [IgnoreDataMember]
    public string? IgnoredField { get; set; }
}
