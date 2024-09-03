using Equatable.Attributes;

namespace Equatable.Entities;


[Equatable]
public partial class Status : ModelBase
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
