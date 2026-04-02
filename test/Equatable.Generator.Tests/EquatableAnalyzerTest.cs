using System.Collections.Immutable;

using Equatable.Attributes;
using Equatable.SourceGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Equatable.Generator.Tests;

public class EquatableAnalyzerTest
{
    [Fact]
    public async Task AnalyzeValidUsage()
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
        var source = @"
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
        var source = @"
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
        var source = @"
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
        var source = @"
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
        var source = @"
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
        var source = @"
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
        var source = @"
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
        var source = @"
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
        var source = @"
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
        var source = @"
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
        var source = @"
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
        var source = @"
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
    public async Task AnalyzeDictionaryWithMissingAttributeEmitsDictDiagnostic()
    {
        // Dictionary implements both IDictionary and IEnumerable, should emit EQ0001 (not EQ0002)
        var source = @"
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
        var source = @"
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

    private static async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Concat(
            [
                MetadataReference.CreateFromFile(typeof(EquatableAttribute).Assembly.Location),
            ]);

        var compilation = CSharpCompilation.Create(
            "Test.Analyzer",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var analyzer = new EquatableAnalyzer();
        var compilationWithAnalyzers = compilation.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));

        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }
}
