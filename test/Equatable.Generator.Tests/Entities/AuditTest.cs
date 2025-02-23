using Equatable.Entities;

namespace Equatable.Generator.Tests.Entities;

public class AuditTest
{
    [Fact]
    public void EqualAuditTrue()
    {
        var lockObject = new object();

        var left = new Audit
        {
            Id = 1,
            Date = new DateTime(2024, 9, 1),
            UserId = 1,
            TaskId = 2,
            Lock = lockObject
        };

        var right = new Audit
        {
            Id = 1,
            Date = new DateTime(2024, 9, 1),
            UserId = 1,
            TaskId = 2,
            Lock = lockObject
        };

        var isEqual = left.Equals(right);
        Assert.True(isEqual);

        // check operator ==
        isEqual = left == right;
        Assert.True(isEqual);
    }

    [Fact]
    public void NotEqualAudit()
    {
        var left = new Audit
        {
            Id = 1,
            Date = new DateTime(2024, 9, 1),
            UserId = 1,
            TaskId = 2,
            Lock = new object()
        };

        var right = new Audit
        {
            Id = 1,
            Date = new DateTime(2024, 9, 1),
            UserId = 1,
            TaskId = 2,
            Lock = new object()
        };

        var isEqual = left.Equals(right);
        Assert.False(isEqual);

        // check operator !=
        isEqual = left != right;
        Assert.True(isEqual);

    }

    [Fact]
    public void HashCodeAuditTrue()
    {
        var lockObject = new object();
        var left = new Audit
        {
            Id = 1,
            Date = new DateTime(2024, 9, 1),
            UserId = 1,
            TaskId = 2,
            Lock = lockObject
        };

        var right = new Audit
        {
            Id = 1,
            Date = new DateTime(2024, 9, 1),
            UserId = 1,
            TaskId = 2,
            Lock = lockObject
        };

        var leftCode = left.GetHashCode();
        var rightCode = right.GetHashCode();

        Assert.Equal(rightCode, leftCode);
    }

    [Fact]
    public void HashCodeAuditNotEqual()
    {
        var left = new Audit
        {
            Id = 1,
            Date = new DateTime(2024, 9, 1),
            UserId = 1,
            TaskId = 2,
            Lock = new object()
        };

        var right = new Audit
        {
            Id = 1,
            Date = new DateTime(2024, 9, 1),
            UserId = 1,
            TaskId = 2,
            Lock = new object()
        };

        var leftCode = left.GetHashCode();
        var rightCode = right.GetHashCode();

        Assert.NotEqual(rightCode, leftCode);
    }
}
