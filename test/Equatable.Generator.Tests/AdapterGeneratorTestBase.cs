using System.Collections.Immutable;

using Equatable.Attributes;
using Equatable.SourceGenerator;
using Equatable.SourceGenerator.DataContract;
using Equatable.SourceGenerator.MessagePack;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Equatable.Generator.Tests;

public abstract class AdapterGeneratorTestBase
{
    private static readonly IEnumerable<MetadataReference> PinnedReferences =
    [
        MetadataReference.CreateFromFile(typeof(System.Runtime.Serialization.DataMemberAttribute).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(MessagePack.KeyAttribute).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Equatable.Attributes.DataContract.DataContractEquatableAttribute).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Equatable.Attributes.MessagePack.MessagePackEquatableAttribute).Assembly.Location),
    ];

    protected static IEnumerable<MetadataReference> BuildReferences<T>()
        where T : IIncrementalGenerator, new()
        => AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Concat(
            [
                MetadataReference.CreateFromFile(typeof(T).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(EquatableAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DataContractEquatableGenerator).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(MessagePackEquatableGenerator).Assembly.Location),
            ])
            .Concat(PinnedReferences);

    protected static (ImmutableArray<Diagnostic> Diagnostics, string Output) GetGeneratedOutput<T>(string source)
        where T : IIncrementalGenerator, new()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create(
            "Test.Generator",
            [syntaxTree],
            BuildReferences<T>(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var originalTreeCount = compilation.SyntaxTrees.Length;
        var driver = CSharpGeneratorDriver.Create(new T());
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var trees = outputCompilation.SyntaxTrees.ToList();
        return (diagnostics, trees.Count != originalTreeCount ? trees[^1].ToString() : string.Empty);
    }

    protected static (ImmutableArray<Diagnostic> Diagnostics, string Output) GetNamedGeneratedOutput<T>(string source, string typeName)
        where T : IIncrementalGenerator, new()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create(
            "Test.Generator",
            [syntaxTree],
            BuildReferences<T>(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var originalTreeCount = compilation.SyntaxTrees.Length;
        var driver = CSharpGeneratorDriver.Create(new T());
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var generated = outputCompilation.SyntaxTrees.Skip(originalTreeCount).ToList();
        var match = generated.FirstOrDefault(t => t.ToString().Contains($"partial class {typeName}"))
            ?? (generated.Count > 0 ? generated[^1] : null);

        return (diagnostics, match?.ToString() ?? string.Empty);
    }
}
