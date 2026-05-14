using Equatable.SourceGenerator.MessagePack;

namespace Equatable.Generator.Tests;

public class MessagePackAnalyzerTest
{
    [Fact]
    public async Task AnalyzeMessagePackEquatableMissingMessagePackObject()
    {
        const string source = @"
using MessagePack;
using Equatable.Attributes.MessagePack;

namespace Equatable.Entities;

[MessagePackEquatable]
public partial class PricingContract
{
    [Key(0)]
    public int MarketId { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source,
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
using Equatable.Attributes.MessagePack;

namespace Equatable.Entities;

[MessagePackObject]
[MessagePackEquatable]
public partial class PricingContract
{
    [Key(0)]
    public int MarketId { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source,
            new MessagePackEquatableAnalyzer());

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task AnalyzeDerivedWithoutMessagePackObjectFiresEQ0021()
    {
        // The derived class itself lacks [MessagePackObject] — EQ0021 fires on it.
        const string source = @"
using MessagePack;
using Equatable.Attributes.MessagePack;

namespace Equatable.Entities;

[MessagePackEquatable]
public partial class DerivedContract : BaseContract { }

[MessagePackObject]
public abstract class BaseContract
{
    [Key(0)]
    public int Id { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source,
            new MessagePackEquatableAnalyzer());

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0021", diagnostic.Id);
        Assert.Contains("DerivedContract", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeDerivedWithMessagePackObjectIsValid()
    {
        // Both levels have [MessagePackObject] — no diagnostic.
        const string source = @"
using MessagePack;
using Equatable.Attributes.MessagePack;

namespace Equatable.Entities;

[MessagePackObject]
[MessagePackEquatable]
public partial class DerivedContract : BaseContract { }

[MessagePackObject]
public abstract class BaseContract
{
    [Key(0)]
    public int Id { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source,
            new MessagePackEquatableAnalyzer());

        Assert.Empty(diagnostics);
    }

    // ── EQ0023 — unannotated property on MessagePackEquatable type ────────────────────────────────

    [Fact]
    public async Task AnalyzeUnannotatedPropertyEmitsEQ0023()
    {
        // ReceivedAt has no [Key] or [IgnoreMember] — silently excluded, EQ0023 fires
        const string source = @"
using System;
using MessagePack;
using Equatable.Attributes.MessagePack;

namespace Equatable.Entities;

[MessagePackObject]
[MessagePackEquatable]
public partial class PricingContract
{
    [Key(0)]
    public int MarketId { get; set; }

    public DateTime ReceivedAt { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source,
            new MessagePackEquatableAnalyzer());

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0023", diagnostic.Id);
        Assert.Contains("ReceivedAt", diagnostic.GetMessage());
        Assert.Contains("PricingContract", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeIgnoreMemberSuppressesEQ0023()
    {
        // [IgnoreMember] is explicit exclusion — no EQ0023
        const string source = @"
using System;
using MessagePack;
using Equatable.Attributes.MessagePack;

namespace Equatable.Entities;

[MessagePackObject]
[MessagePackEquatable]
public partial class PricingContract
{
    [Key(0)]
    public int MarketId { get; set; }

    [IgnoreMember]
    public DateTime ReceivedAt { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source,
            new MessagePackEquatableAnalyzer());

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task AnalyzeIgnoreEqualitySuppressesEQ0023()
    {
        // [IgnoreEquality] is also an explicit exclusion — no EQ0023
        const string source = @"
using System;
using MessagePack;
using Equatable.Attributes;
using Equatable.Attributes.MessagePack;

namespace Equatable.Entities;

[MessagePackObject]
[MessagePackEquatable]
public partial class PricingContract
{
    [Key(0)]
    public int MarketId { get; set; }

    [IgnoreEquality]
    public DateTime ReceivedAt { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source,
            new MessagePackEquatableAnalyzer());

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task AnalyzeMultipleUnannotatedPropertiesEmitMultipleEQ0023()
    {
        const string source = @"
using System;
using MessagePack;
using Equatable.Attributes.MessagePack;

namespace Equatable.Entities;

[MessagePackObject]
[MessagePackEquatable]
public partial class PricingContract
{
    [Key(0)]
    public int MarketId { get; set; }

    public DateTime ReceivedAt { get; set; }
    public string? Notes { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source,
            new MessagePackEquatableAnalyzer());

        Assert.Equal(2, diagnostics.Length);
        Assert.Contains(diagnostics, d => d.Id == "EQ0023" && d.GetMessage().Contains("ReceivedAt"));
        Assert.Contains(diagnostics, d => d.Id == "EQ0023" && d.GetMessage().Contains("Notes"));
    }
}
