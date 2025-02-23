using Equatable.Entities;

namespace Equatable.Generator.Tests.Entities;

public class StatusTest
{
    [Fact]
    public void EqualUsesBaseTrue()
    {
        var left = new Status
        {
            Id = 1,
            Name = "In Progress",
            Description = "In Progress",
            IsActive = true,
            DisplayOrder = 1,
            Created = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            CreatedBy = "system",
            Updated = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            UpdatedBy = "system"
        };

        var right = new Status
        {
            Id = 1,
            Name = "In Progress",
            Description = "In Progress",
            IsActive = true,
            DisplayOrder = 1,
            Created = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            CreatedBy = "system",
            Updated = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            UpdatedBy = "system"
        };

        var isEqual = left.Equals(right);
        Assert.True(isEqual);

        // check operator ==
        isEqual = left == right;
        Assert.True(isEqual);

    }

    [Fact]
    public void NotEqualUsesBase()
    {
        var left = new Status
        {
            Id = 1,
            Name = "In Progress",
            Description = "In Progress",
            IsActive = true,
            DisplayOrder = 1,
            Created = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            CreatedBy = "system",
            Updated = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            UpdatedBy = "system"
        };

        var right = new Status
        {
            Id = 2,
            Name = "In Progress",
            Description = "In Progress",
            IsActive = true,
            DisplayOrder = 1,
            Created = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            CreatedBy = "system",
            Updated = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            UpdatedBy = "system"
        };

        var isEqual = left.Equals(right);
        Assert.False(isEqual);

        // check operator !=
        isEqual = left != right;
        Assert.True(isEqual);

    }

    [Fact]
    public void HashCodeUsesBaseTrue()
    {
        var left = new Status
        {
            Id = 1,
            Name = "In Progress",
            Description = "In Progress",
            IsActive = true,
            DisplayOrder = 1,
            Created = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            CreatedBy = "system",
            Updated = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            UpdatedBy = "system"
        };

        var right = new Status
        {
            Id = 1,
            Name = "In Progress",
            Description = "In Progress",
            IsActive = true,
            DisplayOrder = 1,
            Created = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            CreatedBy = "system",
            Updated = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            UpdatedBy = "system"
        };

        var leftCode = left.GetHashCode();
        var rightCode = right.GetHashCode();

        Assert.Equal(rightCode, leftCode);
    }

    [Fact]
    public void HashCodeUsesBaseNotEqual()
    {
        var left = new Status
        {
            Id = 1,
            Name = "In Progress",
            Description = "In Progress",
            IsActive = true,
            DisplayOrder = 1,
            Created = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            CreatedBy = "system",
            Updated = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            UpdatedBy = "system"
        };

        var right = new Status
        {
            Id = 2,
            Name = "In Progress",
            Description = "In Progress",
            IsActive = true,
            DisplayOrder = 1,
            Created = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            CreatedBy = "system",
            Updated = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            UpdatedBy = "system"
        };

        var leftCode = left.GetHashCode();
        var rightCode = right.GetHashCode();

        Assert.NotEqual(rightCode, leftCode);
    }
}
