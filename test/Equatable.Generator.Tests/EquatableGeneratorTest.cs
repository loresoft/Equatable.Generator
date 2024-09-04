using System.Collections.Immutable;

using Equatable.Attributes;
using Equatable.SourceGenerator;

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

    public string? DisplayName { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public DateTimeOffset? LastLogin { get; set; }

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

        diagnostics.Should().BeEmpty();

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

        diagnostics.Should().BeEmpty();

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

        diagnostics.Should().BeEmpty();

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

        diagnostics.Should().BeEmpty();

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
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    [property: SequenceEquality] List<string> Versions
);
";

        var (diagnostics, output) = GetGeneratedOutput<EquatableGenerator>(source);

        diagnostics.Should().BeEmpty();

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

        diagnostics.Should().BeEmpty();

        return Verifier
            .Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    private static (ImmutableArray<Diagnostic> Diagnostics, string Output) GetGeneratedOutput<T>(string source)
        where T : IIncrementalGenerator, new()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Concat(
            [
                MetadataReference.CreateFromFile(typeof(T).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(EquatableAttribute).Assembly.Location),
            ]);

        var compilation = CSharpCompilation.Create(
            "Test.Generator",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var originalTreeCount = compilation.SyntaxTrees.Length;
        var generator = new T();

        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var trees = outputCompilation.SyntaxTrees.ToList();

        return (diagnostics, trees.Count != originalTreeCount ? trees[^1].ToString() : string.Empty);
    }

}
