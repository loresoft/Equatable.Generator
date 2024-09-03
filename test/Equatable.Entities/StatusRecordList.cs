using System;
using System.Collections.Generic;

using Equatable.Attributes;

namespace Equatable.Entities;

public partial record StatusRecordList(
    int Id,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    DateTimeOffset Created,
    string? CreatedBy,
    DateTimeOffset Updated,
    string? UpdatedBy,
    long RowVersion,
    List<string> Versions
);
