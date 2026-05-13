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
}
