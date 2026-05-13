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
}
