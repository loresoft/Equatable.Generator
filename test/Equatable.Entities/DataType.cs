using System;

using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class DataType
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public bool Boolean { get; set; }

    public short Short { get; set; }

    public long Long { get; set; }

    public float Float { get; set; }

    public double Double { get; set; }

    public decimal Decimal { get; set; }

    public DateTime DateTime { get; set; }

    public DateTimeOffset DateTimeOffset { get; set; }

    public Guid Guid { get; set; }

    public TimeSpan TimeSpan { get; set; }

#if NET6_0_OR_GREATER
    public DateOnly DateOnly { get; set; }

    public TimeOnly TimeOnly { get; set; }
#endif

    public bool? BooleanNull { get; set; }

    public short? ShortNull { get; set; }

    public long? LongNull { get; set; }

    public float? FloatNull { get; set; }

    public double? DoubleNull { get; set; }

    public decimal? DecimalNull { get; set; }

    public DateTime? DateTimeNull { get; set; }

    public DateTimeOffset? DateTimeOffsetNull { get; set; }

    public Guid? GuidNull { get; set; }

    public TimeSpan? TimeSpanNull { get; set; }

#if NET6_0_OR_GREATER
    public DateOnly? DateOnlyNull { get; set; }

    public TimeOnly? TimeOnlyNull { get; set; }
#endif
}
