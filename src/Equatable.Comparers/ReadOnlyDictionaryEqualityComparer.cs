namespace Equatable.Comparers;

/// <summary>
/// <see cref="IReadOnlyDictionary{TKey, TValue}"/> equality comparer instance
/// </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
public class ReadOnlyDictionaryEqualityComparer<TKey, TValue> : IEqualityComparer<IReadOnlyDictionary<TKey, TValue>>
{
    /// <summary>
    /// Gets the default equality comparer for specified generic argument.
    /// </summary>
    public static ReadOnlyDictionaryEqualityComparer<TKey, TValue> Default { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyDictionaryEqualityComparer{TKey, TValue}" /> class.
    /// </summary>
    public ReadOnlyDictionaryEqualityComparer() : this(EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyDictionaryEqualityComparer{TKey, TValue}" /> class.
    /// </summary>
    /// <param name="keyComparer">The <see cref="IEqualityComparer{TKey}"/> that is used to determine equality of keys in a dictionary</param>
    /// <param name="valueComparer">The <see cref="IEqualityComparer{TValue}"/> that is used to determine equality of values in a dictionary</param>
    /// <exception cref="ArgumentNullException"><paramref name="keyComparer"/> or <paramref name="valueComparer"/> is null</exception>
    public ReadOnlyDictionaryEqualityComparer(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
    {
        KeyComparer = keyComparer ?? throw new ArgumentNullException(nameof(keyComparer));
        ValueComparer = valueComparer ?? throw new ArgumentNullException(nameof(valueComparer));
    }

    /// <summary>
    /// Gets the <see cref="IEqualityComparer{TKey}"/> that is used to determine equality of keys in a dictionary
    /// </summary>
    public IEqualityComparer<TKey> KeyComparer { get; }

    /// <summary>
    /// Gets the <see cref="IEqualityComparer{TValue}"/> that is used to determine equality of values in a dictionary
    /// </summary>
    public IEqualityComparer<TValue> ValueComparer { get; }

    /// <inheritdoc />
    public bool Equals(IReadOnlyDictionary<TKey, TValue>? x, IReadOnlyDictionary<TKey, TValue>? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x is null || y is null)
            return false;

        if (x.Count != y.Count)
            return false;

        // y.TryGetValue uses y's own internal comparer, not this.KeyComparer.
        // Build a lookup keyed by KeyComparer so the same comparer governs both
        // Equals and GetHashCode — required for the hash contract to hold.
        var yLookup = new Dictionary<TKey, TValue>(y, KeyComparer);

        foreach (var pair in x)
        {
            if (!yLookup.TryGetValue(pair.Key, out var value))
                return false;

            if (!ValueComparer.Equals(pair.Value, value))
                return false;
        }

        return true;
    }

    /// <inheritdoc />
    public int GetHashCode(IReadOnlyDictionary<TKey, TValue> obj)
    {
        if (obj == null)
            return 0;

        // Start at 1, not 0: an empty dictionary must not hash the same as null.
        // Equals correctly returns false for (null, empty), so GetHashCode must also
        // differ — otherwise a hash table would bucket them together and force an Equals
        // call that returns false, producing unnecessary collisions. null returns 0 above;
        // 1 here ensures an empty collection is always distinguishable.
        int hashCode = 1;

        foreach (var pair in obj)
            hashCode += HashCode.Combine(KeyComparer.GetHashCode(pair.Key!), ValueComparer.GetHashCode(pair.Value!));

        return hashCode;
    }
}
