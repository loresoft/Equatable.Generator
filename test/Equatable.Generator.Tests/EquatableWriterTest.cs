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
}
