using System.Collections.Immutable;

using Equatable.Attributes;
using Equatable.SourceGenerator;
using Equatable.SourceGenerator.DataContract;
using Equatable.SourceGenerator.MessagePack;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Equatable.Generator.Tests;

public class EquatableGeneratorTest
{
    [Fact]
    public Task GenerateUserImport()
    {
        var source = @"
using System;
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    [StringEquality(StringComparison.OrdinalIgnoreCase)]
    public string EmailAddress { get; set; } = null!;

    [JsonPropertyName(""name"")]
    public string? DisplayName { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public DateTimeOffset? LastLogin { get; set; }

    [JsonIgnore]
    [IgnoreEquality]
    public string FullName => $""{FirstName} {LastName}"";

    [HashSetEquality]
    public HashSet<string>? Roles { get; set; }

    [DictionaryEquality]
    public Dictionary<string, int>? Permissions { get; set; }

    [SequenceEquality]
    public List<DateTimeOffset>? History { get; set; }
}
";

        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);

        Assert.Empty(diagnostics);

        return Verifier
            .Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GeneratePriorityBaseEquatable()
    {
        var source = @"
using System;
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public abstract partial class ModelBase
{
    public int Id { get; set; }
    public DateTimeOffset Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset Updated { get; set; }
    public string? UpdatedBy { get; set; }
    public long RowVersion { get; set; }
}

[Equatable]
public partial class Priority : ModelBase
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
";

        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);

        Assert.Empty(diagnostics);

        return Verifier
            .Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GeneratePriorityBase()
    {
        var source = @"
using System;
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

public abstract partial class ModelBase : IEquatable<ModelBase?>
{
    public int Id { get; set; }
    public DateTimeOffset Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset Updated { get; set; }
    public string? UpdatedBy { get; set; }
    public long RowVersion { get; set; }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ModelBase);
    }

    public bool Equals(ModelBase? other)
    {
        return other is not null &&
               Id == other.Id &&
               Created.Equals(other.Created) &&
               CreatedBy == other.CreatedBy &&
               Updated.Equals(other.Updated) &&
               UpdatedBy == other.UpdatedBy &&
               RowVersion == other.RowVersion;
    }

    public override int GetHashCode()
    {
        int hashCode = -172088165;
        hashCode = hashCode * -1521134295 + Id.GetHashCode();
        hashCode = hashCode * -1521134295 + Created.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(CreatedBy);
        hashCode = hashCode * -1521134295 + Updated.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(UpdatedBy);
        hashCode = hashCode * -1521134295 + RowVersion.GetHashCode();
        return hashCode;
    }

    public static bool operator ==(ModelBase? left, ModelBase? right)
    {
        return EqualityComparer<ModelBase>.Default.Equals(left, right);
    }

    public static bool operator !=(ModelBase? left, ModelBase? right)
    {
        return !(left == right);
    }
}

[Equatable]
public partial class Priority : ModelBase
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
";

        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);

        Assert.Empty(diagnostics);

        return Verifier
            .Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateRecordSealed()
    {
        var source = @"
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
";

        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);

        Assert.Empty(diagnostics);

        return Verifier
            .Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateRecordSequence()
    {
        var source = @"
using System;
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial record StatusRecordList(
    int Id,
    [property: StringEquality(StringComparison.OrdinalIgnoreCase)] string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    [property: SequenceEquality] List<string> Versions
);
";

        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);

        Assert.Empty(diagnostics);

        return Verifier
            .Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateStructReadOnly()
    {
        var source = @"
using System;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public readonly partial struct StatusReadOnly
{
    public StatusReadOnly(int id, string name, string? description, int displayOrder, bool isActive)
    {
        Id = id;
        Name = name;
        Description = description;
        DisplayOrder = displayOrder;
        IsActive = isActive;
    }

    public int Id { get; }
    public string Name { get; } = null!;
    public string? Description { get; }
    public int DisplayOrder { get; }
    public bool IsActive { get; }
}
";

        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);

        Assert.Empty(diagnostics);

        return Verifier
            .Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateCustomComparer()
    {
        var source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class CustomComparer
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    [EqualityComparer(typeof(LengthEqualityComparer))]
    public string? Key { get; set; }
}

public class LengthEqualityComparer : IEqualityComparer<string?>
{
    public static readonly LengthEqualityComparer Default = new();

    public bool Equals(string? x, string? y) => x?.Length == y?.Length;

