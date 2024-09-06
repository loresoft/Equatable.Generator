namespace Equatable.Comparers;

/// <summary>
/// <see cref="IEnumerable{T}"/> equality comparer instance
/// </summary>
/// <typeparam name="TValue">The type of the values.</typeparam>
public class SequenceEqualityComparer<TValue> : IEqualityComparer<IEnumerable<TValue>>
{
    /// <summary>
    /// Gets the default equality comparer for specified generic argument.
    /// </summary>
    public static SequenceEqualityComparer<TValue> Default { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceEqualityComparer{TValue}" /> class.
    /// </summary>
    public SequenceEqualityComparer() : this(EqualityComparer<TValue>.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceEqualityComparer{TValue}" /> class.
    /// </summary>
    public SequenceEqualityComparer(IEqualityComparer<TValue> comparer)
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

        return x.SequenceEqual(y, Comparer);
    }

    /// <inheritdoc />
    public int GetHashCode(IEnumerable<TValue> obj)
    {
        if (obj == null)
            return 0;

        var hashCode = new HashCode();

        foreach (var item in obj)
            hashCode.Add(item, Comparer);

        return hashCode.ToHashCode();
    }
}
