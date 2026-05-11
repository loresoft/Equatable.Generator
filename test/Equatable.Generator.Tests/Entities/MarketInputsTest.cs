using System.Collections.Generic;
using Equatable.Entities;

namespace Equatable.Generator.Tests.Entities;

public class MarketInputsTest
{
    [Fact]
    public void EqualsWithReadOnlyDictionary()
    {
        var left = new MarketInputs
        {
            FlatInputs = new Dictionary<string, double> { ["a"] = 1.0, ["b"] = 2.0 }
        };

        var right = new MarketInputs
        {
            FlatInputs = new Dictionary<string, double> { ["b"] = 2.0, ["a"] = 1.0 }
        };

        Assert.True(left.Equals(right));
    }

    [Fact]
    public void NotEqualsWithReadOnlyDictionary()
    {
        var left = new MarketInputs
        {
            FlatInputs = new Dictionary<string, double> { ["a"] = 1.0, ["b"] = 2.0 }
        };

        var right = new MarketInputs
        {
            FlatInputs = new Dictionary<string, double> { ["a"] = 1.0, ["b"] = 3.0 }
        };

        Assert.False(left.Equals(right));
    }

    [Fact]
    public void EqualsWithNestedReadOnlyDictionary()
    {
        var left = new MarketInputs
        {
            NestedInputs = new Dictionary<string, IReadOnlyDictionary<string, double>>
            {
                ["market1"] = new Dictionary<string, double> { ["x"] = 0.5, ["y"] = 0.5 },
                ["market2"] = new Dictionary<string, double> { ["x"] = 0.3, ["y"] = 0.7 }
            }
        };

        var right = new MarketInputs
        {
            NestedInputs = new Dictionary<string, IReadOnlyDictionary<string, double>>
            {
                ["market2"] = new Dictionary<string, double> { ["y"] = 0.7, ["x"] = 0.3 },
                ["market1"] = new Dictionary<string, double> { ["y"] = 0.5, ["x"] = 0.5 }
            }
        };

        Assert.True(left.Equals(right));
    }

    [Fact]
    public void NotEqualsWithNestedReadOnlyDictionary()
    {
        var left = new MarketInputs
        {
            NestedInputs = new Dictionary<string, IReadOnlyDictionary<string, double>>
            {
                ["market1"] = new Dictionary<string, double> { ["x"] = 0.5, ["y"] = 0.5 }
            }
        };

        var right = new MarketInputs
        {
            NestedInputs = new Dictionary<string, IReadOnlyDictionary<string, double>>
            {
                ["market1"] = new Dictionary<string, double> { ["x"] = 0.5, ["y"] = 0.6 }
            }
        };

        Assert.False(left.Equals(right));
    }

    [Fact]
    public void HashCodeEqualsWithReadOnlyDictionary()
    {
        var left = new MarketInputs
        {
            FlatInputs = new Dictionary<string, double> { ["a"] = 1.0, ["b"] = 2.0 }
        };

        var right = new MarketInputs
        {
            FlatInputs = new Dictionary<string, double> { ["b"] = 2.0, ["a"] = 1.0 }
        };

        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }
}
