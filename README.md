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

    // string[] defaults to [SequenceEquality] — no attribute needed.
    [DataMember(Order = 1)] public string[]? Tags { get; set; }

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

**Default for:** `List<T>`, `T[]` — no attribute needed on these types.

**Supported types:** any `IEnumerable<T>` — `List<T>`, `T[]`, `ICollection<T>`, `IReadOnlyList<T>`, `IEnumerable<T>`, `HashSet<T>` (via override), and more.

```csharp
public List<string>? Tracks { get; set; }   // SequenceEquality by default
public int[]? Scores { get; set; }           // SequenceEquality by default
```

`["A","B","C"]` equals `["A","B","C"]` ✓  
`["A","B","C"]` does NOT equal `["C","B","A"]` ✓

**Direction override:** apply to `HashSet<T>` to force order-sensitive comparison on a normally unordered set.

```csharp
[SequenceEquality]
public HashSet<string>? OrderedTags { get; set; }  // override: order now matters
```

---

### `[HashSetEquality]` — order does not matter

**Default for:** `HashSet<T>` — no attribute needed on plain hash sets.

**Supported types:** any `IEnumerable<T>` — `HashSet<T>`, `ISet<T>`, `IReadOnlySet<T>`, `List<T>`, `T[]` (via override), and more. When the value implements `ISet<T>`, `SetEquals` is called directly (fast path). Otherwise a temporary `HashSet<T>` is constructed for the comparison.

```csharp
public HashSet<string>? Roles { get; set; }  // HashSetEquality by default
```

`{"admin","editor"}` equals `{"editor","admin"}` ✓

**Direction override:** apply to `List<T>` or `T[]` to make them order-insensitive.

```csharp
[HashSetEquality]
public List<string>? PermissionCodes { get; set; }  // override: order no longer matters
```

---

### `[DictionaryEquality]` — insertion order does not matter

**Default for:** `Dictionary<K,V>` — no attribute needed on plain dictionaries.

**Supported types:** any `IReadOnlyDictionary<K,V>` — `Dictionary<K,V>`, `IReadOnlyDictionary<K,V>`, `SortedDictionary<K,V>`, `ConcurrentDictionary<K,V>`, and more.

```csharp
public Dictionary<string, double>? Prices { get; set; }  // DictionaryEquality by default
```

`{a:1.85, b:1.90}` equals `{b:1.90, a:1.85}` ✓

### `[DictionaryEquality(sequential: true)]` — key-sorted comparison

Both sides are sorted by key before comparison. Insertion order is still irrelevant, but the result is deterministic — useful for snapshots and logs.

**Supported types:** same as `[DictionaryEquality]` — any `IReadOnlyDictionary<K,V>`.

```csharp
[DictionaryEquality(sequential: true)]
public Dictionary<string, int>? RankByRegion { get; set; }
```

---

## Nested collections

Every collection attribute works on nested collection types without any extra annotation. The outer attribute propagates its intent inward; inner types that have their own default use it.

### Propagation rules

| Outer annotation | Inner `Dictionary<K,V>` | Inner `List<T>` / `T[]` | Inner `HashSet<T>` |
|---|---|---|---|
| `[DictionaryEquality]` | `DictionaryEquality` | `SequenceEquality` | `HashSetEquality` |
| `[DictionaryEquality(sequential:true)]` | `DictionaryEquality(sequential:true)` | `SequenceEquality` | `HashSetEquality` |
| `[SequenceEquality]` | `DictionaryEquality` | `SequenceEquality` | `HashSetEquality` |

```csharp
// outer dict → key-sorted; inner dict → key-sorted (propagated)
[DictionaryEquality(sequential: true)]
public Dictionary<string, Dictionary<string, int>>? ByRegionAndTeam { get; set; }

// outer dict → key-sorted; inner list → order-sensitive (default for List<T>)
[DictionaryEquality(sequential: true)]
public Dictionary<string, List<int>>? HistoryByRegion { get; set; }

// three levels deep — propagation goes all the way
[DictionaryEquality(sequential: true)]
public Dictionary<string, Dictionary<string, Dictionary<string, int>>>? ThreeLevelConfig { get; set; }
```

### Explicit overrides are always transparent

Annotations on a property are the single source of truth — they are never implied or hidden. If a `List<T>` property has no attribute, it uses `SequenceEquality`. If it has `[HashSetEquality]`, it uses `HashSetEquality`. There is no magic inference that could surprise you.

```csharp
public List<string>? Tags { get; set; }          // SequenceEquality (default)

[HashSetEquality]
public List<string>? Permissions { get; set; }   // HashSetEquality (explicit override)

[SequenceEquality]
public HashSet<string>? OrderedSet { get; set; } // SequenceEquality (explicit override)
```

---

## Multi-dimensional arrays

`T[,]`, `T[,,]`, and higher-rank arrays are supported via `MultiDimensionalArrayEqualityComparer<T>`. Use `[SequenceEquality]` to opt in.

**Supported types:** any `System.Array` with rank ≥ 2 — `T[,]`, `T[,,]`, and beyond. Single-dimensional `T[]` uses the standard `[SequenceEquality]` path instead.

```csharp
[SequenceEquality]
public int[,] Grid { get; set; }

[SequenceEquality]
public double[,,] Cube { get; set; }
```

Two arrays are equal when:
1. They have the **same rank** (`int[,]` ≠ `int[,,]`)
2. Every **dimension length** matches (`[2,3]` ≠ `[3,2]`)
3. Every **element** is equal in row-major order

```csharp
var a = new int[,] { { 1, 2 }, { 3, 4 } };
var b = new int[,] { { 1, 2 }, { 3, 4 } };
// a == b ✓

var c = new int[,] { { 1, 3 }, { 2, 4 } };  // transposed
// a != c ✓  (row-major order is sensitive to transposition)
```

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
