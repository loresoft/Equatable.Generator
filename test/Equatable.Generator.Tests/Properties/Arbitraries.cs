namespace Equatable.Generator.Tests.Properties;

/// <summary>
/// Custom FsCheck v3 Arbitrary instances for types not auto-generated (HashSet).
/// FsCheck v3 doesn't auto-derive HashSet — register via [Properties(Arbitrary = new[] { typeof(Arbitraries) })].
/// </summary>
public static class Arbitraries
{
    public static Arbitrary<HashSet<string>> HashSetOfString() =>
        Arb.From(
            Gen.ArrayOf(Gen.Choose(0, 100).Select(i => i.ToString()))
               .Select(arr => new HashSet<string>(arr ?? [])));

    public static Arbitrary<HashSet<int>> HashSetOfInt() =>
        Arb.From(
            Gen.ArrayOf(Gen.Choose(-100, 100))
               .Select(arr => new HashSet<int>(arr ?? [])));

    public static Arbitrary<HashSet<List<int>>> HashSetOfListOfInt() =>
        Arb.From(
            Gen.ArrayOf(Gen.ArrayOf(Gen.Choose(-100, 100)).Select(a => a.ToList()))
               .Select(arr => new HashSet<List<int>>(arr ?? [])));

    public static Arbitrary<HashSet<Dictionary<string, int>>> HashSetOfDictionaryOfStringInt() =>
        Arb.From(
            Gen.ArrayOf(
                Gen.ArrayOf(
                    Gen.Zip(Gen.Choose(0, 20).Select(i => i.ToString()), Gen.Choose(-100, 100)))
                   .Select(pairs => pairs.DistinctBy(p => p.Item1)
                                        .ToDictionary(p => p.Item1, p => p.Item2)))
               .Select(arr => new HashSet<Dictionary<string, int>>(arr ?? [])));
}
