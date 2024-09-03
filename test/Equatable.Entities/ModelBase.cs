using System;

using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public abstract partial class ModelBase
{
    public int Id { get; set; }

    public DateTimeOffset Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset Updated { get; set; }

    public string? UpdatedBy { get; set; }

    public long RowVersion { get; set; }
}
