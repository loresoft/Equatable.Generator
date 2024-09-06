namespace Equatable.Comparers;

/// <summary>
/// <see cref="IEnumerable{T}"/> equality comparer instance
/// </summary>
/// <typeparam name="TValue">The type of the values.</typeparam>
public class HashSetEqualityComparer<TValue> : IEqualityComparer<IEnumerable<TValue>>
{
    /// <summary>
    /// Gets the default equality comparer for specified generic argument.
    /// </summary>
    public static HashSetEqualityComparer<TValue> Default { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="HashSetEqualityComparer{TValue}" /> class.
    /// </summary>
    public HashSetEqualityComparer() : this(EqualityComparer<TValue>.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HashSetEqualityComparer{TValue}" /> class.
    /// </summary>
    public HashSetEqualityComparer(IEqualityComparer<TValue> comparer)
    {
        Comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
    }

    /// <summary>
    /// Gets the <see cref="IEqualityComparer{T}"/> that is used to determine equality of values
    /// </summary>
    public IEqualityComparer<TValue> Comparer { get; }

    /// <inheritdoc />
    public bool Equals(IEnumerable<TValue>? x, IEnumerable<TValue>? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x is null || y is null)
            return false;

        if (x is ISet<TValue> xSet)
            return xSet.SetEquals(y);

        if (y is ISet<TValue> ySet)
            return ySet.SetEquals(x);

        xSet = new HashSet<TValue>(x, Comparer);
        return xSet.SetEquals(y);
    }

    /// <inheritdoc />
    public int GetHashCode(IEnumerable<TValue> obj)
    {
        if (obj == null)
            return 0;

        var hashCode = new HashCode();

        // sort to ensure set with different order are the same
        foreach (var item in obj.OrderBy(s => s))
            hashCode.Add(item, Comparer);

        return hashCode.ToHashCode();
    }
}
