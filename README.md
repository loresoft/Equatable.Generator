# Equatable.Generator

Source generator for Equals and GetHashCode

[![Build Project](https://github.com/loresoft/Equatable.Generator/actions/workflows/dotnet.yml/badge.svg)](https://github.com/loresoft/Equatable.Generator/actions/workflows/dotnet.yml)

[![Coverage Status](https://coveralls.io/repos/github/loresoft/Equatable.Generator/badge.svg?branch=main)](https://coveralls.io/github/loresoft/Equatable.Generator?branch=main)

[![Equatable.Generator](https://img.shields.io/nuget/v/Equatable.Generator.svg)](https://www.nuget.org/packages/Equatable.Generator/)

## Features

- Override `Equals` and `GetHashCode`
- Implement `IEquatable<T>`
- Support `class`, `record` and `struct` types
- Support `EqualityComparer` per property
- Comparers supported: String, Sequence, Dictionary, HashSet, Reference, and Custom

### Usage

#### Add package

Add the nuget package to your projects.

`dotnet add package Equatable.Generator`

Prevent including Equatable.Generator as a dependence

```xml
<PackageReference Include="Equatable.Generator" PrivateAssets="all" />
```

### Requirements

This library requires:
- Target framework .NET Standard 2.0 or greater
- Project C# `LangVersion` 9.0 or higher

### Equatable Attributes

Place equatable attribute on a `class`, `record` or `struct`.  The source generate will create a partial with overrides for `Equals` and `GetHashCode`.

- `[Equatable]` Marks the class to generate overrides for `Equals` and `GetHashCod`

 The default comparer used in the implementation of `Equals` and `GetHashCode` is `EqualityComparer<T>.Default`.  Customize the comparer used with the following attributes.

- `[IgnoreEquality]` Ignore property in `Equals` and `GetHashCode` implementations
- `[StringEquality]` Use specified `StringComparer` when comparing strings
- `[SequenceEquality]` Use `Enumerable.SequenceEqual` to determine whether enumerables are equal
- `[DictionaryEquality]` Use to determine if dictionaries are equal
- `[HashSetEquality]` Use `ISet<T>.SetEquals` to determine whether enumerables are equal
- `[ReferenceEquality]` Use `Object.ReferenceEquals` to determines whether instances are the same instance
- `[EqualityComparer]` Use the specified `EqualityComparer`
