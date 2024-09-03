using System;

using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class StatusConstructor
{
    public StatusConstructor(
        int id,
        string name,
        string description,
        bool isActive,
        int displayOrder,
        DateTimeOffset created,
        string createdBy,
        DateTimeOffset updated,
        string updatedBy,
        long rowVersion)
    {
        Id = id;
        Name = name;
        Description = description;
        DisplayOrder = displayOrder;
        IsActive = isActive;
        Created = created;
        CreatedBy = createdBy;
        Updated = updated;
        UpdatedBy = updatedBy;
        RowVersion = rowVersion;
    }

    public int Id { get; }
    public string Name { get; }
    public string Description { get; }
    public int DisplayOrder { get; }
    public bool IsActive { get; }
    public DateTimeOffset Created { get; }
    public string CreatedBy { get; }
    public DateTimeOffset Updated { get; }
    public string UpdatedBy { get; }

    public long RowVersion { get; }
}
