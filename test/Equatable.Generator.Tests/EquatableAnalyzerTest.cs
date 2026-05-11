using System.Collections.Immutable;

using Equatable.Attributes;
using Equatable.SourceGenerator;
using Equatable.SourceGenerator.DataContract;
using Equatable.SourceGenerator.MessagePack;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Equatable.Generator.Tests;

public class EquatableAnalyzerTest
{
    [Fact]
    public async Task AnalyzeValidUsage()
    {
        const string source = @"
using System;
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    [StringEquality(StringComparison.OrdinalIgnoreCase)]
    public string EmailAddress { get; set; } = null!;

    public string? FirstName { get; set; }

    [HashSetEquality]
    public HashSet<string>? Roles { get; set; }

    [DictionaryEquality]
    public Dictionary<string, int>? Permissions { get; set; }

    [SequenceEquality]
    public List<DateTimeOffset>? History { get; set; }
}
";

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task AnalyzeIgnoreEqualityExcluded()
    {
        const string source = @"
using System;
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    public string? FirstName { get; set; }

    [IgnoreEquality]
    public List<string>? Ignored { get; set; }
}
";

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task AnalyzeReferenceEqualityValid()
    {
        const string source = @"
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Audit
{
    public int Id { get; set; }

    [ReferenceEquality]
    public object? Lock { get; set; }
}
";

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task AnalyzeCustomComparerValid()
    {
        const string source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class CustomComparer
{
    public int Id { get; set; }

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

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task AnalyzeNotEquatableNoWarnings()
    {
        const string source = @"
using System.Collections.Generic;

namespace Equatable.Entities;

public class NotEquatable
{
    public List<string>? Items { get; set; }
    public Dictionary<string, int>? Map { get; set; }
}
";

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task AnalyzeInvalidStringEquality()
    {
        const string source = @"
using System;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    public string EmailAddress { get; set; } = null!;

    [StringEquality(StringComparison.OrdinalIgnoreCase)]
    public DateTimeOffset? LockoutEnd { get; set; }
}
";

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0010", diagnostic.Id);
        Assert.Contains("LockoutEnd", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeInvalidDictionaryEquality()
    {
        const string source = @"
using System;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    public string EmailAddress { get; set; } = null!;

    [DictionaryEquality]
    public DateTimeOffset? LastLogin { get; set; }
}
";

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0011", diagnostic.Id);
        Assert.Contains("LastLogin", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeInvalidHashSetEquality()
    {
        const string source = @"
using System;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    public string EmailAddress { get; set; } = null!;

    [HashSetEquality]
    public DateTimeOffset? LastLogin { get; set; }
}
";

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0012", diagnostic.Id);
        Assert.Contains("LastLogin", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeInvalidSequenceEquality()
    {
        const string source = @"
using System;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    public string EmailAddress { get; set; } = null!;

    [SequenceEquality]
    public DateTimeOffset? LastLogin { get; set; }
}
";

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0013", diagnostic.Id);
        Assert.Contains("LastLogin", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeMissingDictionaryAttribute()
    {
        const string source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    public string EmailAddress { get; set; } = null!;

    public Dictionary<string, int>? Permissions { get; set; }
}
";

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0001", diagnostic.Id);
        Assert.Contains("Permissions", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeMissingSequenceAttribute()
    {
        const string source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    public string EmailAddress { get; set; } = null!;

    public List<string>? Items { get; set; }
}
";

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0002", diagnostic.Id);
        Assert.Contains("Items", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeMissingHashSetAttribute()
    {
        const string source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    public string EmailAddress { get; set; } = null!;

    public HashSet<string>? Roles { get; set; }
}
";

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0002", diagnostic.Id);
        Assert.Contains("Roles", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeStringNoMissingWarning()
    {
        // string implements IEnumerable<char> but should NOT trigger EQ0002
        const string source = @"
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    public string? Name { get; set; }
}
";

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task AnalyzeDictionaryWithMissingAttributeEmitsDictionaryDiagnostic()
    {
        // Dictionary implements both IDictionary and IEnumerable, should emit EQ0001 (not EQ0002)
        const string source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    public Dictionary<string, int>? Data { get; set; }
}
";

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0001", diagnostic.Id);
    }

    [Fact]
    public async Task AnalyzeMultipleMissingAttributes()
    {
        const string source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    public Dictionary<string, int>? Permissions { get; set; }

    public List<string>? Items { get; set; }
}
";

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        Assert.Equal(2, diagnostics.Length);
        Assert.Contains(diagnostics, d => d.Id == "EQ0001" && d.GetMessage().Contains("Permissions"));
        Assert.Contains(diagnostics, d => d.Id == "EQ0002" && d.GetMessage().Contains("Items"));
    }

    [Fact]
    public async Task AnalyzeMissingDictionaryAttributeForIDictionary()
    {
        const string source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    public string EmailAddress { get; set; } = null!;

    public IDictionary<string, int>? Permissions { get; set; }
}
";

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0001", diagnostic.Id);
        Assert.Contains("Permissions", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeMissingSequenceAttributeForIEnumerable()
    {
        const string source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    public string EmailAddress { get; set; } = null!;

    public IEnumerable<string>? Items { get; set; }
}
";

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0002", diagnostic.Id);
        Assert.Contains("Items", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeMissingSequenceAttributeForIReadOnlyCollection()
    {
        const string source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    public string EmailAddress { get; set; } = null!;

    public IReadOnlyCollection<string>? Items { get; set; }
}
";

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0002", diagnostic.Id);
        Assert.Contains("Items", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeDictionaryEqualityOnIDictionaryIsValid()
    {
        // [DictionaryEquality] on an IDictionary<,>-typed property must NOT emit EQ0011
        const string source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    [DictionaryEquality]
    public IDictionary<string, int>? Permissions { get; set; }
}
";

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task AnalyzeSequenceEqualityOnIEnumerableIsValid()
    {
        // [SequenceEquality] on an IEnumerable<T>-typed property must NOT emit EQ0013
        const string source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    [SequenceEquality]
    public IEnumerable<string>? Items { get; set; }
}
";

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task AnalyzeBaseTypePropertiesIncludedWhenNoEquatableBase()
    {
        // Properties from a non-[Equatable] base class should be analyzed
        const string source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

public abstract class ModelBase
{
    public List<string>? Tags { get; set; }
}

[Equatable]
public partial class Priority : ModelBase
{
    public string Name { get; set; } = null!;
}
";

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0002", diagnostic.Id);
        Assert.Contains("Tags", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeBaseTypePropertiesExcludedWhenEquatableBase()
    {
        // Properties from a [Equatable] base class must NOT cause duplicate diagnostics on the derived type
        const string source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public abstract partial class ModelBase
{
    [SequenceEquality]
    public List<string>? Tags { get; set; }
}

[Equatable]
public partial class Priority : ModelBase
{
    public string Name { get; set; } = null!;
}
";

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task AnalyzeDataContractEquatableMissingDataContract()
    {
        const string source = @"
using System.Runtime.Serialization;
using Equatable.Attributes;

namespace Equatable.Entities;

[DataContractEquatable]
public partial class OrderDataContract
{
    [DataMember(Order = 0)]
    public int Id { get; set; }
}
";
        var diagnostics = await GetAnalyzerDiagnosticsAsync(source,
            new DataContractEquatableAnalyzer());

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0020", diagnostic.Id);
        Assert.Contains("OrderDataContract", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeDataContractEquatableWithDataContractIsValid()
    {
        const string source = @"
using System.Runtime.Serialization;
using Equatable.Attributes;

namespace Equatable.Entities;

[DataContract]
[DataContractEquatable]
public partial class OrderDataContract
{
    [DataMember(Order = 0)]
    public int Id { get; set; }
}
";
        var diagnostics = await GetAnalyzerDiagnosticsAsync(source,
            new DataContractEquatableAnalyzer());

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task AnalyzeMessagePackEquatableMissingMessagePackObject()
    {
        const string source = @"
using MessagePack;
using Equatable.Attributes;

namespace Equatable.Entities;

[MessagePackEquatable]
public partial class PricingContract
{
    [Key(0)]
    public int MarketId { get; set; }
}
";
        var diagnostics = await GetAnalyzerDiagnosticsAsync(source,
            new MessagePackEquatableAnalyzer());

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0021", diagnostic.Id);
        Assert.Contains("PricingContract", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeMessagePackEquatableWithMessagePackObjectIsValid()
    {
        const string source = @"
using MessagePack;
using Equatable.Attributes;

namespace Equatable.Entities;

[MessagePackObject]
[MessagePackEquatable]
public partial class PricingContract
{
    [Key(0)]
    public int MarketId { get; set; }
}
";
        var diagnostics = await GetAnalyzerDiagnosticsAsync(source,
            new MessagePackEquatableAnalyzer());

        Assert.Empty(diagnostics);
    }

    private static async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(
        string source, params DiagnosticAnalyzer[] additionalAnalyzers)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Concat(
            [
                MetadataReference.CreateFromFile(typeof(EquatableAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Equatable.Attributes.DataContractEquatableAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Equatable.Attributes.MessagePackEquatableAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Runtime.Serialization.DataMemberAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(MessagePack.KeyAttribute).Assembly.Location),
            ]);

        var compilation = CSharpCompilation.Create(
            "Test.Analyzer",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        DiagnosticAnalyzer[] analyzers = [new EquatableAnalyzer(), .. additionalAnalyzers];
        var compilationWithAnalyzers = compilation.WithAnalyzers([.. analyzers]);

        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }
}
