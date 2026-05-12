namespace Equatable.Comparers;

/// <summary>
/// Order-sensitive <see cref="IReadOnlyDictionary{TKey, TValue}"/> equality comparer.
/// Two dictionaries are equal only if they contain the same key/value pairs in the same insertion order.
/// </summary>
public class OrderedReadOnlyDictionaryEqualityComparer<TKey, TValue> : IEqualityComparer<IReadOnlyDictionary<TKey, TValue>>
{
    /// <summary>Gets the default equality comparer for the specified generic arguments.</summary>
    public static OrderedReadOnlyDictionaryEqualityComparer<TKey, TValue> Default { get; } = new();

    public OrderedReadOnlyDictionaryEqualityComparer() : this(EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default)
    {
    }

    public OrderedReadOnlyDictionaryEqualityComparer(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
    {
        KeyComparer = keyComparer ?? throw new ArgumentNullException(nameof(keyComparer));
        ValueComparer = valueComparer ?? throw new ArgumentNullException(nameof(valueComparer));
    }

    public IEqualityComparer<TKey> KeyComparer { get; }
    public IEqualityComparer<TValue> ValueComparer { get; }

    /// <inheritdoc />
    public bool Equals(IReadOnlyDictionary<TKey, TValue>? x, IReadOnlyDictionary<TKey, TValue>? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x is null || y is null)
            return false;

        return x.SequenceEqual(y, PairComparer);
    }

    /// <inheritdoc />
    public int GetHashCode(IReadOnlyDictionary<TKey, TValue> obj)
    {
        if (obj == null)
            return 0;

        var hashCode = new HashCode();

        foreach (var pair in obj)
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
}
