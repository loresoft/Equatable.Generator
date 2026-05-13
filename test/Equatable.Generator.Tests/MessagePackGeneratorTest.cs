using Equatable.SourceGenerator.MessagePack;

namespace Equatable.Generator.Tests;

public class MessagePackGeneratorTest : AdapterGeneratorTestBase
{
    [Fact]
    public Task GenerateMessagePackEquatable()
    {
        var source = @"
using MessagePack;
using Equatable.Attributes.MessagePack;

namespace Equatable.Entities;

[MessagePackObject]
[MessagePackEquatable]
public partial class OrderDataContract
{
    [Key(0)]
    public int Id { get; set; }

    [Key(1)]
    public string? Name { get; set; }

    public string? InternalNote { get; set; }

    [IgnoreMember]
    public string? IgnoredField { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<MessagePackEquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    // ── derived class from another [MessagePackEquatable] base — must call base.Equals() ────────────
    // When both levels carry [MessagePackEquatable], the derived class must delegate to
    // base.Equals() rather than re-including the base properties directly.

    [Fact]
    public Task GenerateMessagePackEquatableDerivedFromMessagePackEquatableBase()
    {
        var source = @"
using MessagePack;
using Equatable.Attributes.MessagePack;

namespace Equatable.Entities;

[MessagePackObject]
[MessagePackEquatable]
public partial class DerivedContract : BaseContract
{
    [Key(2)]
    public int Rank { get; set; }
}

[MessagePackObject]
[MessagePackEquatable]
public partial class BaseContract
{
    [Key(0)]
    public int Id { get; set; }

    [Key(1)]
    public string? Name { get; set; }
}
";
        var (diagnostics, output) = GetNamedGeneratedOutput<MessagePackEquatableGenerator>(source, "DerivedContract");
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    // ── derived class inherits base properties when base has no generator attribute ──────────────────
    // When the derived class carries [MessagePackEquatable] but the base has no generator attribute,
    // the base's [Key] properties must be included directly (no base.Equals() delegation).

    [Fact]
    public Task GenerateMessagePackEquatableDerivedIncludesUnannotatedBase()
    {
        var source = @"
using MessagePack;
using Equatable.Attributes.MessagePack;

namespace Equatable.Entities;

[MessagePackObject]
[MessagePackEquatable]
public partial class ConcreteRecord : UnannotatedBase
{
    [Key(2)]
    public int Rank { get; set; }
}

public abstract class UnannotatedBase
{
    [Key(0)]
    public int Id { get; set; }

    [Key(1)]
    public string? Name { get; set; }
}
";
        var (diagnostics, output) = GetNamedGeneratedOutput<MessagePackEquatableGenerator>(source, "ConcreteRecord");
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    // ── explicit comparer override ─────────────────────────────────────────────────────────────────
    // An explicit equality attribute on a [Key] property must override the inferred comparer.

    [Fact]
    public Task GenerateMessagePackEquatableWithOrderedDictionaryOverride()
    {
        var source = @"
using System.Collections.Generic;
using MessagePack;
using Equatable.Attributes;
using Equatable.Attributes.MessagePack;

namespace Equatable.Entities;

[MessagePackObject]
[MessagePackEquatable]
public partial class OrderedContract
{
    [Key(0)]
    public int Id { get; set; }

    [Key(1)]
    [DictionaryEquality(sequential: true)]
    public Dictionary<string, int>? Tags { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<MessagePackEquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    // ── inferred collection comparers ─────────────────────────────────────────────────────────────
    // [Key] collection properties with no explicit equality attribute get structural comparers
    // inferred automatically by InferCollectionComparer.

    [Fact]
    public Task GenerateMessagePackEquatableWithInferredCollectionComparers()
    {
        var source = @"
using System.Collections.Generic;
using MessagePack;
using Equatable.Attributes.MessagePack;

namespace Equatable.Entities;

[MessagePackObject]
[MessagePackEquatable]
public partial class ContractWithCollections
{
    [Key(0)]
    public int Id { get; set; }

    [Key(1)]
    public Dictionary<string, int>? Tags { get; set; }

    [Key(2)]
    public List<string>? Labels { get; set; }

    [Key(3)]
    public int[]? Codes { get; set; }

    [Key(4)]
    public IReadOnlyDictionary<string, double>? Rates { get; set; }

    public string? NotIncluded { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<MessagePackEquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    // ── nested collection comparers ───────────────────────────────────────────────────────────────
    // Adapter inference must recurse into nested types and compose structural comparers.
    // e.g. Dictionary<string, List<int>> → DictionaryEqualityComparer with SequenceEqualityComparer
    //      List<Dictionary<string, int>> → SequenceEqualityComparer with DictionaryEqualityComparer

    [Fact]
    public Task GenerateMessagePackEquatableWithNestedCollectionComparers()
    {
        var source = @"
using System.Collections.Generic;
using MessagePack;
using Equatable.Attributes.MessagePack;

namespace Equatable.Entities;

[MessagePackObject]
[MessagePackEquatable]
public partial class ContractWithNestedCollections
{
    [Key(0)]
    public int Id { get; set; }

    [Key(1)]
    public Dictionary<string, List<int>>? TagGroups { get; set; }

    [Key(2)]
    public Dictionary<string, Dictionary<string, int>>? NestedMap { get; set; }

    [Key(3)]
    public List<Dictionary<string, int>>? Records { get; set; }

    [Key(4)]
    public IReadOnlyDictionary<string, List<string>>? ReadOnlyTagGroups { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<MessagePackEquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    // ── all properties excluded edge case ─────────────────────────────────────────────────────────
    // When no properties survive the filter the generated Equals reduces to !(other is null).

    [Fact]
    public Task GenerateMessagePackEquatableWithNoIncludedProperties()
    {
        var source = @"
using MessagePack;
using Equatable.Attributes.MessagePack;

namespace Equatable.Entities;

[MessagePackObject]
[MessagePackEquatable]
public partial class AllIgnored
{
    [IgnoreMember]
    public int Id { get; set; }

    public string? InternalNote { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<MessagePackEquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }
}
