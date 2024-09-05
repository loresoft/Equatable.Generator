using System.Collections.Generic;

using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class CustomLength
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    [EqualityComparer(typeof(LengthComparerDefault))]
    public string? Key { get; set; }

    [EqualityComparer(typeof(LengthComparerInstance), nameof(LengthComparerInstance.Instance))]
    public string? Value { get; set; }

}

public static class LengthComparerDefault
{
    public static readonly LengthEqualityComparer Default = new();
}

public static class LengthComparerInstance
{
    public static readonly LengthEqualityComparer Instance = new();
}

public class LengthEqualityComparer : IEqualityComparer<string?>
{
    public bool Equals(string? x, string? y) => x?.Length == y?.Length;

    public int GetHashCode(string? obj) => obj?.Length.GetHashCode() ?? 0;
}

