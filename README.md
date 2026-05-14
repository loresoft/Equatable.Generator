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

The correct fix is to implement `IEquatable<T>` manually — but that creates a different set of problems. A hand-written `Equals` method must list every property explicitly, and in a large codebase it is easy to:

- **forget a property** — equality silently ignores a field that should matter
- **include a property by mistake** — a computed or infrastructure field ends up in the comparison
- **miss the update** — a new property is added to the class but not to the `Equals` / `GetHashCode` methods

These bugs are hard to spot in code review because the missing or extra property is somewhere in a long method body, not at the declaration site.

This library solves the problem with a declarative, annotation-driven approach. Mark the class and each property **at the declaration** — the generator writes `Equals` and `GetHashCode` at compile time, and the analyzer warns immediately when an annotation is missing or misused. The intent is visible right next to the property; there is no separate method to keep in sync.

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

The generator writes `Equals` and `GetHashCode` for every public property. Works on `class`, `record`, `struct`, and `readonly struct`.

## All attributes at a glance

### `Equatable.Generator` — class-level trigger

| Attribute | What it does |
|---|---|
| `[Equatable]` | Triggers generation; includes all public properties |

### `Equatable.Generator` — property-level equality control

These attributes live in `Equatable.Attributes` and control how each property is compared.

| Attribute | What it generates | Default for |
|---|---|---|
| `[IgnoreEquality]` | Skip this property | — |
| `[StringEquality(StringComparison.X)]` | `StringComparer.X.Equals(a, b)` | — |
| `[EqualityComparer(typeof(T))]` | `T.Default.Equals(a, b)` — any custom comparer | — |
| `[ReferenceEquality]` | `Object.ReferenceEquals(a, b)` | — |
| `[SequenceEquality]` | `SequenceEqualityComparer` — element order matters | `List<T>`, `T[]`, `T[,]`, `T[,,]` |
| `[HashSetEquality]` | `HashSetEqualityComparer` — element order ignored | `HashSet<T>` |
| `[DictionaryEquality]` | `DictionaryEqualityComparer` — key-value equality, insertion order irrelevant | `Dictionary<K,V>` |
| `[DictionaryEquality(sequential:true)]` | `OrderedDictionaryEqualityComparer` — key-sorted comparison | — |

### `Equatable.Generator.DataContract` — class-level trigger

| Attribute | Package | What it does |
|---|---|---|
| `[DataContractEquatable]` | `Equatable.Generator.DataContract` | Triggers generation; reads `[DataMember]` to select properties |

Property selection uses `System.Runtime.Serialization` attributes — these are not part of `Equatable.Generator.DataContract` itself, they come from the BCL or the serialisation library you already use:

| Attribute | Source | Effect |
|---|---|---|
| `[DataMember]` | `System.Runtime.Serialization` | Include this property in equality |
| `[IgnoreDataMember]` | `System.Runtime.Serialization` | Explicitly exclude this property |
| `[IgnoreEquality]` | `Equatable.Attributes` | Explicitly exclude this property |

All property-level equality attributes from `Equatable.Generator` (`[SequenceEquality]`, `[DictionaryEquality]`, `[StringEquality]`, `[EqualityComparer]`, `[ReferenceEquality]`) work as overrides on `[DataMember]` properties. Collection comparers are inferred automatically when no override is present.

### `Equatable.Generator.MessagePack` — class-level trigger

| Attribute | Package | What it does |
|---|---|---|
| `[MessagePackEquatable]` | `Equatable.Generator.MessagePack` | Triggers generation; reads `[Key(n)]` to select properties |

Property selection uses MessagePack attributes — these come from the `MessagePack` package you already use:

| Attribute | Source | Effect |
|---|---|---|
| `[Key(n)]` | `MessagePack` | Include this property in equality |
| `[IgnoreMember]` | `MessagePack` | Explicitly exclude this property |
| `[IgnoreEquality]` | `Equatable.Attributes` | Explicitly exclude this property |

All property-level equality attributes from `Equatable.Generator` work as overrides on `[Key(n)]` properties. Collection comparers are inferred automatically when no override is present.

## Adapter generators

Use `[DataContractEquatable]` or `[MessagePackEquatable]` when your class is already annotated for serialisation. The adapter reads the existing serialisation attributes to decide which properties to include — no duplication of intent required.

### Property selection

| Adapter | Included | Explicitly excluded | Silently skipped → EQ0022/EQ0023 |
|---|---|---|---|
| `[DataContractEquatable]` | `[DataMember]` | `[IgnoreDataMember]` or `[IgnoreEquality]` | all other public properties |
| `[MessagePackEquatable]` | `[Key(n)]` | `[IgnoreMember]` or `[IgnoreEquality]` | all other public properties |

