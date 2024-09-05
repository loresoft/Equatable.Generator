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
        isEqual.Should().BeTrue();

        // check operator ==
        isEqual = left == right;
        isEqual.Should().BeTrue();
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
        isEqual.Should().BeFalse();

        // check operator !=
        isEqual = left != right;
        isEqual.Should().BeTrue();

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

        leftCode.Should().Be(rightCode);
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

        leftCode.Should().NotBe(rightCode);
    }
}
