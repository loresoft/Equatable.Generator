using System.Collections.Generic;
using System.Runtime.Serialization;
using Equatable.Attributes;
using Equatable.Comparers;

namespace Equatable.Entities;

[Equatable]
public partial class MarketInputs
{
    [DictionaryEquality]
    public IReadOnlyDictionary<string, double>? FlatInputs { get; set; }

    [EqualityComparer(typeof(ReadOnlyDictionaryEqualityComparer<string, IReadOnlyDictionary<string, double>>))]
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, double>>? NestedInputs { get; set; }
}
