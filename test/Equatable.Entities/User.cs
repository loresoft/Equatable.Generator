using System;
using System.Collections.Generic;

using Equatable.Attributes;
using Equatable.Entities;

namespace Equatable.Generator.Entities;

[Equatable]
public partial class User : ModelBase
{
    [StringEquality(StringComparison.OrdinalIgnoreCase)]
    public string EmailAddress { get; set; } = null!;

    public bool IsEmailAddressConfirmed { get; set; }

    public string? DisplayName { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? PasswordHash { get; set; }

    public string? ResetHash { get; set; }

    public string? InviteHash { get; set; }

    public int AccessFailedCount { get; set; }

    public bool LockoutEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public DateTimeOffset? LastLogin { get; set; }

    public bool IsDeleted { get; set; }

    [SequenceEquality]
    public List<Task>? AssignedTasks { get; set; }

    [SequenceEquality]
    public List<Role>? Roles { get; set; }
}
