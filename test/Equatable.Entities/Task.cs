using System;

using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Task : ModelBase
{
    public int? StatusId { get; set; }

    public int? PriorityId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? DueDate { get; set; }

    public DateTimeOffset? CompleteDate { get; set; }

    public int? AssignedId { get; set; }


    public Priority? Priority { get; set; }

    public Status? Status { get; set; }

    public User? AssignedUser { get; set; }
}
