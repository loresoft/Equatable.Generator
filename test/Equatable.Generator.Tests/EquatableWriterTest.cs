using Equatable.SourceGenerator;
using Equatable.SourceGenerator.Models;

namespace Equatable.Generator.Tests;

public class EquatableWriterTest
{
    [Fact]
    public async Task GenerateBasicUser()
    {
        var entityClass = new EquatableClass(
            FullyQualified: "global::Equatable.Entities.User",
            EntityNamespace: "Equatable.Entities",
            EntityName: "User",
            FileName: "Equatable.Entities.User.Equatable.g.cs",
            ContainingTypes: Array.Empty<ContainingClass>(),
            Properties: new EquatableArray<EquatableProperty>([
                new EquatableProperty("Id", "int"),
                new EquatableProperty("FirstName", "string?"),
                new EquatableProperty("LastName", "string?"),
                new EquatableProperty("EmailAddress", "string"),
                new EquatableProperty("Created", "System.DateTimeOffset")
            ]),
            IsRecord: false,
            IsValueType: false,
            IsSealed: false,
            IncludeBaseEqualsMethod: false,
            IncludeBaseHashMethod: false,
            SeedHash: -1758092530
        );

        var output = EquatableWriter.Generate(entityClass);

        await
            Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public async Task GenerateUserStringSequence()
    {
        var entityClass = new EquatableClass(
            FullyQualified: "global::Equatable.Entities.User",
            EntityNamespace: "Equatable.Entities",
            EntityName: "User",
            FileName: "Equatable.Entities.User.Equatable.g.cs",
            ContainingTypes: Array.Empty<ContainingClass>(),
            Properties: new EquatableArray<EquatableProperty>([
                new EquatableProperty("Id", "int"),
                new EquatableProperty("FirstName", "string?"),
                new EquatableProperty("LastName", "string?"),
                new EquatableProperty("EmailAddress", "string", ComparerTypes.String, "OrdinalIgnoreCase"),
                new EquatableProperty("Created", "System.DateTimeOffset"),
                new EquatableProperty("Roles", "ICollection<Role>", ComparerTypes.Sequence),
            ]),
                        IsRecord: false,
            IsValueType: false,
            IsSealed: false,
            IncludeBaseEqualsMethod: false,
            IncludeBaseHashMethod: false,
            SeedHash: -1758092530
        );

        var output = EquatableWriter.Generate(entityClass);

        await
            Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public async Task GenerateUserImportHashSetDictionary()
    {
        var entityClass = new EquatableClass(
            FullyQualified: "global::Equatable.Entities.UserImport",
            EntityNamespace: "Equatable.Entities",
            EntityName: "UserImport",
            FileName: "Equatable.Entities.UserImport.Equatable.g.cs",
            ContainingTypes: Array.Empty<ContainingClass>(),
            Properties: new EquatableArray<EquatableProperty>([
                new EquatableProperty("EmailAddress", "string", ComparerTypes.String, "OrdinalIgnoreCase"),
                new EquatableProperty("DisplayName", "string?"),
                new EquatableProperty("FirstName", "string?"),
                new EquatableProperty("LastName", "string?"),
                new EquatableProperty("LockoutEnd", "System.DateTimeOffset?"),
                new EquatableProperty("LastLogin", "System.DateTimeOffset?"),
                new EquatableProperty("Roles", "System.Collections.Generic.HashSet<string>?", ComparerTypes.HashSet),
                new EquatableProperty("Permissions", "System.Collections.Generic.Dictionary<string, int>?", ComparerTypes.Dictionary),
            ]),
                        IsRecord: false,
            IsValueType: false,
            IsSealed: false,
            IncludeBaseEqualsMethod: false,
            IncludeBaseHashMethod: false,
            SeedHash: -1758092530
        );

        var output = EquatableWriter.Generate(entityClass);

        await
            Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    // ── record type — generates EqualityContract check and virtual Equals ─────────────────────────

    [Fact]
    public async Task GenerateRecord_EmitsVirtualEquals()
    {
        var entityClass = new EquatableClass(
            FullyQualified: "global::Equatable.Entities.PricingRecord",
            EntityNamespace: "Equatable.Entities",
            EntityName: "PricingRecord",
            FileName: "Equatable.Entities.PricingRecord.Equatable.g.cs",
            ContainingTypes: Array.Empty<ContainingClass>(),
            Properties: new EquatableArray<EquatableProperty>([
                new EquatableProperty("MarketId", "int"),
                new EquatableProperty("Probability", "double"),
            ]),
            IsRecord: true,
            IsValueType: false,
            IsSealed: false,
            IncludeBaseEqualsMethod: false,
            IncludeBaseHashMethod: false,
            SeedHash: 42
        );

        var output = EquatableWriter.Generate(entityClass);

        await Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    // ── sealed class — Equals is not virtual, no object-typed override needed ────────────────────

    [Fact]
    public async Task GenerateSealed_NonVirtualEquals()
    {
        var entityClass = new EquatableClass(
            FullyQualified: "global::Equatable.Entities.FinalEntity",
            EntityNamespace: "Equatable.Entities",
            EntityName: "FinalEntity",
            FileName: "Equatable.Entities.FinalEntity.Equatable.g.cs",
            ContainingTypes: Array.Empty<ContainingClass>(),
            Properties: new EquatableArray<EquatableProperty>([
                new EquatableProperty("Id", "int"),
            ]),
            IsRecord: false,
            IsValueType: false,
            IsSealed: true,
            IncludeBaseEqualsMethod: false,
            IncludeBaseHashMethod: false,
            SeedHash: 0
        );

        var output = EquatableWriter.Generate(entityClass);

        await Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    // ── derived class — generated Equals and GetHashCode include base delegation ─────────────────

    [Fact]
    public async Task GenerateDerived_DelegatesBaseEqualsAndHashCode()
    {
        var entityClass = new EquatableClass(
            FullyQualified: "global::Equatable.Entities.DerivedEntity",
            EntityNamespace: "Equatable.Entities",
            EntityName: "DerivedEntity",
            FileName: "Equatable.Entities.DerivedEntity.Equatable.g.cs",
            ContainingTypes: Array.Empty<ContainingClass>(),
            Properties: new EquatableArray<EquatableProperty>([
                new EquatableProperty("Label", "string?"),
            ]),
            IsRecord: false,
            IsValueType: false,
            IsSealed: false,
            IncludeBaseEqualsMethod: true,
            IncludeBaseHashMethod: true,
            SeedHash: 99
        );

        var output = EquatableWriter.Generate(entityClass);

        await Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }

    // ── nested class — generated file is wrapped in containing-type partial declarations ─────────

    [Fact]
    public async Task GenerateNested_WrapsInContainingType()
    {
        var entityClass = new EquatableClass(
            FullyQualified: "global::Equatable.Entities.Outer.Inner",
            EntityNamespace: "Equatable.Entities",
            EntityName: "Inner",
            FileName: "Equatable.Entities.Outer.Inner.Equatable.g.cs",
            ContainingTypes: new EquatableArray<ContainingClass>([
                new ContainingClass("Outer", IsRecord: false, IsValueType: false),
            ]),
            Properties: new EquatableArray<EquatableProperty>([
                new EquatableProperty("Value", "int"),
            ]),
            IsRecord: false,
            IsValueType: false,
            IsSealed: false,
            IncludeBaseEqualsMethod: false,
            IncludeBaseHashMethod: false,
            SeedHash: 7
        );

        var output = EquatableWriter.Generate(entityClass);

        await Verify(output).UseDirectory("Snapshots").ScrubLinesContaining("GeneratedCodeAttribute");
    }
}
