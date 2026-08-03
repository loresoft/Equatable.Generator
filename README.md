# Equatable.Generator

A C# source generator that writes `Equals`, `GetHashCode`, and equality operators for you, with attribute based control over how each member is compared.

[![Build Project](https://github.com/loresoft/Equatable.Generator/actions/workflows/dotnet.yml/badge.svg)](https://github.com/loresoft/Equatable.Generator/actions/workflows/dotnet.yml)
[![Coverage Status](https://coveralls.io/repos/github/loresoft/Equatable.Generator/badge.svg?branch=main)](https://coveralls.io/github/loresoft/Equatable.Generator?branch=main)
[![NuGet Version](https://img.shields.io/nuget/v/Equatable.Generator.svg)](https://www.nuget.org/packages/Equatable.Generator/)

## Features

- Generates `Equals(object)`, `Equals(T)`, and `GetHashCode()` overrides
- Implements `IEquatable<T>` and the `==` / `!=` operators
- Supports `class`, `record`, and `struct` types
- Per member comparer selection through attributes
- Built-in comparers: string, sequence, dictionary, set, reference, and custom
- Works on properties and fields
- No runtime dependencies; the package is compile time only

## Installation

```shell
dotnet add package Equatable.Generator
```

The generator emits its own attributes, so nothing needs to flow to consumers of your library. Mark the reference as private to keep it out of your package dependencies:

```xml
<PackageReference Include="Equatable.Generator" PrivateAssets="all" />
```

## Requirements

- Roslyn 4.14 or later. In practice: Visual Studio 2022 17.14+, Visual Studio 2026+, a current Rider release, or the .NET SDK 9.0.300 or newer.
- Project C# `LangVersion` 8.0 or higher

## Quick start

Mark a **partial** type with `[Equatable]`. The generator creates the matching partial with equality members for all public properties and fields.

```c#
using Equatable.Attributes;

[Equatable]
public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
}
```

```c#
var first = new Product { Id = 1, Name = "Widget" };
var second = new Product { Id = 1, Name = "Widget" };

Console.WriteLine(first == second);              // True
Console.WriteLine(first.Equals(second));         // True
Console.WriteLine(first is IEquatable<Product>); // True
```

For `class` and `struct` types the generator emits `IEquatable<T>`, both `Equals` methods, `GetHashCode`, and the `==` / `!=` operators. For `record` types it emits the strongly typed `Equals` and `GetHashCode` only, because the compiler supplies the rest.

## Default comparison rules

| Member type     | Default behavior                                        |
| --------------- | ------------------------------------------------------- |
| Value types     | The `==` operator, avoiding boxing and comparer lookups |
| `string` types  | `StringComparer.Ordinal` (same as `string.Equals`)      |
| Everything else | `EqualityComparer<T>.Default`                           |

Use the attributes below to change the behavior of an individual member.

## Attributes

All attributes live in the `Equatable.Attributes` namespace.

| Attribute                            | Applies to            | Behavior                                                                                |
| ------------------------------------ | --------------------- | --------------------------------------------------------------------------------------- |
| `[Equatable]`                        | class, record, struct | Generates the equality members for the type                                             |
| `[IgnoreEquality]`                   | property, field       | Excludes the member from `Equals` and `GetHashCode`                                     |
| `[StringEquality(StringComparison)]` | property, field       | Uses the `StringComparer` matching the supplied `StringComparison`                      |
| `[SequenceEquality]`                 | property, field       | Compares elements in order with `Enumerable.SequenceEqual`; order affects the hash code |
| `[DictionaryEquality]`               | property, field       | Compares entry counts and per key values; order independent                             |
| `[HashSetEquality]`                  | property, field       | Compares contents with `ISet<T>.SetEquals`; order independent                           |
| `[ReferenceEquality]`                | property, field       | Compares with `Object.ReferenceEquals` and hashes with `RuntimeHelpers.GetHashCode`     |
| `[EqualityComparer(Type, Property)]` | property, field       | Uses the comparer returned by the named static property on the given type               |

### Custom comparer example

```c#
[Equatable]
public partial class Document
{
    [EqualityComparer(typeof(TrimmedComparer), nameof(TrimmedComparer.Instance))]
    public string? Title { get; set; }
}

public sealed class TrimmedComparer : IEqualityComparer<string?>
{
    public static TrimmedComparer Instance { get; } = new();

    public bool Equals(string? x, string? y)
        => string.Equals(x?.Trim(), y?.Trim(), StringComparison.Ordinal);

    public int GetHashCode(string? obj)
        => obj?.Trim().GetHashCode() ?? 0;
}
```

## Full example

```c#
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
    public string FullName => $"{FirstName} {LastName}";

    [HashSetEquality]
    public HashSet<string>? Roles { get; set; }

    [DictionaryEquality]
    public Dictionary<string, int>? Permissions { get; set; }

    [SequenceEquality]
    public List<DateTimeOffset>? History { get; set; }
}
```

Records, including positional records, are supported. Use the `property:` target so the attribute lands on the generated property:

```c#
[Equatable]
public partial record StatusRecord(
    int Id,
    [property: StringEquality(StringComparison.OrdinalIgnoreCase)] string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    [property: SequenceEquality] List<string> Versions
);
```

Structs work the same way:

```c#
[Equatable]
public partial struct Coordinate
{
    public double Latitude { get; set; }

    public double Longitude { get; set; }
}
```

## Notes and limitations

- The type must be declared `partial`.
- Nested types are supported as long as every containing type is also `partial`.
- The attributes are conditional on the `EQUATABLE_GENERATOR` symbol, so they leave no trace in the compiled output.

## Contributing

Issues and pull requests are welcome on [GitHub](https://github.com/loresoft/Equatable.Generator).
