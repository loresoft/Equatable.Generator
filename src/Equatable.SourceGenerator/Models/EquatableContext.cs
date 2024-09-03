using Microsoft.CodeAnalysis;

namespace Equatable.SourceGenerator.Models;

public record EquatableContext(
        EquatableClass? EntityClass,
        EquatableArray<Diagnostic>? Diagnostics);
