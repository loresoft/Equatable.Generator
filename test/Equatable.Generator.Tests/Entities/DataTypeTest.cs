using Equatable.Entities;

namespace Equatable.Generator.Tests.Entities;

public class DataTypeTest
{
    [Fact]
    public void EqualNotNull()
    {
        var left = new DataType
        {
            Id = 1,
            Name = "Test1",
            Boolean = false,
            Short = 2,
            Long = 200,
            Float = 200.20F,
            Double = 300.35,
            Decimal = 456.12M,
            DateTime = new DateTime(2024, 5, 1, 8, 0, 0),
            DateTimeOffset = new DateTimeOffset(2024, 5, 1, 8, 0, 0, TimeSpan.FromHours(-6)),
            Guid = Guid.Empty,
            TimeSpan = TimeSpan.FromHours(1),
            DateOnly = new DateOnly(2022, 12, 1),
            TimeOnly = new TimeOnly(1, 30, 0),
            BooleanNull = false,
            ShortNull = 2,
            LongNull = 200,
            FloatNull = 200.20F,
            DoubleNull = 300.35,
            DecimalNull = 456.12M,
            DateTimeNull = new DateTime(2024, 4, 1, 8, 0, 0),
            DateTimeOffsetNull = new DateTimeOffset(2024, 4, 1, 8, 0, 0, TimeSpan.FromHours(-6)),
            GuidNull = Guid.Empty,
            TimeSpanNull = TimeSpan.FromHours(1),
            DateOnlyNull = new DateOnly(2022, 12, 1),
            TimeOnlyNull = new TimeOnly(1, 30, 0),
        };

        var right = new DataType
        {
            Id = 1,
            Name = "Test1",
            Boolean = false,
            Short = 2,
            Long = 200,
            Float = 200.20F,
            Double = 300.35,
            Decimal = 456.12M,
            DateTime = new DateTime(2024, 5, 1, 8, 0, 0),
            DateTimeOffset = new DateTimeOffset(2024, 5, 1, 8, 0, 0, TimeSpan.FromHours(-6)),
            Guid = Guid.Empty,
            TimeSpan = TimeSpan.FromHours(1),
            DateOnly = new DateOnly(2022, 12, 1),
            TimeOnly = new TimeOnly(1, 30, 0),
            BooleanNull = false,
            ShortNull = 2,
            LongNull = 200,
            FloatNull = 200.20F,
            DoubleNull = 300.35,
            DecimalNull = 456.12M,
            DateTimeNull = new DateTime(2024, 4, 1, 8, 0, 0),
            DateTimeOffsetNull = new DateTimeOffset(2024, 4, 1, 8, 0, 0, TimeSpan.FromHours(-6)),
            GuidNull = Guid.Empty,
            TimeSpanNull = TimeSpan.FromHours(1),
            DateOnlyNull = new DateOnly(2022, 12, 1),
            TimeOnlyNull = new TimeOnly(1, 30, 0),
        };

        var isEqual = left.Equals(right);
        Assert.True(isEqual);

        // check operator ==
        isEqual = left == right;
        Assert.True(isEqual);

    }

    [Fact]
    public void EqualNulls()
    {
        var left = new DataType
        {
            Id = 1,
            Name = "Test1",
            Boolean = false,
            Short = 2,
            Long = 200,
            Float = 200.20F,
            Double = 300.35,
            Decimal = 456.12M,
            DateTime = new DateTime(2024, 5, 1, 8, 0, 0),
            DateTimeOffset = new DateTimeOffset(2024, 5, 1, 8, 0, 0, TimeSpan.FromHours(-6)),
            Guid = Guid.Empty,
            TimeSpan = TimeSpan.FromHours(1),
            DateOnly = new DateOnly(2022, 12, 1),
            TimeOnly = new TimeOnly(1, 30, 0)
        };

        var right = new DataType
        {
            Id = 1,
            Name = "Test1",
            Boolean = false,
            Short = 2,
            Long = 200,
            Float = 200.20F,
            Double = 300.35,
            Decimal = 456.12M,
            DateTime = new DateTime(2024, 5, 1, 8, 0, 0),
            DateTimeOffset = new DateTimeOffset(2024, 5, 1, 8, 0, 0, TimeSpan.FromHours(-6)),
            Guid = Guid.Empty,
            TimeSpan = TimeSpan.FromHours(1),
            DateOnly = new DateOnly(2022, 12, 1),
            TimeOnly = new TimeOnly(1, 30, 0)
        };

        var isEqual = left.Equals(right);
        Assert.True(isEqual);

        // check operator ==
        isEqual = left == right;
        Assert.True(isEqual);

    }

