using Equatable.Entities;

namespace Equatable.Generator.Tests.Properties;

/// <summary>
/// Property-based tests for [DictionaryEquality] on IReadOnlyDictionary,
/// including auto-composed nested collection comparers.
/// </summary>
public class LookupTableProperties
{
    // --- FlatEntries: IReadOnlyDictionary<string, double> ---

    [Property]
    public Property FlatEntries_Reflexivity(Dictionary<string, double> dict)
    {
        var t = new LookupTable { FlatEntries = dict };
        return t.Equals(t).ToProperty();
    }

    [Property]
    public Property FlatEntries_Symmetry(Dictionary<string, double> x, Dictionary<string, double> y)
    {
        var a = new LookupTable { FlatEntries = x };
        var b = new LookupTable { FlatEntries = y };
        return (a.Equals(b) == b.Equals(a)).ToProperty();
    }

    [Property]
    public Property FlatEntries_InsertionOrderDoesNotMatter(Dictionary<string, double> dict)
    {
        var a = new LookupTable { FlatEntries = dict };
        var b = new LookupTable { FlatEntries = new Dictionary<string, double>(dict.Reverse()) };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property FlatEntries_HashIsInsertionOrderIndependent(Dictionary<string, double> dict)
    {
        var a = new LookupTable { FlatEntries = dict };
        var b = new LookupTable { FlatEntries = new Dictionary<string, double>(dict.Reverse()) };
        return (a.GetHashCode() == b.GetHashCode()).ToProperty();
    }

    [Property]
    public Property FlatEntries_EqualImpliesSameHashCode(Dictionary<string, double> dict)
    {
        var a = new LookupTable { FlatEntries = dict };
        var b = new LookupTable { FlatEntries = new Dictionary<string, double>(dict) };
        return (a.Equals(b) && a.GetHashCode() == b.GetHashCode()).ToProperty();
    }

    [Property]
    public Property FlatEntries_NullBothSidesEqual()
    {
        var a = new LookupTable { FlatEntries = null };
        var b = new LookupTable { FlatEntries = null };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property FlatEntries_NullOneNotEqual(Dictionary<string, double> dict)
    {
        if (dict.Count == 0)
            return true.ToProperty().When(true);

        var a = new LookupTable { FlatEntries = dict };
        var b = new LookupTable { FlatEntries = null };
        return (!a.Equals(b) && !b.Equals(a)).ToProperty();
    }

    [Property]
    public Property FlatEntries_DifferentValueNotEqual(string key, double v1, double v2)
    {
        if (key == null || Math.Abs(v1 - v2) < double.Epsilon || double.IsNaN(v1) || double.IsNaN(v2))
            return true.ToProperty().When(true);

        var a = new LookupTable { FlatEntries = new Dictionary<string, double> { [key] = v1 } };
        var b = new LookupTable { FlatEntries = new Dictionary<string, double> { [key] = v2 } };
        return (!a.Equals(b)).ToProperty();
    }

    // --- NestedEntries: IReadOnlyDictionary<string, IReadOnlyDictionary<string, double>>
    //     auto-composed comparer: no manual [EqualityComparer] needed ---

    [Property]
    public Property NestedEntries_Reflexivity(Dictionary<string, Dictionary<string, double>> raw)
    {
        var t = new LookupTable { NestedEntries = ToNested(raw) };
        return t.Equals(t).ToProperty();
    }

    [Property]
    public Property NestedEntries_Symmetry(
        Dictionary<string, Dictionary<string, double>> x,
        Dictionary<string, Dictionary<string, double>> y)
    {
        var a = new LookupTable { NestedEntries = ToNested(x) };
        var b = new LookupTable { NestedEntries = ToNested(y) };
        return (a.Equals(b) == b.Equals(a)).ToProperty();
    }

    [Property]
    public Property NestedEntries_OuterInsertionOrderDoesNotMatter(Dictionary<string, Dictionary<string, double>> raw)
    {
        var a = new LookupTable { NestedEntries = ToNested(raw) };
        var b = new LookupTable { NestedEntries = ToNested(raw.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value)) };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property NestedEntries_InnerInsertionOrderDoesNotMatter(Dictionary<string, Dictionary<string, double>> raw)
    {
        // reverse the inner dictionary for each entry
        var reversed = raw.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.Reverse().ToDictionary(p => p.Key, p => p.Value));

        var a = new LookupTable { NestedEntries = ToNested(raw) };
        var b = new LookupTable { NestedEntries = ToNested(reversed) };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property NestedEntries_EqualImpliesSameHashCode(Dictionary<string, Dictionary<string, double>> raw)
    {
        var a = new LookupTable { NestedEntries = ToNested(raw) };
        var b = new LookupTable { NestedEntries = ToNested(raw.ToDictionary(kv => kv.Key, kv => kv.Value)) };
        return (a.Equals(b) && a.GetHashCode() == b.GetHashCode()).ToProperty();
    }

    [Property]
    public Property NestedEntries_HashIsOuterInsertionOrderIndependent(Dictionary<string, Dictionary<string, double>> raw)
    {
        var a = new LookupTable { NestedEntries = ToNested(raw) };
        var b = new LookupTable { NestedEntries = ToNested(raw.Reverse().ToDictionary(kv => kv.Key, kv => kv.Value)) };
        return (a.GetHashCode() == b.GetHashCode()).ToProperty();
    }

    [Property]
    public Property NestedEntries_DifferentInnerValueNotEqual(string outerKey, string innerKey, double v1, double v2)
    {
        if (outerKey == null || innerKey == null || Math.Abs(v1 - v2) < double.Epsilon || double.IsNaN(v1) || double.IsNaN(v2))
            return true.ToProperty().When(true);

        var a = new LookupTable
        {
            NestedEntries = new Dictionary<string, IReadOnlyDictionary<string, double>>
            {
                [outerKey] = new Dictionary<string, double> { [innerKey] = v1 }
            }
        };
        var b = new LookupTable
        {
            NestedEntries = new Dictionary<string, IReadOnlyDictionary<string, double>>
            {
                [outerKey] = new Dictionary<string, double> { [innerKey] = v2 }
            }
        };
        return (!a.Equals(b)).ToProperty();
    }

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, double>> ToNested(
        Dictionary<string, Dictionary<string, double>> raw)
        => raw.ToDictionary(kv => kv.Key, kv => (IReadOnlyDictionary<string, double>)kv.Value);
}
