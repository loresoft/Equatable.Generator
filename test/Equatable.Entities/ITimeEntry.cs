using System;

namespace Equatable.Entities;

public interface ITimeEntry
{
    DateTime EntryDate { get; set; }

    DateTime? AmTimeIn { get; set; }
    DateTime? AmTimeOut { get; set; }

    DateTime? PmTimeIn { get; set; }
    DateTime? PmTimeOut { get; set; }

    decimal? OtherHours { get; set; }

    decimal? TotalHours { get; set; }

    DateTimeOffset Created { get; set; }
    DateTimeOffset Updated { get; set; }
}