Properties with no annotation at all are silently excluded from equality. If that omission was accidental the build will warn (EQ0022 / EQ0023) — add the inclusion or exclusion attribute to make the intent explicit.

### Comparer inference — no equality attributes needed on collection properties

Adapters auto-infer the correct collection comparer from the property type, so `[DataMember]` / `[Key(n)]` properties never need an explicit `[SequenceEquality]`, `[DictionaryEquality]`, or `[HashSetEquality]`. The same defaults apply as for `[Equatable]`: `List<T>` / `T[]` → `SequenceEquality`; `HashSet<T>` → `HashSetEquality`; `Dictionary<K,V>` → `DictionaryEquality`.

```csharp
[DataContract]
[DataContractEquatable]
public partial class EventContract
{
    [DataMember(Order = 0)] public int EventId { get; set; }

    // List<T> → SequenceEqualityComparer inferred — no attribute needed
    [DataMember(Order = 1)] public List<string>? Tags { get; set; }

    // Dictionary<K,V> → DictionaryEqualityComparer inferred — no attribute needed
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

    // HashSet<T> → HashSetEqualityComparer inferred — no attribute needed
    [Key(2)] public HashSet<string>? Tags { get; set; }

    [IgnoreMember]
    public DateTime ReceivedAt { get; set; }  // excluded
}
```

### Overriding the inferred comparer

Explicit equality attributes take priority over inference. All the same attributes from the `[Equatable]` table work on adapter-included properties:

```csharp
[DataContract]
[DataContractEquatable]
public partial class EventContract
{
    // Override: treat list as a set (order irrelevant)
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

Annotate the **outer property once** — the generator infers the right comparer for every nested level automatically.

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

// three levels deep — propagation goes all the way
[DictionaryEquality(sequential: true)]
public Dictionary<string, Dictionary<string, Dictionary<string, int>>>? ThreeLevelConfig { get; set; }
```

### `[SequenceEquality]`

```csharp
// outer: SequenceEquality (order matters for the outer list)
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

### Explicit overrides are always transparent

Annotations on a property are the single source of truth — they are never implied or hidden. If a `List<T>` property has no attribute, it uses `SequenceEquality`. If it has `[HashSetEquality]`, it uses `HashSetEquality`. There is no magic inference that could surprise you.

```csharp
public List<string>? Tags { get; set; }          // SequenceEquality (default)

[HashSetEquality]
public List<string>? Permissions { get; set; }   // HashSetEquality (explicit override)

[SequenceEquality]
public HashSet<string>? OrderedSet { get; set; } // SequenceEquality (explicit override)
```

The same logic applies inside nested collections. The outer annotation sets the comparer kind; inner types follow their own defaults unless they are themselves the type you are overriding at the outer level.

```csharp
// outer List → SequenceEquality (default, no attribute needed)
// inner HashSet → HashSetEquality (default for HashSet)
public List<HashSet<string>>? Groups { get; set; }

// outer List → HashSetEquality (override — treat the list as a set of sets)
// inner HashSet → HashSetEquality (propagated from outer override)
[HashSetEquality]
public List<HashSet<string>>? GroupsUnordered { get; set; }

// outer HashSet → SequenceEquality (override — order now matters for the outer set)
// inner List → SequenceEquality (propagated from outer override)
[SequenceEquality]
public HashSet<List<int>>? OrderedGroups { get; set; }
```

---

## Multi-dimensional arrays

`T[,]`, `T[,,]`, and higher-rank arrays are handled by `MultiDimensionalArrayEqualityComparer<T>` — no attribute needed, just like `T[]`.

**Default for:** any array with rank ≥ 2. Single-dimensional `T[]` uses `SequenceEqualityComparer` instead.

```csharp
// 2D array — MultiDimensionalArrayEqualityComparer by default, no attribute needed
public int[,] Grid { get; set; }

// 3D array — same default, rank detected automatically at compile time
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

### Overrides for multi-dimensional arrays

The outer comparer is always `MultiDimensionalArrayEqualityComparer` for rank ≥ 2 — it cannot be swapped for `SequenceEqualityComparer` or `HashSetEqualityComparer`. There is no supported element-level override: `[EqualityComparer]` on a `T[,]` property bypasses `MultiDimensionalArrayEqualityComparer` entirely and compares the array as a single reference, which is incorrect. Use the default and rely on element type's own equality instead.

Single-dimensional `T[]` is more flexible — it supports all comparer overrides:

