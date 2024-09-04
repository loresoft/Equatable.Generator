using System;
using System.Collections.Generic;

using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Audit : ModelBase
{
    public DateTime Date { get; set; }
    public int? UserId { get; set; }
    public int? TaskId { get; set; }
    public string? Content { get; set; }
    public string? UserName { get; set; }
}
