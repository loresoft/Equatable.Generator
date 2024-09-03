using System.Runtime.CompilerServices;

namespace Equatable.Comparers;

/// <summary>
/// Reference equality comparer instance
/// </summary>
/// <typeparam name="T">The type of the value to compare.</typeparam>
public class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
{
    /// <summary>
    /// Gets the default equality comparer
    /// </summary>
    public static ReferenceEqualityComparer<T> Default { get; } = new();

    /// <inheritdoc />
    public bool Equals(T? x, T? y)
    {
        return ReferenceEquals(x, y);
    }

    /// <inheritdoc />
    public int GetHashCode(T obj)
    {
        return RuntimeHelpers.GetHashCode(obj);
    }
}
