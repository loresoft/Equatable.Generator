using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Equatable.SourceGenerator.Models;

public readonly struct EquatableArray<T>(T[] array) : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    public T[] Array { get; } = array ?? [];

    public int Count => Array.Length;

    public ReadOnlySpan<T> AsSpan() => Array.AsSpan();

    public T[] AsArray() => Array;


    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) => left.Equals(right);

    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);

    public bool Equals(EquatableArray<T> array) => Array.AsSpan().SequenceEqual(array.AsSpan());

    public override bool Equals(object? obj) => obj is EquatableArray<T> array && Equals(this, array);

    public override int GetHashCode()
    {
        if (Array is not T[] array)
            return 0;

        HashCode hashCode = default;
        foreach (T item in array)
            hashCode.Add(item);

        return hashCode.ToHashCode();
    }


    IEnumerator<T> IEnumerable<T>.GetEnumerator() => (Array as IEnumerable<T>).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Array.GetEnumerator();


    public static implicit operator EquatableArray<T>(T[] array) => new(array);
}