    [Fact]
    public void NotEqualNotNull()
    {
        var left = new DataType
        {
            Id = 1,
            Name = "Test1",
            Boolean = false,
            Short = 2,
            Long = 200,
            Float = 200.20F,
            Double = 300.35,
            Decimal = 456.12M,
            DateTime = new DateTime(2024, 5, 1, 8, 0, 0),
            DateTimeOffset = new DateTimeOffset(2024, 5, 1, 8, 0, 0, TimeSpan.FromHours(-6)),
            Guid = Guid.Empty,
            TimeSpan = TimeSpan.FromHours(1),
            DateOnly = new DateOnly(2022, 12, 1),
            TimeOnly = new TimeOnly(1, 30, 0),
            BooleanNull = false,
            ShortNull = 2,
            LongNull = 200,
            FloatNull = 200.20F,
            DoubleNull = 300.35,
            DecimalNull = 456.12M,
            DateTimeNull = new DateTime(2024, 4, 1, 8, 0, 0),
            DateTimeOffsetNull = new DateTimeOffset(2024, 4, 1, 8, 0, 0, TimeSpan.FromHours(-6)),
            GuidNull = Guid.Empty,
            TimeSpanNull = TimeSpan.FromHours(1),
            DateOnlyNull = new DateOnly(2022, 12, 1),
            TimeOnlyNull = new TimeOnly(1, 30, 0),
        };

        var right = new DataType
        {
            Id = 2,
            Name = "Test2",
            Boolean = true,
            Short = 2,
            Long = 200,
            Float = 200.20F,
            Double = 300.35,
            Decimal = 456.12M,
            DateTime = new DateTime(2024, 5, 1, 8, 0, 0),
            DateTimeOffset = new DateTimeOffset(2024, 5, 1, 8, 0, 0, TimeSpan.FromHours(-6)),
            Guid = Guid.Empty,
            TimeSpan = TimeSpan.FromHours(1),
            DateOnly = new DateOnly(2022, 12, 1),
            TimeOnly = new TimeOnly(1, 30, 0),
            BooleanNull = false,
            ShortNull = 2,
            LongNull = 200,
            FloatNull = 200.20F,
            DoubleNull = 300.35,
            DecimalNull = 456.12M,
            DateTimeNull = new DateTime(2024, 4, 1, 8, 0, 0),
            DateTimeOffsetNull = new DateTimeOffset(2024, 4, 1, 8, 0, 0, TimeSpan.FromHours(-6)),
            GuidNull = Guid.Empty,
            TimeSpanNull = TimeSpan.FromHours(1),
            DateOnlyNull = new DateOnly(2022, 12, 1),
            TimeOnlyNull = new TimeOnly(1, 30, 0),
        };

        var isEqual = left.Equals(right);
        Assert.False(isEqual);

        // check operator !=
        isEqual = left != right;
        Assert.True(isEqual);

    }

    [Fact]
    public void NotEqualNulls()
    {
        var left = new DataType
        {
            Id = 1,
            Name = "Test1",
            Boolean = false,
            Short = 2,
            Long = 200,
            Float = 200.20F,
            Double = 300.35,
            Decimal = 456.12M,
            DateTime = new DateTime(2024, 5, 1, 8, 0, 0),
            DateTimeOffset = new DateTimeOffset(2024, 5, 1, 8, 0, 0, TimeSpan.FromHours(-6)),
            Guid = Guid.Empty,
            TimeSpan = TimeSpan.FromHours(1),
            DateOnly = new DateOnly(2022, 12, 1),
            TimeOnly = new TimeOnly(1, 30, 0)
        };

        var right = new DataType
        {
            Id = 2,
            Name = "Test2",
            Boolean = true,
            Short = 2,
            Long = 200,
            Float = 200.20F,
            Double = 300.35,
            Decimal = 456.12M,
            DateTime = new DateTime(2024, 5, 1, 8, 0, 0),
            DateTimeOffset = new DateTimeOffset(2024, 5, 1, 8, 0, 0, TimeSpan.FromHours(-6)),
            Guid = Guid.Empty,
            TimeSpan = TimeSpan.FromHours(1),
            DateOnly = new DateOnly(2022, 12, 1),
            TimeOnly = new TimeOnly(1, 30, 0)
        };

        var isEqual = left.Equals(right);
        Assert.False(isEqual);

        // check operator !=
        isEqual = left != right;
        Assert.True(isEqual);

    }
}
