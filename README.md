# Equatable.Generator

Source generator for `Equals` and `GetHashCode` with attribute-based control of equality implementation.

[![Build Project](https://github.com/loresoft/Equatable.Generator/actions/workflows/dotnet.yml/badge.svg)](https://github.com/loresoft/Equatable.Generator/actions/workflows/dotnet.yml)

[![Coverage Status](https://coveralls.io/repos/github/loresoft/Equatable.Generator/badge.svg?branch=main)](https://coveralls.io/github/loresoft/Equatable.Generator?branch=main)

[![Equatable.Generator](https://img.shields.io/nuget/v/Equatable.Generator.svg)](https://www.nuget.org/packages/Equatable.Generator/)

## What it does

In C# every class inherits `Equals` from `object`, which compares **references** (memory addresses), not values:

```csharp
var a = new Product { Id = 1, Name = "Widget" };
var b = new Product { Id = 1, Name = "Widget" };
Console.WriteLine(a == b);  // false — different objects, even though values are identical
```

This library generates correct `Equals` + `GetHashCode` at compile-time — zero runtime overhead, zero boilerplate.

## Packages

| Package | What it does |
|---|---|
| `Equatable.Generator` | Generates equality for `[Equatable]` classes/records/structs. Includes all collection attributes. |
| `Equatable.Generator.DataContract` | Adapter — reads `[DataMember]` attributes (WCF / protobuf-net contracts) |
| `Equatable.Generator.MessagePack` | Adapter — reads `[Key(n)]` attributes (MessagePack serialisation) |
| `Equatable.Comparers` | Ships the runtime comparers used by the generated code |

## Getting started

```xml
<!-- Add to your .csproj — PrivateAssets means it doesn't become a runtime dependency -->
<PackageReference Include="Equatable.Generator" PrivateAssets="all" />
```

Mark your class as `partial` and add `[Equatable]`:

```csharp
[Equatable]
public partial class Product
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
}
```

The generator writes `Equals` and `GetHashCode` for every public property. Works on `class`, `record`, and `readonly struct`.

## All attributes at a glance

| Attribute | What it generates | Default for |
|---|---|---|
| `[Equatable]` | Triggers generation; includes all public properties | — |
| `[IgnoreEquality]` | Skip this property | — |
| `[StringEquality(StringComparison.X)]` | `StringComparer.X.Equals(a, b)` | — |
| `[EqualityComparer(typeof(T))]` | `T.Default.Equals(a, b)` — any custom comparer | — |
| `[SequenceEquality]` | `SequenceEqualityComparer` — element order matters | `List<T>`, `T[]` |
| `[HashSetEquality]` | `HashSetEqualityComparer` — element order ignored | `HashSet<T>` |
| `[DictionaryEquality]` | `ReadOnlyDictionaryEqualityComparer` — key-value equality | `Dictionary<K,V>` |
| `[DictionaryEquality(sequential:true)]` | `OrderedReadOnlyDictionaryEqualityComparer` — key-sorted | — |
| `[ReferenceEquality]` | `Object.ReferenceEquals(a, b)` | — |

## Adapter generators

Use `[DataContractEquatable]` or `[MessagePackEquatable]` when your class is already annotated for serialisation. They work exactly like `[Equatable]` but only include the properties the serialiser knows about:

```csharp
// Only [DataMember] properties are included in equality.
// [IgnoreDataMember] and un-annotated properties are skipped.
[DataContract]
[DataContractEquatable]
public partial class EventContract
{
    [DataMember(Order = 0)] public int EventId { get; set; }

    [DataMember(Order = 1)]
    [SequenceEquality]
    public string[]? Tags { get; set; }

    [IgnoreDataMember]
    public DateTime LastSeen { get; set; }  // excluded from equality
}
```

```csharp
// Only [Key(n)] properties are included.
// [IgnoreMember] properties are skipped.
[MessagePackObject]
[MessagePackEquatable]
public partial class LiveScore
{
    [Key(0)] public int MatchId { get; set; }
    [Key(1)] public int HomeScore { get; set; }

    [IgnoreMember]
    public DateTime ReceivedAt { get; set; }  // excluded
}
```

## Collection attributes in detail

### `[SequenceEquality]` — order matters

```csharp
[SequenceEquality]
public List<string>? Tracks { get; set; }
```

`["A","B","C"]` equals `["A","B","C"]` ✓  
`["A","B","C"]` does NOT equal `["C","B","A"]` ✓

Also works on `int[]`, `IEnumerable<T>`, and any sequence type.

**Direction override:** apply to `HashSet<T>` to force order-sensitive comparison on a normally unordered set.

---

### `[HashSetEquality]` — order does not matter

```csharp
[HashSetEquality]
public HashSet<string>? Roles { get; set; }
```

`{"admin","editor"}` equals `{"editor","admin"}` ✓

**Direction override:** apply to `List<T>` or `T[]` to make them order-insensitive.

---

### `[DictionaryEquality]` — insertion order does not matter

```csharp
[DictionaryEquality]
public Dictionary<string, double>? OddsBySource { get; set; }
```

`{betgenius:1.85, abelson:1.90}` equals `{abelson:1.90, betgenius:1.85}` ✓

### `[DictionaryEquality(sequential: true)]` — key-sorted comparison

Both sides are sorted by key before comparison. Insertion order is still irrelevant, but the result is deterministic — useful for snapshots and logs.

```csharp
[DictionaryEquality(sequential: true)]
public Dictionary<string, int>? RankByRegion { get; set; }
```

---

## Comparer propagation into nested collections

Annotate the **outer property once** — the chosen comparer kind propagates automatically into all nested levels.

```csharp
// outer dict → key-sorted
// inner dict → key-sorted (propagated automatically)
[DictionaryEquality(sequential: true)]
public Dictionary<string, Dictionary<string, int>>? ScoresByRegionAndTeam { get; set; }

// outer dict → key-sorted
// inner list → order-sensitive (inferred from List<T>)
[DictionaryEquality(sequential: true)]
public Dictionary<string, List<int>>? HistoryByRegion { get; set; }

// three levels deep — propagation goes all the way
[DictionaryEquality(sequential: true)]
public Dictionary<string, Dictionary<string, Dictionary<string, int>>>? ThreeLevelConfig { get; set; }
```

You never need to annotate nested properties separately.

---

## `[EqualityComparer]` — fully custom comparer

When no built-in attribute fits, write your own `IEqualityComparer<T>`:

```csharp
public sealed class CountOnlyComparer : IEqualityComparer<Dictionary<string, int>?>
{
    public static readonly CountOnlyComparer Default = new();
    public bool Equals(Dictionary<string, int>? x, Dictionary<string, int>? y) =>
        x is null ? y is null : y is not null && x.Count == y.Count;
    public int GetHashCode(Dictionary<string, int>? obj) => obj?.Count ?? 0;
}

[EqualityComparer(typeof(CountOnlyComparer))]
public Dictionary<string, int>? AssetWeights { get; set; }
```

---

## Equality invariants

Every generated implementation satisfies:

| Property | Meaning |
|---|---|
| **Reflexive** | `a.Equals(a)` is always `true` |
| **Symmetric** | `a.Equals(b) == b.Equals(a)` always |
| **Null-safe** | `a.Equals(null)` is always `false` |
| **Hash contract** | `a.Equals(b)` implies `a.GetHashCode() == b.GetHashCode()` |

The hash contract is critical for using objects as dictionary keys or in hash sets.

---

## Requirements

- Target framework .NET Standard 2.0 or greater
- C# `LangVersion` 8.0 or higher
