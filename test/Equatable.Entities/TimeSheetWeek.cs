using System;
using System.Collections.Generic;

using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class TimeSheetWeek<TTimeEntry>
    where TTimeEntry : ITimeEntry
{
    public int WeekNumber { get; set; }

    public DateTime PayPeriodFrom { get; set; }

    public DateTime PayPeriodTo { get; set; }

    public decimal? OtherHours { get; set; }

    public decimal? TotalHours { get; set; }

    [SequenceEquality]
    public List<TTimeEntry> TimeEntries { get; set; } = [];

    [IgnoreEquality]
    public decimal? WorkHours => TotalHours - (OtherHours ?? 0);
}
