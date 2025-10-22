using System;

using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class TimeSheetSave
{
    public int Id { get; set; }

    public int CaseManagerId { get; set; }

    public DateTime PayPeriodFrom { get; set; }

    public DateTime PayPeriodTo { get; set; }


    public bool Submitted { get; set; }

    public string? EmployeeSignature { get; set; }


    public int? TimeSheetStatusId { get; set; }

    public int? TimeSheetApprovalId { get; set; }

    public DateTimeOffset? SignatureDate { get; set; }

    public string? Comments { get; set; }


    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    public string? CreatedBy { get; set; }

    public DateTimeOffset Updated { get; set; } = DateTimeOffset.UtcNow;

    public string? UpdatedBy { get; set; }
}
