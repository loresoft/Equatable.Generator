using Equatable.SourceGenerator.DataContract;

namespace Equatable.Generator.Tests;

public class DataContractGeneratorTest : AdapterGeneratorTestBase
{
    [Fact]
    public Task GenerateDataContractEquatable()
    {
        var source = @"
using System.Runtime.Serialization;
using Equatable.Attributes.DataContract;

namespace Equatable.Entities;

[DataContract]
[DataContractEquatable]
public partial class OrderDataContract
{
    [DataMember(Order = 0)]
    public int Id { get; set; }

    [DataMember(Order = 1)]
    public string? Name { get; set; }

    public string? InternalNote { get; set; }

    [IgnoreDataMember]
    public string? IgnoredField { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<DataContractEquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    // ── derived class from another [DataContractEquatable] base — must call base.Equals() ───────────
    // When both levels carry [DataContractEquatable], the derived class must delegate to
    // base.Equals() rather than re-including the base properties directly.

    [Fact]
    public Task GenerateDataContractEquatableDerivedFromDataContractEquatableBase()
    {
        var source = @"
using System.Runtime.Serialization;
using Equatable.Attributes.DataContract;

namespace Equatable.Entities;

[DataContract]
[DataContractEquatable]
public partial class DerivedContract : BaseContract
{
    [DataMember(Order = 2)]
    public int Rank { get; set; }
}

[DataContract]
[DataContractEquatable]
public partial class BaseContract
{
    [DataMember(Order = 0)]
    public int Id { get; set; }

    [DataMember(Order = 1)]
    public string? Name { get; set; }
}
";
        var (diagnostics, output) = GetNamedGeneratedOutput<DataContractEquatableGenerator>(source, "DerivedContract");
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    // ── derived class inherits base properties when base has no generator attribute ──────────────────
    // When the derived class carries [DataContractEquatable] but the base has no generator attribute,
    // the base's [DataMember] properties must be included directly (no base.Equals() delegation).

    [Fact]
    public Task GenerateDataContractEquatableDerivedIncludesUnannotatedBase()
    {
        var source = @"
using System.Runtime.Serialization;
using Equatable.Attributes.DataContract;

namespace Equatable.Entities;

[DataContract]
[DataContractEquatable]
public partial class ConcreteRecord : UnannotatedBase
{
    [DataMember(Order = 2)]
    public int Rank { get; set; }
}

public abstract class UnannotatedBase
{
    [DataMember(Order = 0)]
    public int Id { get; set; }

    [DataMember(Order = 1)]
    public string? Name { get; set; }
}
";
        var (diagnostics, output) = GetNamedGeneratedOutput<DataContractEquatableGenerator>(source, "ConcreteRecord");
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    // ── explicit comparer override ─────────────────────────────────────────────────────────────────
    // An explicit equality attribute on a [DataMember] property must override the inferred comparer.

    [Fact]
    public Task GenerateDataContractEquatableWithOrderedDictionaryOverride()
    {
        var source = @"
using System.Collections.Generic;
using System.Runtime.Serialization;
using Equatable.Attributes;
using Equatable.Attributes.DataContract;

namespace Equatable.Entities;

[DataContract]
[DataContractEquatable]
public partial class OrderedContract
{
    [DataMember(Order = 0)]
    public int Id { get; set; }

    [DataMember(Order = 1)]
    [DictionaryEquality(sequential: true)]
    public Dictionary<string, int>? Tags { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<DataContractEquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    // ── inferred collection comparers ─────────────────────────────────────────────────────────────
    // [DataMember] collection properties with no explicit equality attribute get structural comparers
    // inferred automatically by InferCollectionComparer.

    [Fact]
    public Task GenerateDataContractEquatableWithInferredCollectionComparers()
    {
        var source = @"
using System.Collections.Generic;
using System.Runtime.Serialization;
using Equatable.Attributes.DataContract;

namespace Equatable.Entities;

[DataContract]
[DataContractEquatable]
public partial class ContractWithCollections
{
    [DataMember(Order = 0)]
    public int Id { get; set; }

    [DataMember(Order = 1)]
    public Dictionary<string, int>? Tags { get; set; }

    [DataMember(Order = 2)]
    public List<string>? Labels { get; set; }

    [DataMember(Order = 3)]
    public int[]? Codes { get; set; }

    [DataMember(Order = 4)]
    public IReadOnlyDictionary<string, double>? Rates { get; set; }

    public string? NotIncluded { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<DataContractEquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    // ── nested collection comparers ───────────────────────────────────────────────────────────────
    // Adapter inference must recurse into nested types and compose structural comparers.
    // e.g. Dictionary<string, List<int>> → DictionaryEqualityComparer with SequenceEqualityComparer
    //      List<Dictionary<string, int>> → SequenceEqualityComparer with DictionaryEqualityComparer

    [Fact]
    public Task GenerateDataContractEquatableWithNestedCollectionComparers()
    {
        var source = @"
using System.Collections.Generic;
using System.Runtime.Serialization;
using Equatable.Attributes.DataContract;

namespace Equatable.Entities;

[DataContract]
[DataContractEquatable]
public partial class ContractWithNestedCollections
{
    [DataMember(Order = 0)]
    public int Id { get; set; }

    [DataMember(Order = 1)]
    public Dictionary<string, List<int>>? TagGroups { get; set; }

    [DataMember(Order = 2)]
    public Dictionary<string, Dictionary<string, int>>? NestedMap { get; set; }

    [DataMember(Order = 3)]
    public List<Dictionary<string, int>>? Records { get; set; }

    [DataMember(Order = 4)]
    public IReadOnlyDictionary<string, List<string>>? ReadOnlyTagGroups { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<DataContractEquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    // ── all properties excluded edge case ─────────────────────────────────────────────────────────
    // When no properties survive the filter the generated Equals reduces to !(other is null).

    [Fact]
    public Task GenerateDataContractEquatableWithNoIncludedProperties()
    {
        var source = @"
using System.Runtime.Serialization;
using Equatable.Attributes.DataContract;

namespace Equatable.Entities;

[DataContract]
[DataContractEquatable]
public partial class AllIgnored
{
    [IgnoreDataMember]
    public int Id { get; set; }

    public string? InternalNote { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<DataContractEquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateDataContractEquatableWithHashSetEqualityOnListAndArray()
    {
        var source = @"
using System.Collections.Generic;
using System.Runtime.Serialization;
using Equatable.Attributes;
using Equatable.Attributes.DataContract;

namespace Equatable.Entities;

[DataContract]
[DataContractEquatable]
public partial class HashSetOverrideContract
{
    [DataMember(Order = 0)]
    public int Id { get; set; }

    [DataMember(Order = 1)]
    [HashSetEquality]
    public List<string>? Tags { get; set; }

    [DataMember(Order = 2)]
    [HashSetEquality]
    public int[]? Codes { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<DataContractEquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateDataContractEquatableWithSequenceEqualityOnHashSet()
    {
        var source = @"
using System.Collections.Generic;
using System.Runtime.Serialization;
using Equatable.Attributes;
using Equatable.Attributes.DataContract;

namespace Equatable.Entities;

[DataContract]
[DataContractEquatable]
public partial class SequenceOverrideContract
{
    [DataMember(Order = 0)]
    public int Id { get; set; }

    [DataMember(Order = 1)]
    [SequenceEquality]
    public HashSet<string>? Tags { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<DataContractEquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateDataContractEquatableWithSequentialNestedDictPropagation()
    {
        var source = @"
using System.Collections.Generic;
using System.Runtime.Serialization;
using Equatable.Attributes;
using Equatable.Attributes.DataContract;

namespace Equatable.Entities;

[DataContract]
[DataContractEquatable]
public partial class NestedOrderedContract
{
    [DataMember(Order = 0)]
    public int Id { get; set; }

    [DataMember(Order = 1)]
    [DictionaryEquality(sequential: true)]
    public Dictionary<string, Dictionary<string, int>>? NestedDicts { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<DataContractEquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }
}
