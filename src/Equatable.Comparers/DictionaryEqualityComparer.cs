namespace Equatable.Comparers;

/// <summary>
/// <see cref="Dictionary{TKey, TValue}"/> equality comparer instance
/// </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
public class DictionaryEqualityComparer<TKey, TValue> : IEqualityComparer<IDictionary<TKey, TValue>>
{
    /// <summary>
    /// Gets the default equality comparer for specified generic argument.
    /// </summary>
    public static DictionaryEqualityComparer<TKey, TValue> Default { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryEqualityComparer{TKey, TValue}" /> class.
    /// </summary>
    public DictionaryEqualityComparer() : this(EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryEqualityComparer{TKey, TValue}" /> class.
    /// </summary>
    /// <param name="keyComparer">The <see cref="IEqualityComparer{TKey}"/> that is used to determine equality of keys in a dictionary</param>
    /// <param name="valueComparer">The <see cref="IEqualityComparer{TValue}"/> that is used to determine equality of values in a dictionary</param>
    /// <exception cref="ArgumentNullException"><paramref name="keyComparer"/> or <paramref name="valueComparer"/> is null</exception>
    public DictionaryEqualityComparer(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
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
    public bool Equals(IDictionary<TKey, TValue>? x, IDictionary<TKey, TValue>? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x == null || y == null)
            return false;

        if (x.Count != y.Count)
            return false;

        foreach (var pair in x)
        {
            if (!y.TryGetValue(pair.Key, out var value))
                return false;

            if (!ValueComparer.Equals(pair.Value, value))
                return false;
        }

        return true;
    }

    /// <inheritdoc />
    public int GetHashCode(IDictionary<TKey, TValue> obj)
    {
        if (obj == null)
            return 0;

        var hash = new HashCode();

        // sort by key to ensure dictionary with different order are the same
        foreach (var pair in obj.OrderBy(d => d.Key))
        {
            hash.Add(pair.Key, KeyComparer);
            hash.Add(pair.Value, ValueComparer);
        }

        return hash.ToHashCode();
    }
}
