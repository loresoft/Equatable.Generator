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

        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

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

        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

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

        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

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

        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

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

        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

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

        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

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

        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

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

        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

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

        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

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

        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

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

        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

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

        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

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

        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

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

        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

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

        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

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

        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

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

        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

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

        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

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

        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

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

        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

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

        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

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

        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

        Assert.Empty(diagnostics);
    }

    // ── ISet / IReadOnlySet / array diagnostics ───────────────────────────────────────────────────

    [Fact]
    public async Task AnalyzeMissingAttributeForISet()
    {
        // ISet<T> implements IEnumerable<T> → EQ0002 fires when no attribute is present
        const string source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    public ISet<int>? Tags { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0002", diagnostic.Id);
        Assert.Contains("Tags", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeMissingAttributeForIReadOnlySet()
    {
        // IReadOnlySet<T> implements IEnumerable<T> → EQ0002 fires when no attribute is present
        const string source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    public IReadOnlySet<int>? Roles { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0002", diagnostic.Id);
        Assert.Contains("Roles", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeMissingAttributeForArray()
    {
        // int[] without [SequenceEquality] → EQ0002
        const string source = @"
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    public int[]? Codes { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0002", diagnostic.Id);
        Assert.Contains("Codes", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeMultiDimensionalArrayNoAttributeNoWarning()
    {
        // int[,] without any attribute → no diagnostic (MultiDimensionalArrayEqualityComparer is the default)
        const string source = @"
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Grid
{
    public int[,]? Cells { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task AnalyzeHashSetEqualityOnISetIsValid()
    {
        // [HashSetEquality] on ISet<T> must NOT emit EQ0012
        const string source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    [HashSetEquality]
    public ISet<int>? Tags { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task AnalyzeHashSetEqualityOnIReadOnlySetIsValid()
    {
        // [HashSetEquality] on IReadOnlySet<T> must NOT emit EQ0012
        const string source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    [HashSetEquality]
    public IReadOnlySet<int>? Roles { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task AnalyzeSequenceEqualityOnArrayIsValid()
    {
        // [SequenceEquality] on int[] must NOT emit EQ0013
        const string source = @"
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class UserImport
{
    [SequenceEquality]
    public int[]? Codes { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

        Assert.Empty(diagnostics);
    }

    // ── EQ0014 — invalid attribute on multi-dimensional array ─────────────────────────────────────

    [Fact]
    public async Task AnalyzeSequenceEqualityOnMultiDimArrayEmitsEQ0014()
    {
        // [SequenceEquality] on int[,] → EQ0014 (MultiDimensionalArrayEqualityComparer is always used)
        const string source = @"
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Grid
{
    [SequenceEquality]
    public int[,]? Cells { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0014", diagnostic.Id);
        Assert.Contains("Cells", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeHashSetEqualityOnMultiDimArrayEmitsEQ0014()
    {
        // [HashSetEquality] on int[,] → EQ0014 only
        // (ImplementsEnumerable returns true for all arrays, so EQ0012 does not fire;
        //  the runtime validator in the generator would reject Rank > 1, but the analyzer
        //  only emits EQ0014 to keep diagnostics non-overlapping)
        const string source = @"
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Grid
{
    [HashSetEquality]
    public int[,]? Cells { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0014", diagnostic.Id);
        Assert.Contains("Cells", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeEqualityComparerOnMultiDimArrayEmitsEQ0014()
    {
        // [EqualityComparer] on int[,] → EQ0014 (bypasses MultiDimensionalArrayEqualityComparer entirely)
        const string source = @"
using System.Collections.Generic;
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Grid
{
    [EqualityComparer(typeof(MyComparer))]
    public int[,]? Cells { get; set; }
}

public class MyComparer : IEqualityComparer<int[,]?>
{
    public static readonly MyComparer Default = new();
    public bool Equals(int[,]? x, int[,]? y) => ReferenceEquals(x, y);
    public int GetHashCode(int[,]? obj) => obj?.GetHashCode() ?? 0;
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0014", diagnostic.Id);
        Assert.Contains("Cells", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeReferenceEqualityOnMultiDimArrayEmitsEQ0014()
    {
        // [ReferenceEquality] on int[,] → EQ0014
        const string source = @"
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Grid
{
    [ReferenceEquality]
    public int[,]? Cells { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0014", diagnostic.Id);
        Assert.Contains("Cells", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeMultiDimArrayThreeDimensionsEmitsEQ0014()
    {
        // int[,,] with [SequenceEquality] → EQ0014 (rank 3 also triggers)
        const string source = @"
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Cube
{
    [SequenceEquality]
    public double[,,]? Data { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0014", diagnostic.Id);
        Assert.Contains("Data", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeMultiDimArrayNoAttributeNoWarning()
    {
        // int[,,] without any attribute → no diagnostic (default comparer handles it)
        const string source = @"
using Equatable.Attributes;

namespace Equatable.Entities;

[Equatable]
public partial class Cube
{
    public double[,,]? Data { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source);

        Assert.Empty(diagnostics);
    }
}
