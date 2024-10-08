using System;

using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserLogin : ModelBase
{
    public string? EmailAddress { get; set; }
    public Guid? UserId { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
    public bool IsSuccessful { get; set; }
    public string? FailureMessage { get; set; }

    [IgnoreEquality]
    public User? User { get; set; }
}
