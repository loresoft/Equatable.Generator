using System;

using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public sealed partial record StatusRecord(
    int Id,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    DateTimeOffset Created,
    string? CreatedBy,
    DateTimeOffset Updated,
    string? UpdatedBy,
    long RowVersion
);
