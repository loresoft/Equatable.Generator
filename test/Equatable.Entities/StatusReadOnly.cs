using System;

using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public readonly partial struct StatusReadOnly
{
    public StatusReadOnly(int id, string name, string? description, int displayOrder, bool isActive)
    {
        Id = id;
        Name = name;
        Description = description;
        DisplayOrder = displayOrder;
        IsActive = isActive;
    }

    public int Id { get; }
    public string Name { get; } = null!;
    public string? Description { get; }
    public int DisplayOrder { get; }
    public bool IsActive { get; }
}
