using System;

using Equatable.Attributes;
using Equatable.Generator.Entities;

namespace Equatable.Entities;

[Equatable]
public partial class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
