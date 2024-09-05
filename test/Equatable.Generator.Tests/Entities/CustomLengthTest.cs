using Equatable.Entities;

namespace Equatable.Generator.Tests.Entities;

public class CustomLengthTest
{
    [Fact]
    public void EqualCustomLengthTrue()
    {
        var left = new CustomLength
        {
            Id = 1,
            Name = "custom",
            Key = "aaa",
            Value = "zzz"
        };

        var right = new CustomLength
        {
            Id = 1,
            Name = "custom",
            Key = "bbb",
            Value = "ccc"
        };

        var isEqual = left.Equals(right);
        isEqual.Should().BeTrue();

        // check operator ==
        isEqual = left == right;
        isEqual.Should().BeTrue();
    }

    [Fact]
    public void NotEqualCustomLength()
    {
        var left = new CustomLength
        {
            Id = 1,
            Name = "custom",
            Key = "xyzf",
            Value = "abc"
        };

        var right = new CustomLength
        {
            Id = 1,
            Name = "custom",
            Key = "xyz",
            Value = "abc"
        };

        var isEqual = left.Equals(right);
        isEqual.Should().BeFalse();

        // check operator !=
        isEqual = left != right;
        isEqual.Should().BeTrue();

    }

    [Fact]
    public void HashCodeCustomLengthTrue()
    {
        var left = new CustomLength
        {
            Id = 1,
            Name = "custom",
            Key = "fff",
            Value = "sss"
        };

        var right = new CustomLength
        {
            Id = 1,
            Name = "custom",
            Key = "rrr",
            Value = "zzz"
        };

        var leftCode = left.GetHashCode();
        var rightCode = right.GetHashCode();

        leftCode.Should().Be(rightCode);
    }

    [Fact]
    public void HashCodeCustomLengthNotEqual()
    {
        var left = new CustomLength
        {
            Id = 1,
            Name = "custom",
            Key = "xyzf",
            Value = "abc"
        };

        var right = new CustomLength
        {
            Id = 1,
            Name = "custom",
            Key = "xyz",
            Value = "abc"
        };

        var leftCode = left.GetHashCode();
        var rightCode = right.GetHashCode();

        leftCode.Should().NotBe(rightCode);
    }
}