```csharp
// T[] default: SequenceEquality (order matters)
public int[] Scores { get; set; }

// T[] override: HashSetEquality (order no longer matters)
[HashSetEquality]
public int[] GroupIds { get; set; }
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

## Build-time diagnostics

The analyzer validates every `[Equatable]` class at compile time and emits warnings when attributes are missing or misused. These diagnostics are designed to surface mistakes that would otherwise produce silent wrong behavior at runtime.

### Missing attribute warnings

| Diagnostic | Applies to | Condition | Example |
|---|---|---|---|
| `EQ0001` | `[Equatable]` | `IDictionary<K,V>` or `IReadOnlyDictionary<K,V>` property has no attribute | `Dictionary<string,int>? Map` |
| `EQ0002` | `[Equatable]` | `IEnumerable<T>` property (including `T[]`) has no attribute | `List<string>? Tags`, `int[]? Ids` |
| `EQ0020` | `[DataContractEquatable]` | Class has no `[DataContract]` | — |
| `EQ0021` | `[MessagePackEquatable]` | Class has no `[MessagePackObject]` | — |
| `EQ0022` | `[DataContractEquatable]` | Public property has no `[DataMember]`, `[IgnoreDataMember]`, or `[IgnoreEquality]` | — |
| `EQ0023` | `[MessagePackEquatable]` | Public property has no `[Key(n)]`, `[IgnoreMember]`, or `[IgnoreEquality]` | — |

**EQ0001 / EQ0002** only apply to `[Equatable]` classes, where every public property is included by default and must be explicitly annotated for collection/dictionary types. Adapter generators (`[DataContractEquatable]`, `[MessagePackEquatable]`) auto-infer the correct comparer from the property type, so collection properties annotated with `[DataMember]` or `[Key(n)]` never need `[SequenceEquality]` or `[DictionaryEquality]`.

Multi-dimensional arrays (`T[,]`, `T[,,]`) are exempt from EQ0002 because `MultiDimensionalArrayEqualityComparer` is always the default — no annotation is needed or accepted.

**EQ0020 / EQ0021** catch the case where the adapter attribute is added but the corresponding serialisation attribute is missing. Without `[DataContract]` the serialiser ignores all `[DataMember]` annotations, so the generated equality would silently include no properties. The same applies to `[MessagePackObject]` / `[Key(n)]`.

**EQ0022 / EQ0023** catch silently excluded properties on adapter-annotated types. The adapters only include properties that carry the serialisation inclusion attribute (`[DataMember]` / `[Key(n)]`) — all other public properties are silently skipped. This is intentional for computed properties or infrastructure fields you never want serialised, but an accidental omission is hard to notice. EQ0022/EQ0023 force the intent to be explicit: either add the inclusion attribute or add an explicit exclusion to suppress the warning:

```csharp
[DataContract]
[DataContractEquatable]
public partial class EventContract
{
    [DataMember(Order = 0)] public int EventId { get; set; }   // included ✓

    // EQ0022 — silently excluded; was this intentional?
    public DateTime LastSeen { get; set; }

    [IgnoreDataMember]      public DateTime LastSeen { get; set; }  // explicit exclusion ✓
    // or
    [IgnoreEquality]        public DateTime LastSeen { get; set; }  // explicit exclusion ✓
}
```

### Invalid attribute warnings

| Diagnostic | Condition |
|---|---|
| `EQ0010` | `[StringEquality]` on a non-`string` property |
| `EQ0011` | `[DictionaryEquality]` on a type that does not implement `IDictionary<K,V>` or `IReadOnlyDictionary<K,V>` |
| `EQ0012` | `[HashSetEquality]` on a type that does not implement `IEnumerable<T>` |
| `EQ0013` | `[SequenceEquality]` on a type that does not implement `IEnumerable<T>` |
| `EQ0014` | Any collection or equality attribute on a multi-dimensional array (`rank ≥ 2`) |
| `EQ0015` | `[SequenceEquality]` or `[HashSetEquality]` on a dictionary type (`IDictionary<K,V>` or `IReadOnlyDictionary<K,V>`) |

### EQ0014 — attributes have no effect on multi-dimensional arrays

`EQ0014` fires whenever any collection or equality attribute (`[SequenceEquality]`, `[HashSetEquality]`, `[DictionaryEquality]`, `[EqualityComparer]`, `[ReferenceEquality]`) is placed on a `T[,]` or higher-rank array property. This is intentional and expected — it is not possible to override the comparer for a multi-dimensional array:

- `[SequenceEquality]` and `[HashSetEquality]` are silently ignored; `MultiDimensionalArrayEqualityComparer` is used regardless.
- `[EqualityComparer(typeof(MyComparer))]` appears to work but actually bypasses `MultiDimensionalArrayEqualityComparer` entirely, passing the whole array object as a single value to `MyComparer`. The result is effectively reference equality — almost certainly not what was intended.

The diagnostic turns a silent, surprising behavior into a loud, visible one at compile time:

```csharp
// EQ0014 — attribute has no effect on rank-2 array
[SequenceEquality]
public int[,]? Grid { get; set; }

