using System;

using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class StatusReadOnly
{
    public int Id { get; init; }

    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }

    public DateTimeOffset Created { get; init; }
    public string? CreatedBy { get; init; }
    public DateTimeOffset Updated { get; init; }
    public string? UpdatedBy { get; init; }
    public long RowVersion { get; init; }
}
