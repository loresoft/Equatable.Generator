using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    [StringEquality(StringComparison.OrdinalIgnoreCase)]
    public string EmailAddress { get; set; } = null!;

    [JsonPropertyName("name")]
    public string? DisplayName { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public DateTimeOffset? LastLogin { get; set; }

    [JsonIgnore]
    [IgnoreEquality]
    public string FullName => $"{FirstName} {LastName}";

    [HashSetEquality]
    public HashSet<string>? Roles { get; set; }

    [DictionaryEquality]
    public Dictionary<string, int>? Permissions { get; set; }

    [SequenceEquality]
    public List<DateTimeOffset>? History { get; set; }
}
