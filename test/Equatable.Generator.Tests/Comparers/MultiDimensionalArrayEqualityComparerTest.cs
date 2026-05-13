using Equatable.Comparers;

namespace Equatable.Generator.Tests.Comparers;

public class MultiDimensionalArrayEqualityComparerTest
{
    private static readonly MultiDimensionalArrayEqualityComparer<int> Comparer
        = MultiDimensionalArrayEqualityComparer<int>.Default;

    // ── Equals ───────────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        int[,] a = { { 1, 2 }, { 3, 4 } };
        Assert.True(Comparer.Equals(a, a));
    }

    [Fact]
    public void Equals_BothNull_ReturnsTrue()
    {
        Assert.True(Comparer.Equals(null, null));
    }

    [Fact]
    public void Equals_OneNull_ReturnsFalse()
    {
        int[,] a = { { 1 } };
        Assert.False(Comparer.Equals(a, null));
        Assert.False(Comparer.Equals(null, a));
    }

    [Fact]
    public void Equals_DifferentRank_ReturnsFalse()
    {
        // int[,] (rank 2) vs int[,,] (rank 3)
        Array rank2 = new int[2, 2];
        Array rank3 = new int[2, 2, 2];
        var comparerObj = new MultiDimensionalArrayEqualityComparer<int>();
        Assert.False(comparerObj.Equals(rank2, rank3));
    }

    [Fact]
    public void Equals_SameRankDifferentDimensions_ReturnsFalse()
    {
        // int[2,3] vs int[3,2] — same rank, different dimension lengths
        int[,] a = new int[2, 3];
        int[,] b = new int[3, 2];
        Assert.False(Comparer.Equals(a, b));
    }

    [Fact]
    public void Equals_2D_EqualContent_ReturnsTrue()
    {
        int[,] a = { { 1, 2 }, { 3, 4 } };
        int[,] b = { { 1, 2 }, { 3, 4 } };
        Assert.True(Comparer.Equals(a, b));
    }

    [Fact]
    public void Equals_2D_DifferentContent_ReturnsFalse()
    {
        int[,] a = { { 1, 2 }, { 3, 4 } };
        int[,] b = { { 1, 2 }, { 3, 99 } };
        Assert.False(Comparer.Equals(a, b));
    }

    [Fact]
    public void Equals_3D_EqualContent_ReturnsTrue()
    {
        int[,,] a = { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } };
        int[,,] b = { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } };
        var comparer3D = new MultiDimensionalArrayEqualityComparer<int>();
        Assert.True(comparer3D.Equals(a, b));
    }

    [Fact]
    public void Equals_EmptyArrays_BothEmpty_ReturnsTrue()
    {
        Array a = new int[0, 0];
        Array b = new int[0, 0];
        Assert.True(Comparer.Equals(a, b));
    }

    [Fact]
    public void Equals_CustomComparer_UsedForElementComparison()
    {
        // OrdinalIgnoreCase: "A" and "a" are equal elements
        var ci = new MultiDimensionalArrayEqualityComparer<string>(StringComparer.OrdinalIgnoreCase);
        string[,] a = { { "Hello", "World" } };
        string[,] b = { { "hello", "WORLD" } };
        Assert.True(ci.Equals(a, b));
    }

    [Fact]
    public void Equals_CustomComparer_DefaultWouldDiffer()
    {
        // Default (ordinal) comparer would treat "A" and "a" as different
        var ordinal = new MultiDimensionalArrayEqualityComparer<string>(StringComparer.Ordinal);
        string[,] a = { { "A" } };
        string[,] b = { { "a" } };
        Assert.False(ordinal.Equals(a, b));
    }

    // ── GetHashCode ───────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetHashCode_Null_ReturnsZero()
    {
        Assert.Equal(0, Comparer.GetHashCode(null));
    }

    [Fact]
    public void GetHashCode_Empty_DiffersFromNull()
    {
        // HashCode.ToHashCode() on zero iterations returns a non-zero seed, so empty ≠ null
        Array empty = new int[0, 0];
        Assert.NotEqual(0, Comparer.GetHashCode(empty));
    }

    [Fact]
    public void GetHashCode_EqualArrays_SameHash()
    {
        int[,] a = { { 1, 2 }, { 3, 4 } };
        int[,] b = { { 1, 2 }, { 3, 4 } };
        Assert.Equal(Comparer.GetHashCode(a), Comparer.GetHashCode(b));
    }

    [Fact]
    public void GetHashCode_DifferentContent_DifferentHash()
    {
        int[,] a = { { 1, 2 }, { 3, 4 } };
        int[,] b = { { 1, 2 }, { 3, 99 } };
        Assert.NotEqual(Comparer.GetHashCode(a), Comparer.GetHashCode(b));
    }

    [Fact]
    public void GetHashCode_RowMajorOrder_SensitiveToTransposition()
    {
        // {{1,2},{3,4}} in row-major = [1,2,3,4]
        // {{1,3},{2,4}} in row-major = [1,3,2,4] — different hash
        int[,] a = { { 1, 2 }, { 3, 4 } };
        int[,] b = { { 1, 3 }, { 2, 4 } };
        Assert.NotEqual(Comparer.GetHashCode(a), Comparer.GetHashCode(b));
    }
}
