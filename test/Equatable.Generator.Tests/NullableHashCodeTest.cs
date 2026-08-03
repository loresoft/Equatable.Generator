using Equatable.Entities;

namespace Equatable.Generator.Tests;

public class NullableHashCodeTest
{
    [Fact]
    public void NullableBooleanNullHashesDifferentThanFalse()
    {
        var nullValue = new Measurement { IsVerified = null };
        var falseValue = new Measurement { IsVerified = false };

        Assert.NotEqual(nullValue.GetHashCode(), falseValue.GetHashCode());
    }

    [Fact]
    public void NullableBooleanNullHashesDifferentThanTrue()
    {
        var nullValue = new Measurement { IsVerified = null };
        var trueValue = new Measurement { IsVerified = true };

        Assert.NotEqual(nullValue.GetHashCode(), trueValue.GetHashCode());
    }

    [Fact]
    public void NullableIntegerNullHashesDifferentThanZero()
    {
        var nullValue = new Measurement { SampleCount = null };
        var zeroValue = new Measurement { SampleCount = 0 };

        Assert.NotEqual(nullValue.GetHashCode(), zeroValue.GetHashCode());
    }

    [Fact]
    public void NullableEnumNullHashesDifferentThanDefaultMember()
    {
        var nullValue = new Measurement { Accuracy = null };
        var defaultValue = new Measurement { Accuracy = Accuracy.Low };

        Assert.NotEqual(nullValue.GetHashCode(), defaultValue.GetHashCode());
    }

    [Fact]
    public void NullableBooleanNullIsNotEqualToFalse()
    {
        var nullValue = new Measurement { IsVerified = null };
        var falseValue = new Measurement { IsVerified = false };

        Assert.NotEqual(nullValue, falseValue);
    }

    [Fact]
    public void StructWithoutEqualityOperatorComparesByValue()
    {
        var left = new Measurement { Weight = new Weight { Value = 1.5, Unit = "kg" } };
        var right = new Measurement { Weight = new Weight { Value = 1.5, Unit = "kg" } };

        Assert.Equal(left, right);
    }

    [Fact]
    public void StructWithEqualityOperatorComparesByValue()
    {
        var left = new Measurement { Distance = new Distance { Value = 2.5 } };
        var right = new Measurement { Distance = new Distance { Value = 2.5 } };

        Assert.Equal(left, right);
    }
}
