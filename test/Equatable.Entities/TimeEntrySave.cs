using System;

using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class TimeEntrySave : ITimeEntry
{
    public int Id { get; set; }

    public int TimeSheetId { get; set; }

    public DateTime EntryDate { get; set; }

    public DateTime? AmTimeIn { get; set; }

    public DateTime? AmTimeOut { get; set; }

    public DateTime? PmTimeIn { get; set; }

    public DateTime? PmTimeOut { get; set; }

    public decimal? OtherHours { get; set; }

    public int? OtherHourTypeId { get; set; }

    public decimal? TotalHours { get; set; }

    public bool Exported { get; set; }

    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    public string? CreatedBy { get; set; }

    public DateTimeOffset Updated { get; set; } = DateTimeOffset.UtcNow;

    public string? UpdatedBy { get; set; }

    [IgnoreEquality]
    public decimal? WorkHours => TotalHours - (OtherHours ?? 0);
}
