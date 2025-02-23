using Equatable.Entities;

namespace Equatable.Generator.Tests.Entities;

public class TaskTest
{
    [Fact]
    public void EqualNested()
    {
        var left = new Equatable.Entities.Task
        {
            Id = 1,
            Title = "In Progress",
            Description = "In Progress",
            StartDate = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            Created = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            CreatedBy = "system",
            Updated = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            UpdatedBy = "system",
            Status = new Status
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
            }
        };

        var right = new Equatable.Entities.Task
        {
            Id = 1,
            Title = "In Progress",
            Description = "In Progress",
            StartDate = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            Created = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            CreatedBy = "system",
            Updated = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            UpdatedBy = "system",
            Status = new Status
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
            }
        };

        var isEqual = left.Equals(right);
        Assert.True(isEqual);

        // check operator ==
        isEqual = left == right;
        Assert.True(isEqual);
    }

    [Fact]
    public void NotEqualNested()
    {
        var left = new Equatable.Entities.Task
        {
            Id = 1,
            Title = "In Progress",
            Description = "In Progress",
            StartDate = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            Created = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            CreatedBy = "system",
            Updated = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            UpdatedBy = "system",
            Status = new Status
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
            }
        };

        var right = new Equatable.Entities.Task
        {
            Id = 1,
            Title = "In Progress",
            Description = "In Progress",
            StartDate = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            Created = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            CreatedBy = "system",
            Updated = new DateTimeOffset(2024, 9, 1, 11, 30, 15, TimeSpan.Zero),
            UpdatedBy = "system",
            Status = new Status
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
            }
        };

        var isEqual = left.Equals(right);
        Assert.False(isEqual);

        // check operator !=
        isEqual = left != right;
        Assert.True(isEqual);
    }

}
