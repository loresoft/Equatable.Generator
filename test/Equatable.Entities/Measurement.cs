using System;

using Equatable.Attributes;

namespace Equatable.Entities;

/// <summary>A struct that does not declare an equality operator.</summary>
public struct Weight
{
    public double Value { get; set; }

    public string? Unit { get; set; }
}

/// <summary>A struct that declares an equality operator.</summary>
public struct Distance : IEquatable<Distance>
{
    public double Value { get; set; }

    public bool Equals(Distance other) => Value.Equals(other.Value);

    public override bool Equals(object? obj) => obj is Distance other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Distance left, Distance right) => left.Equals(right);

    public static bool operator !=(Distance left, Distance right) => !left.Equals(right);
}

/// <summary>An enum used to verify nullable enum handling.</summary>
public enum Accuracy
{
    Low,
    Medium,
    High
}

[Equatable]
public partial class Measurement
{
    // struct without operator ==, must fall back to EqualityComparer<T>.Default
    public Weight Weight { get; set; }

    public Weight? OptionalWeight { get; set; }

    // struct with operator ==, must use the == fast path
    public Distance Distance { get; set; }

    public Distance? OptionalDistance { get; set; }

    // nullable value types where null must not hash the same as the default value
    public bool? IsVerified { get; set; }

    public int? SampleCount { get; set; }

    public Accuracy? Accuracy { get; set; }
}
