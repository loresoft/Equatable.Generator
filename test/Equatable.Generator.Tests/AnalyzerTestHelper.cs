using System.Collections.Immutable;

using Equatable.Attributes;
using Equatable.SourceGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Equatable.Generator.Tests;

internal static class AnalyzerTestHelper
{
    public static async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(
        string source, params DiagnosticAnalyzer[] additionalAnalyzers)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Concat(
            [
                MetadataReference.CreateFromFile(typeof(EquatableAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Equatable.Attributes.DataContract.DataContractEquatableAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Equatable.Attributes.MessagePack.MessagePackEquatableAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Runtime.Serialization.DataMemberAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(MessagePack.KeyAttribute).Assembly.Location),
            ]);

        var compilation = CSharpCompilation.Create(
            "Test.Analyzer",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        DiagnosticAnalyzer[] analyzers = [new EquatableAnalyzer(), .. additionalAnalyzers];
        var compilationWithAnalyzers = compilation.WithAnalyzers([.. analyzers]);

        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }
}
