using System.Collections.Generic;

using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class TimeSheetGraph
{
    public TimeSheetSave TimeSheet { get; set; } = null!;

    [SequenceEquality]
    public List<TimeSheetWeek<TimeEntrySave>> TimeSheetWeeks { get; set; } = [];

    public decimal? OtherHours { get; set; }

    public decimal? TotalHours { get; set; }

    [IgnoreEquality]
    public decimal? WorkHours => TotalHours - (OtherHours ?? 0);
}
