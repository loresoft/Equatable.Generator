using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Role : ModelBase
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}
