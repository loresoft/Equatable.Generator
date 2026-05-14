using Equatable.SourceGenerator.DataContract;

namespace Equatable.Generator.Tests;

public class DataContractAnalyzerTest
{
    [Fact]
    public async Task AnalyzeDataContractEquatableMissingDataContract()
    {
        const string source = @"
using System.Runtime.Serialization;
using Equatable.Attributes.DataContract;

namespace Equatable.Entities;

[DataContractEquatable]
public partial class OrderDataContract
{
    [DataMember(Order = 0)]
    public int Id { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source,
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
using Equatable.Attributes.DataContract;

namespace Equatable.Entities;

[DataContract]
[DataContractEquatable]
public partial class OrderDataContract
{
    [DataMember(Order = 0)]
    public int Id { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source,
            new DataContractEquatableAnalyzer());

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task AnalyzeDerivedWithoutDataContractFiresEQ0020()
    {
        // The derived class itself lacks [DataContract] — EQ0020 fires on it regardless of the base.
        const string source = @"
using System.Runtime.Serialization;
using Equatable.Attributes.DataContract;

namespace Equatable.Entities;

[DataContractEquatable]
public partial class DerivedOrder : BaseOrder { }

[DataContract]
public abstract class BaseOrder
{
    [DataMember(Order = 0)]
    public int Id { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source,
            new DataContractEquatableAnalyzer());

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0020", diagnostic.Id);
        Assert.Contains("DerivedOrder", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeDerivedWithDataContractIsValid()
    {
        // Both levels have [DataContract] — no diagnostic.
        const string source = @"
using System.Runtime.Serialization;
using Equatable.Attributes.DataContract;

namespace Equatable.Entities;

[DataContract]
[DataContractEquatable]
public partial class DerivedOrder : BaseOrder { }

[DataContract]
public abstract class BaseOrder
{
    [DataMember(Order = 0)]
    public int Id { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source,
            new DataContractEquatableAnalyzer());

        Assert.Empty(diagnostics);
    }

    // ── EQ0022 — unannotated property on DataContractEquatable type ───────────────────────────────

    [Fact]
    public async Task AnalyzeUnannotatedPropertyEmitsEQ0022()
    {
        // LastSeen has no [DataMember] or [IgnoreDataMember] — silently excluded, EQ0022 fires
        const string source = @"
using System;
using System.Runtime.Serialization;
using Equatable.Attributes.DataContract;

namespace Equatable.Entities;

[DataContract]
[DataContractEquatable]
public partial class OrderDataContract
{
    [DataMember(Order = 0)]
    public int Id { get; set; }

    public DateTime LastSeen { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source,
            new DataContractEquatableAnalyzer());

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("EQ0022", diagnostic.Id);
        Assert.Contains("LastSeen", diagnostic.GetMessage());
        Assert.Contains("OrderDataContract", diagnostic.GetMessage());
    }

    [Fact]
    public async Task AnalyzeIgnoreDataMemberSuppressesEQ0022()
    {
        // [IgnoreDataMember] is explicit exclusion — no EQ0022
        const string source = @"
using System;
using System.Runtime.Serialization;
using Equatable.Attributes.DataContract;

namespace Equatable.Entities;

[DataContract]
[DataContractEquatable]
public partial class OrderDataContract
{
    [DataMember(Order = 0)]
    public int Id { get; set; }

    [IgnoreDataMember]
    public DateTime LastSeen { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source,
            new DataContractEquatableAnalyzer());

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task AnalyzeIgnoreEqualitySuppressesEQ0022()
    {
        // [IgnoreEquality] is also an explicit exclusion — no EQ0022
        const string source = @"
using System;
using System.Runtime.Serialization;
using Equatable.Attributes;
using Equatable.Attributes.DataContract;

namespace Equatable.Entities;

[DataContract]
[DataContractEquatable]
public partial class OrderDataContract
{
    [DataMember(Order = 0)]
    public int Id { get; set; }

    [IgnoreEquality]
    public DateTime LastSeen { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source,
            new DataContractEquatableAnalyzer());

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task AnalyzeMultipleUnannotatedPropertiesEmitMultipleEQ0022()
    {
        const string source = @"
using System;
using System.Runtime.Serialization;
using Equatable.Attributes.DataContract;

namespace Equatable.Entities;

[DataContract]
[DataContractEquatable]
public partial class OrderDataContract
{
    [DataMember(Order = 0)]
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }
}
";
        var diagnostics = await AnalyzerTestHelper.GetAnalyzerDiagnosticsAsync(source,
            new DataContractEquatableAnalyzer());

        Assert.Equal(2, diagnostics.Length);
        Assert.Contains(diagnostics, d => d.Id == "EQ0022" && d.GetMessage().Contains("CreatedAt"));
        Assert.Contains(diagnostics, d => d.Id == "EQ0022" && d.GetMessage().Contains("Notes"));
    }
}
