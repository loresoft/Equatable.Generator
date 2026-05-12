namespace Equatable.Comparers;

/// <summary>
/// Order-sensitive <see cref="IDictionary{TKey, TValue}"/> equality comparer.
/// Two dictionaries are equal only if they contain the same key/value pairs in the same key-sorted order.
/// </summary>
public class OrderedDictionaryEqualityComparer<TKey, TValue> : IEqualityComparer<IDictionary<TKey, TValue>>
{
    /// <summary>Gets the default equality comparer for the specified generic arguments.</summary>
    public static OrderedDictionaryEqualityComparer<TKey, TValue> Default { get; } = new();

    public OrderedDictionaryEqualityComparer() : this(EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default)
    {
    }

    public OrderedDictionaryEqualityComparer(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
    {
        KeyComparer = keyComparer ?? throw new ArgumentNullException(nameof(keyComparer));
        ValueComparer = valueComparer ?? throw new ArgumentNullException(nameof(valueComparer));
        // Prefer IComparer<TKey> from keyComparer for sort; fall back to a hash-tiebreaker
        // to guarantee strict total order (dictionary keys are unique, so ties indicate
        // a sort comparer inconsistent with key equality — hash tiebreaker fixes this).
        KeySortComparer = keyComparer as IComparer<TKey>
            ?? new HashTiebreakerComparer(keyComparer);
    }

    public IEqualityComparer<TKey> KeyComparer { get; }
    public IEqualityComparer<TValue> ValueComparer { get; }
    private IComparer<TKey> KeySortComparer { get; }

    /// <inheritdoc />
    public bool Equals(IDictionary<TKey, TValue>? x, IDictionary<TKey, TValue>? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x is null || y is null)
            return false;

        return x.OrderBy(p => p.Key, KeySortComparer).SequenceEqual(y.OrderBy(p => p.Key, KeySortComparer), PairComparer);
    }

    /// <inheritdoc />
    public int GetHashCode(IDictionary<TKey, TValue> obj)
    {
        if (obj == null)
            return 0;

        var hashCode = new HashCode();

        foreach (var pair in obj.OrderBy(p => p.Key, KeySortComparer))
        {
            hashCode.Add(pair.Key, KeyComparer);
            hashCode.Add(pair.Value, ValueComparer);
        }

        return hashCode.ToHashCode();
    }

    private KeyValuePairEqualityComparer PairComparer => new(KeyComparer, ValueComparer);

    private sealed class KeyValuePairEqualityComparer(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        : IEqualityComparer<KeyValuePair<TKey, TValue>>
    {
        public bool Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y) =>
            keyComparer.Equals(x.Key, y.Key) && valueComparer.Equals(x.Value, y.Value);

        public int GetHashCode(KeyValuePair<TKey, TValue> obj) =>
            HashCode.Combine(keyComparer.GetHashCode(obj.Key!), valueComparer.GetHashCode(obj.Value!));
    }

    // Provides a strict total order for keys that have no natural IComparer<TKey>,
    // using hash code as tiebreaker when the natural comparer returns 0 for distinct keys.
    private sealed class HashTiebreakerComparer(IEqualityComparer<TKey> equalityComparer) : IComparer<TKey>
    {
        private static readonly IComparer<TKey> _natural = Comparer<TKey>.Default;

        public int Compare(TKey? x, TKey? y)
        {
            int cmp = _natural.Compare(x, y);
            if (cmp != 0) return cmp;
            // Natural comparer considers them equal; break tie by hash code
            int hx = x is null ? 0 : equalityComparer.GetHashCode(x);
            int hy = y is null ? 0 : equalityComparer.GetHashCode(y);
            return hx.CompareTo(hy);
        }
    }
}
