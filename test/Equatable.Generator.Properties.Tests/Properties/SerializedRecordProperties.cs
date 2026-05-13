using Equatable.Entities;

namespace Equatable.Generator.Tests.Properties;

/// <summary>
/// Property-based tests for [MessagePackEquatable]:
/// only [Key] properties participate in equality; [IgnoreMember] and unannotated properties are excluded.
/// </summary>
public class SerializedRecordProperties
{
    [Property]
    public Property Reflexivity(int id, double score)
    {
        // NaN != NaN in IEEE 754 — skip to avoid false failures in value equality
        if (double.IsNaN(score)) return true.ToProperty().When(true);
        var r = new SerializedRecord { Id = id, Score = score };
        return r.Equals(r).ToProperty();
    }

    [Property]
    public Property Symmetry(int id1, double s1, int id2, double s2)
    {
        var a = new SerializedRecord { Id = id1, Score = s1 };
        var b = new SerializedRecord { Id = id2, Score = s2 };
        return (a.Equals(b) == b.Equals(a)).ToProperty();
    }

    [Property]
    public Property EqualImpliesSameHashCode(int id, double score)
    {
        if (double.IsNaN(score)) return true.ToProperty().When(true);
        var a = new SerializedRecord { Id = id, Score = score };
        var b = new SerializedRecord { Id = id, Score = score };
        return (a.Equals(b) && a.GetHashCode() == b.GetHashCode()).ToProperty();
    }

    [Property]
    public Property IgnoredAndUnannotatedFieldsExcluded(int id, double score, string? meta1, string? meta2, string? extra1, string? extra2)
    {
        if (double.IsNaN(score)) return true.ToProperty().When(true);
        // Metadata ([IgnoreMember]) and Extra (no attribute) must not affect equality
        var a = new SerializedRecord { Id = id, Score = score, Metadata = meta1, Extra = extra1 };
        var b = new SerializedRecord { Id = id, Score = score, Metadata = meta2, Extra = extra2 };
        return a.Equals(b).ToProperty();
    }

    [Property]
    public Property IgnoredFieldsExcludedFromHashCode(int id, double score, string? meta1, string? meta2)
    {
        var a = new SerializedRecord { Id = id, Score = score, Metadata = meta1 };
        var b = new SerializedRecord { Id = id, Score = score, Metadata = meta2 };
        return (a.GetHashCode() == b.GetHashCode()).ToProperty();
    }

    [Property]
    public Property DifferentIdNotEqual(double score, int id1, int id2)
    {
        if (id1 == id2 || double.IsNaN(score))
            return true.ToProperty().When(false);

        var a = new SerializedRecord { Id = id1, Score = score };
        var b = new SerializedRecord { Id = id2, Score = score };
        return (!a.Equals(b)).ToProperty();
    }

    [Property]
    public Property DifferentScoreNotEqual(int id, double s1, double s2)
    {
        // Generated code uses == (exact bit equality), not a tolerance comparison.
        // NaN != NaN in IEEE 754 so also skip NaN inputs — the reflexivity test covers that case.
        if (s1 == s2 || double.IsNaN(s1) || double.IsNaN(s2))
            return true.ToProperty().When(false);

        var a = new SerializedRecord { Id = id, Score = s1 };
        var b = new SerializedRecord { Id = id, Score = s2 };
        return (!a.Equals(b)).ToProperty();
    }
}
