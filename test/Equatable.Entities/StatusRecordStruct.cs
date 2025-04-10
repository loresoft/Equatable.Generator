using System;

using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial record struct StatusRecordStruct(
    int Id,
    [property: StringEquality(StringComparison.OrdinalIgnoreCase)] string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    DateTimeOffset Created,
    string? CreatedBy,
    DateTimeOffset Updated,
    string? UpdatedBy,
    long RowVersion
);
