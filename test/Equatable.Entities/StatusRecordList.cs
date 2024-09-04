using System;
using System.Collections.Generic;

using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial record StatusRecordList(
    int Id,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    List<string> Versions
);
