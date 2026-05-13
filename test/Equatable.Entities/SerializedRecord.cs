using Equatable.Attributes.MessagePack;
using MessagePack;

namespace Equatable.Entities;

[MessagePackEquatable]
[MessagePackObject]
public partial class SerializedRecord
{
    [Key(0)]
    public int Id { get; set; }

    [Key(1)]
    public double Score { get; set; }

    [IgnoreMember]
    public string? Metadata { get; set; }

    // not included — no [Key]
    public string? Extra { get; set; }
}
