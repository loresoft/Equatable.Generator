using System.Collections.Generic;

using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class LookupTable
{
    [DictionaryEquality]
    public IReadOnlyDictionary<string, double>? FlatEntries { get; set; }

    // nested: generator auto-composes ReadOnlyDictionaryEqualityComparer for the value type
    [DictionaryEquality]
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, double>>? NestedEntries { get; set; }
}
