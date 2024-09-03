using Equatable.SourceGenerator.Models;

namespace Equatable.SourceGenerator.Tests;

public class EquatableWriterTest
{
    [Fact]
    public async Task GenerateBasicUser()
    {
        var entityClass = new EquatableClass(
            "Equatable.Generator.Entities",
            "User",
            new EquatableArray<EquatableProperty>([
                new EquatableProperty("Id", "System.Guid"),
                new EquatableProperty("FirstName", "string"),
                new EquatableProperty("LastName", "string"),
                new EquatableProperty("EmailAddress", "string", ComparerTypes.String, "OrdinalIgnoreCase"),
                new EquatableProperty("Created", "System.DateTimeOffset"),
                new EquatableProperty("Roles", "ICollection<Role>", ComparerTypes.Sequence),
            ])
        );

        var output = EquatableWriter.Generate(entityClass);

        await Verifier
            .Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

}