// Correct — no attribute needed; MultiDimensionalArrayEqualityComparer is the default
public int[,]? Grid { get; set; }
```

### EQ0015 — enumerable attributes have no useful meaning on dictionary types

`EQ0015` fires when `[SequenceEquality]` or `[HashSetEquality]` is applied to a property whose type implements `IDictionary<K,V>` or `IReadOnlyDictionary<K,V>`. These attributes treat the dictionary as a flat sequence of `KeyValuePair<K,V>` entries, which discards key-lookup semantics and produces comparisons that are sensitive to insertion order. Use `[DictionaryEquality]` instead:

```csharp
// EQ0015 — treats Dictionary as a sequence of KeyValuePair entries (order-sensitive, wrong)
[SequenceEquality]
public Dictionary<string, int>? Scores { get; set; }

// EQ0015 — treats Dictionary as a set of KeyValuePair entries (still wrong)
[HashSetEquality]
public Dictionary<string, int>? Scores { get; set; }

// Correct — key-value equality, insertion order irrelevant
[DictionaryEquality]
public Dictionary<string, int>? Scores { get; set; }
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

## What's new

### New packages

- **`Equatable.Generator.DataContract`** — adapter generator that reads `[DataMember]` attributes (`System.Runtime.Serialization`). Only properties annotated with `[DataMember]` are included in equality; `[IgnoreDataMember]` and unannotated properties are excluded. A build warning (EQ0022) fires for any public property with no annotation at all, forcing the intent to be explicit.
- **`Equatable.Generator.MessagePack`** — adapter generator that reads `[Key(n)]` attributes. Only `[Key]` properties are included; `[IgnoreMember]` properties and unannotated properties are excluded. EQ0023 warns on unannotated properties.

### New features

- **`[DictionaryEquality(sequential: true)]`** — key-sorted dictionary comparison. Both sides are sorted by key before comparing, making equality deterministic regardless of insertion order. Useful for snapshots and logs. Propagates into nested dictionary values.
- **Direction overrides** — apply `[HashSetEquality]` to `List<T>` or `T[]` to make them order-insensitive; apply `[SequenceEquality]` to `HashSet<T>` to force order-sensitive comparison.
- **Nested collection comparer propagation** — annotate the outer property once; the chosen comparer kind propagates automatically into all nested levels. `Dictionary<K, Dictionary<K2,V>>`, `Dictionary<K, List<V>>`, three-level nesting — all handled with a single annotation.
- **`MultiDimensionalArrayEqualityComparer`** — structural equality for `T[,]`, `T[,,]`, and higher-rank arrays. Applied automatically as the default — no attribute needed. Checks rank, dimension lengths, and elements in row-major order.
- **`IReadOnlyDictionary<K,V>` support** — dictionary comparers now accept any `IReadOnlyDictionary<K,V>`, not just `Dictionary<K,V>`.
- **Base class delegation** — generated `Equals` calls `base.Equals()` when the base class is also an equatable-generated type. Works across adapter boundaries (e.g. `[Equatable]` derived from `[DataContractEquatable]`).
- **Analyzer diagnostics**
  - `EQ0020` — `[DataContractEquatable]` without `[DataContract]`
  - `EQ0021` — `[MessagePackEquatable]` without `[MessagePackObject]`
  - `EQ0022` — unannotated public property on a `[DataContractEquatable]` type
  - `EQ0023` — unannotated public property on a `[MessagePackEquatable]` type
  - `EQ0014` — any collection or equality attribute on a multi-dimensional array (`rank ≥ 2`), where the comparer cannot be overridden
  - `EQ0015` — `[SequenceEquality]` or `[HashSetEquality]` on a dictionary type

### Bug fixes

- **Empty collections hash differently from null** — a sentinel value is used for empty sequences and dictionaries, satisfying the hash contract (`GetHashCode(empty) != GetHashCode(null)`).
- **`HashSetEqualityComparer.GetHashCode` is order-independent** — uses a commutative sum, consistent with `SetEquals`-based `Equals`.
- **`[DictionaryEquality(sequential: true)]` propagates correctly** — key-sorted mode is applied to nested dictionary values, not just the outermost level.
- **Value types without `==`** — `EqualityComparer<T>.Default` is used instead of direct comparison.

### Improvements

- `Equatable.Generator.DataContract` and `Equatable.Generator.MessagePack` are separate NuGet packages — add only what you need.
- `IsPublicInstanceProperty` extracted as a shared helper, eliminating duplication between adapter generators.
- Allocation-free hash code computation for dictionary comparers.

---

## Requirements

- Target framework .NET Standard 2.0 or greater
- C# `LangVersion` 8.0 or higher
