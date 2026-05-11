using Equatable.Entities;

namespace Equatable.Generator.Tests.Entities;

public class OrderDataContractTest
{
    [Fact]
    public void EqualsOnDataMembers()
    {
        var left = new OrderDataContract { Id = 1, Name = "Test", InternalNote = "note-a", IgnoredField = "ignored-a" };
        var right = new OrderDataContract { Id = 1, Name = "Test", InternalNote = "note-b", IgnoredField = "ignored-b" };

        // InternalNote and IgnoredField are excluded — only Id and Name matter
        Assert.True(left.Equals(right));
    }

    [Fact]
    public void NotEqualsOnDataMembers()
    {
        var left = new OrderDataContract { Id = 1, Name = "Test" };
        var right = new OrderDataContract { Id = 2, Name = "Test" };

        Assert.False(left.Equals(right));
    }

    [Fact]
    public void HashCodeEqualsOnDataMembers()
    {
        var left = new OrderDataContract { Id = 1, Name = "Test", InternalNote = "note-a" };
        var right = new OrderDataContract { Id = 1, Name = "Test", InternalNote = "note-b" };

        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }
}
