using System;

using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Member
{
    public Guid Id { get; set; }

    public string EmailAddress { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }
}
