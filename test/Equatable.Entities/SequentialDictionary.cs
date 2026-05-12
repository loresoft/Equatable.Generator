using System.Collections.Generic;

using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class SequentialDictionary
{
    [DictionaryEquality(sequential: true)]
    public Dictionary<string, int>? Entries { get; set; }

    [DictionaryEquality(sequential: true)]
    public IReadOnlyDictionary<string, int>? ReadOnlyEntries { get; set; }
}
