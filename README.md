# Equatable.Generator

Source generator for `Equals` and `GetHashCode` with attribute-based control of equality implementation.

[![Build Project](https://github.com/loresoft/Equatable.Generator/actions/workflows/dotnet.yml/badge.svg)](https://github.com/loresoft/Equatable.Generator/actions/workflows/dotnet.yml)

[![Coverage Status](https://coveralls.io/repos/github/loresoft/Equatable.Generator/badge.svg?branch=main)](https://coveralls.io/github/loresoft/Equatable.Generator?branch=main)

[![Equatable.Generator](https://img.shields.io/nuget/v/Equatable.Generator.svg)](https://www.nuget.org/packages/Equatable.Generator/)

## Overview

By default, C# classes inherit `Equals` from `object`, which compares object references rather than values:

```csharp
var a = new Product { Id = 1, Name = "Widget" };
var b = new Product { Id = 1, Name = "Widget" };
Console.WriteLine(a == b);  // false — distinct instances, even though all values are identical
```

Structural equality — comparing objects by their field values rather than by identity — is what most application code requires. C# `record` types partially address this by generating `Equals` and `GetHashCode` from all properties automatically. However, the generated equality is only structurally correct for value types and `string`. **Reference-type properties and collection properties still fall back to reference equality**, which is silent and easily overlooked:

```csharp
record Order(int Id, List<string> Tags);

var a = new Order(1, new List<string> { "vip" });
var b = new Order(1, new List<string> { "vip" });
Console.WriteLine(a == b);  // false — List<T> uses reference equality inside the record
```

Every reference type in the object graph requires its own correct `IEquatable<T>` implementation. That obligation compounds rapidly in real domain models, and a missing implementation anywhere produces incorrect equality silently, with no compile-time indication.

> `string` properties behave correctly inside records because `string` implements `IEquatable<string>` with value semantics. This is not special treatment by the record infrastructure — any reference type that does not implement `IEquatable<T>` (such as `List<T>`, `Dictionary<K,V>`, or a plain class) will silently use reference equality.

The standard remedy is a manual `IEquatable<T>` implementation, but this introduces a different category of problems. A hand-written `Equals` method enumerates every property explicitly, and in any non-trivial codebase the following failure modes are common in practice:

- **Omitted property** — a field is absent from `Equals`, causing equality to silently disregard data that should influence the result.
- **Unintended inclusion** — a computed or infrastructure property is included, producing incorrect comparisons.
- **Stale implementation** — a new property is added to the type but not reflected in `Equals` or `GetHashCode`.
- **Hash contract violation** — `Equals` and `GetHashCode` are maintained independently and diverge over time. The contract that equal objects must produce equal hash codes is not enforced by the compiler; a violation causes silent corruption when instances are used as dictionary keys or hash set members.

These defects are difficult to detect in code review because the missing or extraneous property is buried inside a method body rather than visible at the property declaration.

This library addresses the problem through a declarative, annotation-driven approach. Equality intent is expressed directly on each property at its declaration site. The source generator produces correct `Equals` and `GetHashCode` implementations at compile time, and the accompanying Roslyn analyzer emits build warnings when an annotation is absent or incorrectly applied. There is no separate method body to maintain or keep consistent.

```csharp
[Equatable]
public partial class Product
{
    public int Id { get; set; }

    [StringEquality(StringComparison.OrdinalIgnoreCase)]
    public string? Name { get; set; }

    [IgnoreEquality]           // excluded — computed, not part of identity
    public decimal TaxAmount { get; set; }
}
```

## Packages

| Package | What it does |
|---|---|
| `Equatable.Generator` | Generates equality for `[Equatable]` classes, records, structs, and readonly structs. Includes all collection attributes. |
| `Equatable.Generator.DataContract` | Adapter — includes `[DataMember(Order = n)]`, explicitly excludes `[IgnoreDataMember]`, silently skips unannotated properties (EQ0022 warns) |
| `Equatable.Generator.MessagePack` | Adapter — includes `[Key(n)]`, explicitly excludes `[IgnoreMember]`, silently skips unannotated properties (EQ0023 warns) |
| `Equatable.Comparers` | Ships the runtime comparers used by the generated code |

## Getting started

```xml
<PackageReference Include="Equatable.Generator" PrivateAssets="all" />
<PackageReference Include="Equatable.Comparers" />
```

When using the adapter packages, add the corresponding adapter generator the same way:

```xml
<!-- DataContract adapter -->
<PackageReference Include="Equatable.Generator.DataContract" PrivateAssets="all" />

<!-- MessagePack adapter -->
<PackageReference Include="Equatable.Generator.MessagePack" PrivateAssets="all" />
```

`PrivateAssets="all"` is optional on all generator packages — it prevents the compile-time-only package from flowing to consumers of your library. `Equatable.Comparers` is the runtime package the generated code calls into and must be a real dependency.

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

The generator produces `Equals` and `GetHashCode` implementations covering every public property. Supported type declarations: `class`, `record`, `struct`, and `readonly struct`.

## All attributes at a glance

### `Equatable.Generator` — class-level trigger

| Attribute | What it does |
|---|---|
| `[Equatable]` | Triggers generation; includes all public properties |

### `Equatable.Generator` — property-level equality control

These attributes live in `Equatable.Attributes` and control how each property is compared.

| Attribute | What it generates | Default for |
|---|---|---|
| `[IgnoreEquality]` | Exclude this property from equality | — |
| `[StringEquality(StringComparison.X)]` | `StringComparer.X.Equals(a, b)` | — |
| `[EqualityComparer(typeof(T))]` | `T.Default.Equals(a, b)` — any custom `IEqualityComparer<T>` | — |
| `[ReferenceEquality]` | `Object.ReferenceEquals(a, b)` | — |
| `[SequenceEquality]` | `SequenceEqualityComparer` — order-sensitive element comparison | `List<T>`, `T[]`, `T[,]`, `T[,,]` |
| `[HashSetEquality]` | `HashSetEqualityComparer` — order-insensitive element comparison | `HashSet<T>` |
| `[DictionaryEquality]` | `DictionaryEqualityComparer` — key-value equality, insertion order irrelevant | `Dictionary<K,V>` |
| `[DictionaryEquality(sequential:true)]` | `OrderedDictionaryEqualityComparer` — key-sorted comparison | — |

### `Equatable.Generator.DataContract` — class-level trigger

| Attribute | Package | What it does |
|---|---|---|
| `[DataContractEquatable]` | `Equatable.Generator.DataContract` | Triggers generation; reads `[DataMember]` to select properties |

Property selection is driven by `System.Runtime.Serialization` attributes, which come from the BCL or the serialisation library already in use:

| Attribute | Source | Effect |
|---|---|---|
| `[DataMember]` | `System.Runtime.Serialization` | Include this property in equality |
| `[IgnoreDataMember]` | `System.Runtime.Serialization` | Explicitly exclude this property |
| `[IgnoreEquality]` | `Equatable.Attributes` | Explicitly exclude this property |

All property-level equality attributes from `Equatable.Generator` (`[SequenceEquality]`, `[DictionaryEquality]`, `[StringEquality]`, `[EqualityComparer]`, `[ReferenceEquality]`) are accepted as overrides on `[DataMember]` properties. Collection comparers are inferred automatically from the property type when no override is present.

### `Equatable.Generator.MessagePack` — class-level trigger

| Attribute | Package | What it does |
|---|---|---|
| `[MessagePackEquatable]` | `Equatable.Generator.MessagePack` | Triggers generation; reads `[Key(n)]` to select properties |

Property selection is driven by MessagePack attributes from the `MessagePack` package already in use:

| Attribute | Source | Effect |
|---|---|---|
| `[Key(n)]` | `MessagePack` | Include this property in equality |
| `[IgnoreMember]` | `MessagePack` | Explicitly exclude this property |
| `[IgnoreEquality]` | `Equatable.Attributes` | Explicitly exclude this property |

All property-level equality attributes from `Equatable.Generator` are accepted as overrides on `[Key(n)]` properties. Collection comparers are inferred automatically when no override is present.

## Adapter generators

Use `[DataContractEquatable]` or `[MessagePackEquatable]` when the type is already annotated for serialisation. The adapter derives property selection from the existing serialisation attributes, requiring no duplication of intent.

### Property selection

| Adapter | Included | Explicitly excluded | Silently skipped → EQ0022/EQ0023 |
|---|---|---|---|
| `[DataContractEquatable]` | `[DataMember]` | `[IgnoreDataMember]` or `[IgnoreEquality]` | all other public properties |
| `[MessagePackEquatable]` | `[Key(n)]` | `[IgnoreMember]` or `[IgnoreEquality]` | all other public properties |

Public properties that carry no annotation are silently excluded from equality. When that omission is unintentional, the build emits EQ0022 or EQ0023. Resolve the warning by adding the appropriate inclusion or explicit exclusion attribute.

### Comparer inference

Adapter generators infer the correct collection comparer from the property type automatically. Properties annotated with `[DataMember]` or `[Key(n)]` do not require an explicit `[SequenceEquality]`, `[DictionaryEquality]`, or `[HashSetEquality]`. The same defaults apply as for `[Equatable]`: `List<T>` / `T[]` → `SequenceEquality`; `HashSet<T>` → `HashSetEquality`; `Dictionary<K,V>` → `DictionaryEquality`.

```csharp
[DataContract]
[DataContractEquatable]
public partial class EventContract
{
    [DataMember(Order = 0)] public int EventId { get; set; }

    // List<T> → SequenceEqualityComparer inferred — no attribute required
    [DataMember(Order = 1)] public List<string>? Tags { get; set; }

    // Dictionary<K,V> → DictionaryEqualityComparer inferred — no attribute required
    [DataMember(Order = 2)] public Dictionary<string, int>? Scores { get; set; }

    [IgnoreDataMember]
    public DateTime LastSeen { get; set; }  // excluded from equality
}
```

```csharp
[MessagePackObject]
[MessagePackEquatable]
public partial class LiveScore
{
    [Key(0)] public int MatchId { get; set; }
    [Key(1)] public int HomeScore { get; set; }

    // HashSet<T> → HashSetEqualityComparer inferred — no attribute required
    [Key(2)] public HashSet<string>? Tags { get; set; }

    [IgnoreMember]
    public DateTime ReceivedAt { get; set; }  // excluded
}
```

### Overriding the inferred comparer

Explicit equality attributes take precedence over inference. All attributes from the `[Equatable]` table are applicable to adapter-included properties:

```csharp
[DataContract]
[DataContractEquatable]
public partial class EventContract
{
    // Override: treat list as a set — element order is irrelevant
    [DataMember(Order = 0)]
    [HashSetEquality]
    public List<string>? PermissionCodes { get; set; }

    // Override: case-insensitive string comparison
    [DataMember(Order = 1)]
    [StringEquality(StringComparison.OrdinalIgnoreCase)]
    public string? Region { get; set; }

    // Override: fully custom comparer
    [DataMember(Order = 2)]
    [EqualityComparer(typeof(CountOnlyComparer))]
    public Dictionary<string, int>? Weights { get; set; }
}
```

### Serialised but excluded from equality

A property can participate in serialisation while being excluded from equality. This is useful when a field tracks operational metadata — timestamps, audit info, internal sequence numbers — that is transmitted on the wire but must not affect the equality contract of the domain object.

Combine `[DataMember]` or `[Key(n)]` with `[IgnoreEquality]`:

```csharp
[DataContract]
[DataContractEquatable]
public partial class OrderDataContract
{
    [DataMember(Order = 0)]
    public int Id { get; set; }

    [DataMember(Order = 1)]
    public string? Name { get; set; }

    // Included in serialisation (Order = 2) but excluded from equality.
    // Two OrderDataContract values are equal when Id and Name match,
    // regardless of when they were last modified.
    [DataMember(Order = 2)]
    [IgnoreEquality]
    public DateTime LastModified { get; set; }
}
```

```csharp
[MessagePackObject]
[MessagePackEquatable]
public partial class PricingContract
{
    [Key(0)] public int MarketId { get; set; }
    [Key(1)] public string? Name { get; set; }

    // Serialised at key 2 but omitted from generated Equals / GetHashCode.
    [Key(2)]
    [IgnoreEquality]
    public DateTime ReceivedAt { get; set; }
}
```

The generated `Equals` and `GetHashCode` will not reference `LastModified` or `ReceivedAt` even though both properties are present in the serialised form.

---

## Collection attributes in detail

### `[SequenceEquality]` — order-sensitive comparison

**Default for:** `List<T>`, `T[]` — no attribute required on these types.

**Supported types:** any `IEnumerable<T>` — `List<T>`, `T[]`, `ICollection<T>`, `IReadOnlyList<T>`, `IEnumerable<T>`, `HashSet<T>` (via override), and more.

```csharp
public List<string>? Tracks { get; set; }   // SequenceEquality by default
public int[]? Scores { get; set; }           // SequenceEquality by default
```

`["A","B","C"]` equals `["A","B","C"]` ✓  
`["A","B","C"]` does NOT equal `["C","B","A"]` ✓

**Direction override:** apply to `HashSet<T>` to enforce order-sensitive comparison on a normally unordered type.

```csharp
[SequenceEquality]
public HashSet<string>? OrderedTags { get; set; }  // override: element order now matters
```

---

### `[HashSetEquality]` — order-insensitive comparison

**Default for:** `HashSet<T>` — no attribute required on plain hash sets.

**Supported types:** any `IEnumerable<T>` — `HashSet<T>`, `ISet<T>`, `IReadOnlySet<T>`, `List<T>`, `T[]` (via override), and more. When the value implements `ISet<T>`, `SetEquals` is called directly (fast path). Otherwise a temporary `HashSet<T>` is constructed for the comparison.

```csharp
public HashSet<string>? Roles { get; set; }  // HashSetEquality by default
```

`{"admin","editor"}` equals `{"editor","admin"}` ✓

**Direction override:** apply to `List<T>` or `T[]` to make the comparison order-insensitive.

```csharp
[HashSetEquality]
public List<string>? PermissionCodes { get; set; }  // override: element order no longer matters
```

---

### `[DictionaryEquality]` — key-value comparison, insertion order irrelevant

**Default for:** `Dictionary<K,V>` — no attribute required on plain dictionaries.

**Supported types:** any `IReadOnlyDictionary<K,V>` — `Dictionary<K,V>`, `IReadOnlyDictionary<K,V>`, `SortedDictionary<K,V>`, `ConcurrentDictionary<K,V>`, and more.

```csharp
public Dictionary<string, double>? Prices { get; set; }  // DictionaryEquality by default
```

`{a:1.85, b:1.90}` equals `{b:1.90, a:1.85}` ✓

### `[DictionaryEquality(sequential: true)]` — key-sorted comparison

Both sides are sorted by key before comparison. Insertion order is irrelevant, and the result is deterministic — useful for snapshot testing and diagnostic logging.

**Supported types:** same as `[DictionaryEquality]` — any `IReadOnlyDictionary<K,V>`.

```csharp
[DictionaryEquality(sequential: true)]
public Dictionary<string, int>? RankByRegion { get; set; }
```

---

## Nested collections

Annotate the **outer property once** — the generator selects the appropriate comparer for every nested level automatically.

### `[DictionaryEquality]`

```csharp
// outer: DictionaryEquality
// inner Dictionary: DictionaryEquality (propagated)
public Dictionary<string, Dictionary<string, int>>? ByRegion { get; set; }

// outer: DictionaryEquality
// inner List: SequenceEquality (default for List<T>)
public Dictionary<string, List<int>>? ScoresByRegion { get; set; }

// outer: DictionaryEquality
// inner HashSet: HashSetEquality (default for HashSet<T>)
public Dictionary<string, HashSet<string>>? TagsByRegion { get; set; }
```

### `[DictionaryEquality(sequential: true)]`

Key-sorted comparison propagates into every nested dictionary level.

```csharp
// outer: key-sorted dict
// inner Dictionary: key-sorted (propagated)
[DictionaryEquality(sequential: true)]
public Dictionary<string, Dictionary<string, int>>? ByRegionAndTeam { get; set; }

// outer: key-sorted dict
// inner List: SequenceEquality (default for List<T>)
[DictionaryEquality(sequential: true)]
public Dictionary<string, List<int>>? HistoryByRegion { get; set; }

// three levels deep — propagation applies at every level
[DictionaryEquality(sequential: true)]
public Dictionary<string, Dictionary<string, Dictionary<string, int>>>? ThreeLevelConfig { get; set; }
```

### `[SequenceEquality]`

```csharp
// outer: SequenceEquality (element order matters for the outer list)
// inner Dictionary: DictionaryEquality (default for Dictionary<K,V>)
[SequenceEquality]
public List<Dictionary<string, int>>? Steps { get; set; }

// outer: SequenceEquality
// inner List: SequenceEquality (default for List<T>)
[SequenceEquality]
public List<List<int>>? Matrix { get; set; }

// outer: SequenceEquality
// inner HashSet: HashSetEquality (default for HashSet<T>)
[SequenceEquality]
public List<HashSet<string>>? Groups { get; set; }
```

### Explicit overrides propagate transparently

The annotation on a property is the sole source of truth. A `List<T>` property with no attribute uses `SequenceEquality`; with `[HashSetEquality]` it uses `HashSetEquality`. There is no implicit inference that could produce unexpected results.

```csharp
public List<string>? Tags { get; set; }          // SequenceEquality (default)

[HashSetEquality]
public List<string>? Permissions { get; set; }   // HashSetEquality (explicit override)

[SequenceEquality]
public HashSet<string>? OrderedSet { get; set; } // SequenceEquality (explicit override)
```

The same principle applies inside nested collections. The outer annotation establishes the comparer kind; inner types follow their own defaults unless the outer annotation overrides them.

```csharp
// outer List → SequenceEquality (default)
// inner HashSet → HashSetEquality (default for HashSet<T>)
public List<HashSet<string>>? Groups { get; set; }

// outer List → HashSetEquality (override — the list is treated as a set of sets)
// inner HashSet → HashSetEquality (propagated from outer override)
[HashSetEquality]
public List<HashSet<string>>? GroupsUnordered { get; set; }

// outer HashSet → SequenceEquality (override — element order now matters)
// inner List → SequenceEquality (propagated from outer override)
[SequenceEquality]
public HashSet<List<int>>? OrderedGroups { get; set; }
```

---

## Multi-dimensional arrays

`T[,]`, `T[,,]`, and higher-rank arrays are handled automatically by `MultiDimensionalArrayEqualityComparer<T>` — no attribute is required.

**Default for:** any array with rank ≥ 2. Single-dimensional `T[]` uses `SequenceEqualityComparer` instead.

```csharp
// 2D array — MultiDimensionalArrayEqualityComparer applied by default
public int[,] Grid { get; set; }

// 3D array — rank is detected at compile time; same default applies
public double[,,] Cube { get; set; }
```

Two arrays are equal when:
1. They have the **same rank** (`int[,]` ≠ `int[,,]`)
2. Every **dimension length** matches (`[2,3]` ≠ `[3,2]`)
3. Every **element** is equal **in row-major order** (position matters)

```csharp
var a = new int[,] { { 1, 2 }, { 3, 4 } };
var b = new int[,] { { 1, 2 }, { 3, 4 } };
// a == b ✓  (same rank, same dimensions, same elements in row-major order)

var c = new int[,] { { 1, 3 }, { 2, 4 } };  // transposed
// a != c ✓  (row-major order: [0,0]=1,[0,1]=2,[1,0]=3,[1,1]=4 vs [0,0]=1,[0,1]=3,...)

var d = new int[,,] { { { 1, 2 }, { 3, 4 } } };
// a != d ✓  (rank 2 vs rank 3 — always unequal regardless of content)
```

### Comparer overrides for multi-dimensional arrays

`MultiDimensionalArrayEqualityComparer` is always used for rank ≥ 2 and cannot be replaced with `SequenceEqualityComparer` or `HashSetEqualityComparer`. Applying `[EqualityComparer(typeof(MyComparer))]` to a `T[,]` property does not wrap the comparer around `MultiDimensionalArrayEqualityComparer` — it bypasses it entirely, passing the array instance as a single opaque value to the custom comparer. The effective behaviour is reference equality, which is almost certainly incorrect. Rely on the default and ensure the element type defines its own correct equality.

Single-dimensional `T[]` supports the full range of comparer overrides:

```csharp
// T[] default: SequenceEquality (order matters)
public int[] Scores { get; set; }

// T[] override: HashSetEquality (order no longer matters)
[HashSetEquality]
public int[] GroupIds { get; set; }
```

---

## `[EqualityComparer]` — custom comparer

When no built-in attribute is appropriate, supply a custom `IEqualityComparer<T>`:

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

## Build-time diagnostics

The Roslyn analyzer validates every `[Equatable]` type at compile time and emits warnings when attributes are absent or incorrectly applied. These diagnostics are designed to surface mistakes that would otherwise produce silent incorrect behaviour at runtime.

### Missing attribute warnings

| Diagnostic | Applies to | Condition | Example |
|---|---|---|---|
| `EQ0001` | `[Equatable]` | `IDictionary<K,V>` or `IReadOnlyDictionary<K,V>` property has no attribute | `Dictionary<string,int>? Map` |
| `EQ0002` | `[Equatable]` | `IEnumerable<T>` property (including `T[]`) has no attribute | `List<string>? Tags`, `int[]? Ids` |
| `EQ0020` | `[DataContractEquatable]` | Class has no `[DataContract]` | — |
| `EQ0021` | `[MessagePackEquatable]` | Class has no `[MessagePackObject]` | — |
| `EQ0022` | `[DataContractEquatable]` | Public property has no `[DataMember]`, `[IgnoreDataMember]`, or `[IgnoreEquality]` | — |
| `EQ0023` | `[MessagePackEquatable]` | Public property has no `[Key(n)]`, `[IgnoreMember]`, or `[IgnoreEquality]` | — |

**EQ0001 / EQ0002** apply exclusively to `[Equatable]` types, where all public properties are included by default and collection or dictionary types must be explicitly annotated. Adapter generators infer the correct comparer automatically, so `[DataMember]` and `[Key(n)]` properties never require an additional `[SequenceEquality]` or `[DictionaryEquality]` annotation.

Multi-dimensional arrays (`T[,]`, `T[,,]`) are exempt from EQ0002 because `MultiDimensionalArrayEqualityComparer` is always applied as the default — no annotation is required or accepted.

**EQ0020 / EQ0021** detect the case where an adapter attribute is present but its corresponding serialisation attribute is absent. Without `[DataContract]`, the serialiser ignores all `[DataMember]` annotations; the generated equality would include no properties. The same applies to `[MessagePackObject]` and `[Key(n)]`.

**EQ0022 / EQ0023** detect public properties that are silently excluded on adapter-annotated types. The adapters include only properties that carry the serialisation inclusion attribute (`[DataMember]` / `[Key(n)]`); all other public properties are excluded without warning. This is intentional for computed or infrastructure properties, but an accidental omission is difficult to identify. EQ0022 and EQ0023 require the intent to be made explicit: add the inclusion attribute or an explicit exclusion attribute to suppress the diagnostic.

```csharp
[DataContract]
[DataContractEquatable]
public partial class EventContract
{
    [DataMember(Order = 0)] public int EventId { get; set; }   // included ✓

    // EQ0022 — excluded without annotation; intent is ambiguous
    public DateTime LastSeen { get; set; }

    [IgnoreDataMember] public DateTime LastSeen { get; set; }  // explicit exclusion ✓
    // or
    [IgnoreEquality]   public DateTime LastSeen { get; set; }  // explicit exclusion ✓
}
```

### Invalid attribute warnings

| Diagnostic | Condition |
|---|---|
| `EQ0010` | `[StringEquality]` applied to a non-`string` property |
| `EQ0011` | `[DictionaryEquality]` applied to a type that does not implement `IDictionary<K,V>` or `IReadOnlyDictionary<K,V>` |
| `EQ0012` | `[HashSetEquality]` applied to a type that does not implement `IEnumerable<T>` |
| `EQ0013` | `[SequenceEquality]` applied to a type that does not implement `IEnumerable<T>` |
| `EQ0014` | Any collection or equality attribute applied to a multi-dimensional array (`rank ≥ 2`) |
| `EQ0015` | `[SequenceEquality]` or `[HashSetEquality]` applied to a dictionary type (`IDictionary<K,V>` or `IReadOnlyDictionary<K,V>`) |

### EQ0014 — equality attributes have no effect on multi-dimensional arrays

`EQ0014` is emitted when any collection or equality attribute (`[SequenceEquality]`, `[HashSetEquality]`, `[DictionaryEquality]`, `[EqualityComparer]`, `[ReferenceEquality]`) is placed on a property of type `T[,]` or higher rank. The comparer for multi-dimensional arrays cannot be overridden:

- `[SequenceEquality]` and `[HashSetEquality]` are ignored; `MultiDimensionalArrayEqualityComparer` is used regardless.
- `[EqualityComparer(typeof(MyComparer))]` bypasses `MultiDimensionalArrayEqualityComparer` entirely and passes the array instance as a single value to the custom comparer, producing reference equality behaviour.

The diagnostic converts silent, incorrect behaviour into a visible compile-time warning:

```csharp
// EQ0014 — attribute has no effect on a rank-2 array
[SequenceEquality]
public int[,]? Grid { get; set; }

// Correct — no attribute required; MultiDimensionalArrayEqualityComparer is the default
public int[,]? Grid { get; set; }
```

### EQ0015 — enumerable attributes are not applicable to dictionary types

`EQ0015` is emitted when `[SequenceEquality]` or `[HashSetEquality]` is applied to a property whose type implements `IDictionary<K,V>` or `IReadOnlyDictionary<K,V>`. These attributes treat the dictionary as a flat sequence of `KeyValuePair<K,V>` entries, discarding key-lookup semantics and producing insertion-order-sensitive comparisons. Use `[DictionaryEquality]` instead:

```csharp
// EQ0015 — treats Dictionary as a sequence of KeyValuePair entries (order-sensitive)
[SequenceEquality]
public Dictionary<string, int>? Scores { get; set; }

// EQ0015 — treats Dictionary as a set of KeyValuePair entries
[HashSetEquality]
public Dictionary<string, int>? Scores { get; set; }

// Correct — key-value equality, insertion order irrelevant
[DictionaryEquality]
public Dictionary<string, int>? Scores { get; set; }
```

---

## Equality invariants

Every generated implementation satisfies the following properties:

| Property | Guarantee |
|---|---|
| **Reflexive** | `a.Equals(a)` is always `true` |
| **Symmetric** | `a.Equals(b) == b.Equals(a)` always |
| **Null-safe** | `a.Equals(null)` is always `false` |
| **Hash contract** | `a.Equals(b)` implies `a.GetHashCode() == b.GetHashCode()` |

The hash contract is critical for correct behaviour when instances are used as dictionary keys or hash set members.

---

## What's new

### New packages

- **`Equatable.Generator.DataContract`** — adapter generator that reads `[DataMember]` attributes (`System.Runtime.Serialization`). Only properties annotated with `[DataMember]` are included in equality; `[IgnoreDataMember]` and unannotated properties are excluded. EQ0022 is emitted for any public property with no annotation, requiring the intent to be made explicit.
- **`Equatable.Generator.MessagePack`** — adapter generator that reads `[Key(n)]` attributes. Only `[Key]` properties are included; `[IgnoreMember]` and unannotated properties are excluded. EQ0023 is emitted for unannotated properties.

### New features

- **`[DictionaryEquality(sequential: true)]`** — key-sorted dictionary comparison. Both sides are sorted by key before comparison, producing deterministic equality regardless of insertion order. Useful for snapshot testing and diagnostic logging. Propagates into nested dictionary values.
- **Direction overrides** — `[HashSetEquality]` on `List<T>` or `T[]` produces order-insensitive comparison; `[SequenceEquality]` on `HashSet<T>` enforces order-sensitive comparison.
- **Nested collection comparer propagation** — a single annotation on the outer property propagates the chosen comparer kind into all nested levels. `Dictionary<K, Dictionary<K2,V>>`, `Dictionary<K, List<V>>`, and three-level nesting are all handled with a single annotation.
- **`MultiDimensionalArrayEqualityComparer`** — structural equality for `T[,]`, `T[,,]`, and higher-rank arrays, applied automatically as the default. Checks rank, dimension lengths, and element values in row-major order.
- **`IReadOnlyDictionary<K,V>` support** — dictionary comparers accept any `IReadOnlyDictionary<K,V>`, not only `Dictionary<K,V>`.
- **Base class delegation** — the generated `Equals` method calls `base.Equals()` when the base class is also an equatable-generated type, including across adapter boundaries.
- **Analyzer diagnostics**
  - `EQ0020` — `[DataContractEquatable]` without `[DataContract]`
  - `EQ0021` — `[MessagePackEquatable]` without `[MessagePackObject]`
  - `EQ0022` — unannotated public property on a `[DataContractEquatable]` type
  - `EQ0023` — unannotated public property on a `[MessagePackEquatable]` type
  - `EQ0014` — equality attribute on a multi-dimensional array (`rank ≥ 2`), where the comparer cannot be overridden
  - `EQ0015` — `[SequenceEquality]` or `[HashSetEquality]` on a dictionary type

### Bug fixes

- **Empty collections hash differently from null** — a sentinel value ensures `GetHashCode(empty) != GetHashCode(null)`, satisfying the hash contract.
- **`HashSetEqualityComparer.GetHashCode` is order-independent** — uses a commutative accumulation strategy, consistent with `SetEquals`-based `Equals`.
- **`[DictionaryEquality(sequential: true)]` propagates correctly** — key-sorted mode is applied to nested dictionary values, not only the outermost level.
- **Value types without `==`** — `EqualityComparer<T>.Default` is used instead of direct operator comparison.

### Improvements

- `Equatable.Generator.DataContract` and `Equatable.Generator.MessagePack` are independent NuGet packages — include only what the project requires.
- `IsPublicInstanceProperty` extracted as a shared helper, eliminating duplication across adapter generators.
- Allocation-free hash code computation for dictionary comparers.

---

## Requirements

- Target framework: .NET Standard 2.0 or later
- C# language version: 8.0 or higher