    public int GetHashCode(string? obj) => obj?.Length.GetHashCode() ?? 0;
}
";

        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);

        Assert.Empty(diagnostics);

        return Verifier
            .Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateReferenceComparer()
    {
        var source = @"
using System;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Audit
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int? UserId { get; set; }
    public int? TaskId { get; set; }
    public string? Content { get; set; }
    public string? UserName { get; set; }

    [ReferenceEquality]
    public object? Lock { get; set; }
}
";

        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);

        Assert.Empty(diagnostics);

        return Verifier
            .Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateNestedComparer()
    {
        var source = @"
using Equatable.Attributes;

namespace Equatable.Entities;

public partial class Nested
{
    [Equatable]
    public partial class Animal
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
    }
}
";

        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);

        Assert.Empty(diagnostics);

        return Verifier
            .Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateInvalidStringEquality()
    {
        var source = @"
using System;
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    public string EmailAddress { get; set; } = null!;

    [JsonPropertyName(""name"")]
    public string? DisplayName { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    [StringEquality(StringComparison.OrdinalIgnoreCase)]
    public DateTimeOffset? LockoutEnd { get; set; }

    public DateTimeOffset? LastLogin { get; set; }
}
";

        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);

        return Verifier
            .Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateInvalidSequenceEquality()
    {
        var source = @"
using System;
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    [StringEquality(StringComparison.OrdinalIgnoreCase)]
    public string EmailAddress { get; set; } = null!;

    [JsonPropertyName(""name"")]
    public string? DisplayName { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    [SequenceEquality]
    public DateTimeOffset? LastLogin { get; set; }
}
";

        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);

        return Verifier
            .Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateInvalidHashSetEquality()
    {
        var source = @"
using System;
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    [StringEquality(StringComparison.OrdinalIgnoreCase)]
    public string EmailAddress { get; set; } = null!;

    [JsonPropertyName(""name"")]
    public string? DisplayName { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    [HashSetEquality]
    public DateTimeOffset? LastLogin { get; set; }
}
";

        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);

        return Verifier
            .Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateInvalidDictionaryEquality()
    {
        var source = @"
using System;
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    [StringEquality(StringComparison.OrdinalIgnoreCase)]
    public string EmailAddress { get; set; } = null!;

    [JsonPropertyName(""name"")]
    public string? DisplayName { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    [DictionaryEquality]
    public DateTimeOffset? LastLogin { get; set; }
}
";

        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);

        return Verifier
            .Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateSequenceEqualityMultiDimensionalArray()
    {
        var source = @"
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Grid
{
    [SequenceEquality]
    public int[,]? Cells { get; set; }

    public int Id { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);

        return Verifier
            .Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateReadOnlyDictionary()
    {
        var source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class LookupTable
{
    [DictionaryEquality]
    public IReadOnlyDictionary<string, double>? FlatEntries { get; set; }
}
";

        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);

        Assert.Empty(diagnostics);

        return Verifier
            .Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateNestedDictionary()
    {
        var source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class LookupTable
{
    [DictionaryEquality]
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, double>>? NestedEntries { get; set; }
}
";

        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);

        Assert.Empty(diagnostics);

        return Verifier
            .Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateNestedSequenceInDictionary()
    {
        var source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class LookupTable
{
    [DictionaryEquality]
    public IReadOnlyDictionary<string, List<double>>? NestedEntries { get; set; }
}
";

        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);

        Assert.Empty(diagnostics);

        return Verifier
            .Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateSequenceOfDictionaries()
    {
        var source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class LookupTable
{
    [SequenceEquality]
    public List<IReadOnlyDictionary<string, double>>? Items { get; set; }
}
";

        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);

        Assert.Empty(diagnostics);

        return Verifier
            .Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateDictOfLists()
    {
        var source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class NestedCollections
{
    [DictionaryEquality]
    public Dictionary<string, List<int>>? DictOfLists { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateListOfDicts()
    {
        var source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class NestedCollections
{
    [SequenceEquality]
    public List<Dictionary<string, int>>? ListOfDicts { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateThreeLevelNested()
    {
        var source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class NestedCollections
{
    [DictionaryEquality]
    public Dictionary<string, Dictionary<string, List<int>>>? ThreeLevelNested { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    // ── base class with non-[Equatable] generator attribute ───────────────────────────────────────
    // These tests guard against the GetBaseEquatableType bug: only "EquatableAttribute" was checked,
    // so a derived [Equatable] class whose base carries [DataContractEquatable] or
    // [MessagePackEquatable] would silently omit base.Equals()/base.GetHashCode() calls.

    [Fact]
    public Task GenerateDerivedFromDataContractEquatableBase()
    {
        var source = @"
using System.Runtime.Serialization;
using Equatable.Attributes;
using Equatable.Attributes.DataContract;

namespace Equatable.Entities;

[Equatable]
public partial class ConcreteRecord : ContractBase
{
    public int Rank { get; set; }
}

[DataContract]
[DataContractEquatable]
public abstract partial class ContractBase
{
    [DataMember(Order = 0)]
    public int Id { get; set; }

    [DataMember(Order = 1)]
    public string? Name { get; set; }
}
";
        var (diagnostics, output) = GetNamedGeneratedOutput<EquatableGenerator>(source, "ConcreteRecord");
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateDerivedFromMessagePackEquatableBase()
    {
        var source = @"
using MessagePack;
using Equatable.Attributes;
using Equatable.Attributes.MessagePack;

namespace Equatable.Entities;

[Equatable]
public partial class ConcreteRecord : PackedBase
{
    public string? Label { get; set; }
}

[MessagePackObject]
[MessagePackEquatable]
public abstract partial class PackedBase
{
    [Key(0)]
    public int Id { get; set; }

    [Key(1)]
    public double Score { get; set; }
}
";
        var (diagnostics, output) = GetNamedGeneratedOutput<EquatableGenerator>(source, "ConcreteRecord");
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    // ── interface-typed collection properties ──────────────────────────────────────────────────────
    // These tests guard against regression of the ValidateComparer bug: interface types do not appear
    // in their own AllInterfaces list, so the direct-type check must come first.

    [Fact]
    public Task GenerateIDictionaryEquality()
    {
        var source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Container
{
    [DictionaryEquality]
    public IDictionary<string, int>? Entries { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateSequentialDictionaryEquality()
    {
        var source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Container
{
    [DictionaryEquality(sequential: true)]
    public Dictionary<string, int>? Entries { get; set; }

    [DictionaryEquality(sequential: true)]
    public IReadOnlyDictionary<string, int>? ReadOnlyEntries { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateIEnumerableSequenceEquality()
    {
        var source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Container
{
    [SequenceEquality]
    public IEnumerable<int>? Items { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateIListSequenceEquality()
    {
        var source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Container
{
    [SequenceEquality]
    public IList<int>? Items { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateIReadOnlyListSequenceEquality()
    {
        var source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Container
{
    [SequenceEquality]
    public IReadOnlyList<int>? Items { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateISetHashSetEquality()
    {
        var source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Container
{
    [HashSetEquality]
    public ISet<int>? Tags { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateIReadOnlyCollectionSequenceEquality()
    {
        var source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Container
{
    [SequenceEquality]
    public IReadOnlyCollection<int>? Items { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    // ── zero-property edge case ───────────────────────────────────────────────────────────────────
    // A class with [Equatable] and no public properties should generate an Equals body that
    // reduces to !(other is null) with an empty GetHashCode.

    [Fact]
    public Task GenerateEquatableWithNoPublicProperties()
    {
        var source = @"
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Empty
{
}
";
        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateHashSetEqualityOnListAndArray()
    {
        var source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Container
{
    /// List is normally order-sensitive; [HashSetEquality] makes it order-insensitive.
    [HashSetEquality]
    public List<string>? Tags { get; set; }

    /// Array is normally order-sensitive; [HashSetEquality] makes it order-insensitive.
    [HashSetEquality]
    public int[]? Codes { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateSequenceEqualityOnHashSet()
    {
        var source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Container
{
    /// HashSet is normally order-insensitive; [SequenceEquality] makes it order-sensitive.
    [SequenceEquality]
    public HashSet<string>? Tags { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateHashSetEqualityPropagatesIntoNestedCollections()
    {
        var source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Container
{
    /// [HashSetEquality] on List<int[]>: List uses HashSet, inner array also uses HashSet.
    [HashSetEquality]
    public List<int[]>? ListOfArrays { get; set; }

    /// [HashSetEquality] on List<List<string>>: both levels use HashSet.
    [HashSetEquality]
    public List<List<string>>? ListOfLists { get; set; }

    /// [HashSetEquality] on int[][]: both levels use HashSet.
    [HashSetEquality]
    public int[][]? JaggedArray { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateSequenceEqualityPropagatesIntoNestedCollections()
    {
        var source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Container
{
    /// [SequenceEquality] on HashSet<HashSet<string>>: both levels use Sequence.
    [SequenceEquality]
    public HashSet<HashSet<string>>? SetOfSets { get; set; }

    /// [SequenceEquality] on HashSet<int[]>: outer HashSet uses Sequence, inner array also uses Sequence.
    [SequenceEquality]
    public HashSet<int[]>? SetOfArrays { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateSequentialDictionaryEquality_NestedDictPropagatesOrderedComparer()
    {
        var source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Container
{
    [DictionaryEquality(sequential: true)]
    public Dictionary<string, Dictionary<string, int>>? NestedDicts { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    // ── dictKind propagation ──────────────────────────────────────────────────────────────────────
    // When [DictionaryEquality] / [DictionaryEquality(sequential:true)] is set explicitly, the
    // annotated kind propagates into ALL nested dictionary levels.  Nested enumerables (List, array,
    // HashSet) keep their natural comparer regardless.

    [Fact]
    public Task GenerateDictionaryEqualityPropagatesIntoNestedDictionaries()
    {
        var source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Container
{
    /// [DictionaryEquality(sequential:true)] on 3-level nest: all dict levels use Ordered.
    [DictionaryEquality(sequential: true)]
    public Dictionary<string, Dictionary<string, Dictionary<string, int>>>? ThreeLevelOrdered { get; set; }

    /// [DictionaryEquality(sequential:true)] on dict-of-list: dict is ordered, inner list is natural Sequence.
    [DictionaryEquality(sequential: true)]
    public Dictionary<string, List<int>>? OrderedDictOfList { get; set; }

    /// [DictionaryEquality(sequential:true)] on dict-of-dict-of-list: both dict levels ordered, inner list natural.
    [DictionaryEquality(sequential: true)]
    public Dictionary<string, Dictionary<string, List<int>>>? OrderedDictOfDictOfList { get; set; }

    /// [DictionaryEquality] (unordered) on dict-of-dict: both dict levels unordered.
    [DictionaryEquality]
    public Dictionary<string, Dictionary<string, int>>? UnorderedNestedDict { get; set; }
}
";
        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);
        Assert.Empty(diagnostics);
        return Verifier.Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    // Pinned references that must always be present regardless of AppDomain load order.
    // Adapter attribute assemblies and serialization libraries may not be loaded when a test runs first.
    private static readonly IEnumerable<MetadataReference> PinnedReferences =
    [
        MetadataReference.CreateFromFile(typeof(System.Runtime.Serialization.DataMemberAttribute).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(MessagePack.KeyAttribute).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Equatable.Attributes.DataContract.DataContractEquatableAttribute).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Equatable.Attributes.MessagePack.MessagePackEquatableAttribute).Assembly.Location),
    ];

    private static IEnumerable<MetadataReference> BuildReferences<T>()
        where T : IIncrementalGenerator, new()
        => AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Concat(
            [
                MetadataReference.CreateFromFile(typeof(T).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(EquatableAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DataContractEquatableGenerator).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(MessagePackEquatableGenerator).Assembly.Location),
            ])
            .Concat(PinnedReferences);

    private static (ImmutableArray<Diagnostic> Diagnostics, string Output) GetGeneratedOutput<T>(string source)
        where T : IIncrementalGenerator, new()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var compilation = CSharpCompilation.Create(
            "Test.Generator",
            [syntaxTree],
            BuildReferences<T>(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var originalTreeCount = compilation.SyntaxTrees.Length;
        var generator = new T();

        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var trees = outputCompilation.SyntaxTrees.ToList();

        return (diagnostics, trees.Count != originalTreeCount ? trees[^1].ToString() : string.Empty);
    }

    private static (ImmutableArray<Diagnostic> Diagnostics, string Output) GetNamedGeneratedOutput<T>(string source, string typeName)
        where T : IIncrementalGenerator, new()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var compilation = CSharpCompilation.Create(
            "Test.Generator",
            [syntaxTree],
            BuildReferences<T>(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var originalTreeCount = compilation.SyntaxTrees.Length;
        var generator = new T();

        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var generated = outputCompilation.SyntaxTrees.Skip(originalTreeCount).ToList();
        var match = generated.FirstOrDefault(t => t.ToString().Contains($"partial class {typeName}"))
            ?? (generated.Count > 0 ? generated[^1] : null);

        return (diagnostics, match?.ToString() ?? string.Empty);
    }
}
