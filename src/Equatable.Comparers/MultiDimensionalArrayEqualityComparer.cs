namespace Equatable.Comparers;

/// <summary>
/// Structural equality comparer for multi-dimensional arrays (T[,], T[,,], etc.).
/// Compares element-by-element in row-major order without LINQ or intermediate allocations.
/// </summary>
/// <typeparam name="TValue">The element type of the array.</typeparam>
public class MultiDimensionalArrayEqualityComparer<TValue> : IEqualityComparer<Array?>
{
    /// <summary>
    /// Gets the default equality comparer for the specified element type.
    /// </summary>
    public static MultiDimensionalArrayEqualityComparer<TValue> Default { get; } = new();

    /// <summary>
    /// Initializes a new instance using the default element comparer.
    /// </summary>
    public MultiDimensionalArrayEqualityComparer() : this(EqualityComparer<TValue>.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance using the specified element comparer.
    /// </summary>
    public MultiDimensionalArrayEqualityComparer(IEqualityComparer<TValue> comparer)
    {
        Comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
    }

    /// <summary>
    /// Gets the element comparer.
    /// </summary>
    public IEqualityComparer<TValue> Comparer { get; }

    /// <inheritdoc />
    public bool Equals(Array? x, Array? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x is null || y is null)
            return false;

        if (x.Rank != y.Rank)
            return false;

        for (int dim = 0; dim < x.Rank; dim++)
        {
            if (x.GetLength(dim) != y.GetLength(dim))
                return false;
        }

        var ex = x.GetEnumerator();
        var ey = y.GetEnumerator();
        while (ex.MoveNext())
        {
            ey.MoveNext();
            if (!Comparer.Equals((TValue)ex.Current!, (TValue)ey.Current!))
                return false;
        }

        return true;
    }

    /// <inheritdoc />
    public int GetHashCode(Array? obj)
    {
        if (obj is null)
            return 0;

        var hashCode = new HashCode();
        var e = obj.GetEnumerator();
        while (e.MoveNext())
            hashCode.Add((TValue)e.Current!, Comparer);

        return hashCode.ToHashCode();
    }
}
