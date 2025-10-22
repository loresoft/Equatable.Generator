using Equatable.Entities;

namespace Equatable.Generator.Tests.Entities;

public class UserImportTest
{
    [Fact]
    public void EqualsWithStringHashSetDictionarySequence()
    {
        var left = new UserImport
        {
            EmailAddress = "john.doe@email.com",
            FirstName = "John",
            LastName = "Doe",
            DisplayName = "Doe, John",
            History = [
                new DateTimeOffset(2024, 9, 1, 11, 30, 32, TimeSpan.Zero),
                new DateTimeOffset(2024, 9, 1, 11, 45, 15, TimeSpan.Zero),
            ],
            Permissions = new Dictionary<string, int>
            {
                ["read"] = 0,
                ["write"] = 1
            },
            Roles = ["reader", "writer", "administrator"]
        };

        var right = new UserImport
        {
            EmailAddress = "John.Doe@email.com",
            FirstName = "John",
            LastName = "Doe",
            DisplayName = "Doe, John",
            History = [
                new DateTimeOffset(2024, 9, 1, 11, 30, 32, TimeSpan.Zero),
                new DateTimeOffset(2024, 9, 1, 11, 45, 15, TimeSpan.Zero),
            ],
            Permissions = new Dictionary<string, int>
            {
                ["write"] = 1,
                ["read"] = 0
            },
            Roles = ["administrator", "reader", "writer"]
        };

        var isEqual = left.Equals(right);
        Assert.True(isEqual);
    }

    [Fact]
    public void NotEqualsWithString()
    {
        var left = new UserImport
        {
            EmailAddress = "john.doe@email.com",
            FirstName = "John",
            LastName = "Doe",
            DisplayName = "Doe, John",
            History = [
                new DateTimeOffset(2024, 9, 1, 11, 30, 32, TimeSpan.Zero),
                new DateTimeOffset(2024, 9, 1, 11, 45, 15, TimeSpan.Zero),
            ],
            Permissions = new Dictionary<string, int>
            {
                ["read"] = 0,
                ["write"] = 1
            },
            Roles = ["reader", "writer"]
        };

        var right = new UserImport
        {
            EmailAddress = "John.Doe@email.com",
            FirstName = "Johns",
            LastName = "Doe",
            DisplayName = "Doe, John",
            History = [
                new DateTimeOffset(2024, 9, 1, 11, 30, 32, TimeSpan.Zero),
                new DateTimeOffset(2024, 9, 1, 11, 45, 15, TimeSpan.Zero),
            ],
            Permissions = new Dictionary<string, int>
            {
                ["read"] = 0,
                ["write"] = 1
            },
            Roles = ["reader", "writer"]
        };

        var isEqual = left.Equals(right);
        Assert.False(isEqual);
    }

    [Fact]
    public void NotEqualsWithHashSet()
    {
        var left = new UserImport
        {
            EmailAddress = "john.doe@email.com",
            FirstName = "John",
            LastName = "Doe",
            DisplayName = "Doe, John",
            History = [
                new DateTimeOffset(2024, 9, 1, 11, 30, 32, TimeSpan.Zero),
                new DateTimeOffset(2024, 9, 1, 11, 45, 15, TimeSpan.Zero),
            ],
            Permissions = new Dictionary<string, int>
            {
                ["read"] = 0,
                ["write"] = 1
            },
            Roles = ["reader", "writer", "administrator"]
        };

        var right = new UserImport
        {
            EmailAddress = "John.Doe@email.com",
            FirstName = "John",
            LastName = "Doe",
            DisplayName = "Doe, John",
            History = [
                new DateTimeOffset(2024, 9, 1, 11, 30, 32, TimeSpan.Zero),
                new DateTimeOffset(2024, 9, 1, 11, 45, 15, TimeSpan.Zero),
            ],
            Permissions = new Dictionary<string, int>
            {
                ["read"] = 0,
                ["write"] = 1
            },
            Roles = ["reader", "writer"]
        };

        var isEqual = left.Equals(right);
        Assert.False(isEqual);
    }

    [Fact]
    public void NotEqualsSequence()
    {
        var left = new UserImport
        {
            EmailAddress = "john.doe@email.com",
            FirstName = "John",
            LastName = "Doe",
            DisplayName = "Doe, John",
            History = [
                new DateTimeOffset(2024, 9, 1, 11, 30, 32, TimeSpan.Zero),
                new DateTimeOffset(2024, 9, 1, 11, 45, 15, TimeSpan.Zero),
            ],
            Permissions = new Dictionary<string, int>
            {
                ["read"] = 0,
                ["write"] = 1
            },
            Roles = ["reader", "writer", "administrator"]
        };

        var right = new UserImport
        {
            EmailAddress = "John.Doe@email.com",
            FirstName = "John",
            LastName = "Doe",
            DisplayName = "Doe, John",
            History = [
                new DateTimeOffset(2024, 9, 1, 11, 30, 32, TimeSpan.Zero),
                new DateTimeOffset(2024, 9, 1, 11, 45, 15, TimeSpan.Zero),
                new DateTimeOffset(2024, 9, 1, 11, 50, 15, TimeSpan.Zero),
            ],
            Permissions = new Dictionary<string, int>
            {
                ["write"] = 1,
                ["read"] = 0
            },
            Roles = ["administrator", "reader", "writer"]
        };

        var isEqual = left.Equals(right);
        Assert.False(isEqual);
    }


    [Fact]
    public void HashCodeWithStringHashSetDictionarySequence()
    {
        var left = new UserImport
        {
            EmailAddress = "john.doe@email.com",
            FirstName = "John",
            LastName = "Doe",
            DisplayName = "Doe, John",
            History = [
                new DateTimeOffset(2024, 9, 1, 11, 30, 32, TimeSpan.Zero),
                new DateTimeOffset(2024, 9, 1, 11, 45, 15, TimeSpan.Zero),
            ],
            Permissions = new Dictionary<string, int>
            {
                ["read"] = 0,
                ["write"] = 1
            },
            Roles = ["reader", "writer", "administrator"]
        };

        var right = new UserImport
        {
            EmailAddress = "John.Doe@email.com",
            FirstName = "John",
            LastName = "Doe",
            DisplayName = "Doe, John",
            History = [
                new DateTimeOffset(2024, 9, 1, 11, 30, 32, TimeSpan.Zero),
                new DateTimeOffset(2024, 9, 1, 11, 45, 15, TimeSpan.Zero),
            ],
            Permissions = new Dictionary<string, int>
            {
                ["write"] = 1,
                ["read"] = 0
            },
            Roles = ["administrator", "reader", "writer"]
        };

        var leftCode = left.GetHashCode();
        var rightCode = right.GetHashCode();
        Assert.Equal(rightCode, leftCode);
    }

    [Fact]
    public void HashCodeNotEqualsWithString()
    {
        var left = new UserImport
        {
            EmailAddress = "john.doe@email.com",
            FirstName = "John",
            LastName = "Doe",
            DisplayName = "Doe, John",
            History = [
                new DateTimeOffset(2024, 9, 1, 11, 30, 32, TimeSpan.Zero),
                new DateTimeOffset(2024, 9, 1, 11, 45, 15, TimeSpan.Zero),
            ],
            Permissions = new Dictionary<string, int>
            {
                ["read"] = 0,
                ["write"] = 1
            },
            Roles = ["reader", "writer"]
        };

        var right = new UserImport
        {
            EmailAddress = "John.Doe@email.com",
            FirstName = "Johns",
            LastName = "Doe",
            DisplayName = "Doe, John",
            History = [
                new DateTimeOffset(2024, 9, 1, 11, 30, 32, TimeSpan.Zero),
                new DateTimeOffset(2024, 9, 1, 11, 45, 15, TimeSpan.Zero),
            ],
            Permissions = new Dictionary<string, int>
            {
                ["read"] = 0,
                ["write"] = 1
            },
            Roles = ["reader", "writer"]
        };

        var leftCode = left.GetHashCode();
        var rightCode = right.GetHashCode();
        Assert.NotEqual(rightCode, leftCode);
    }

    [Fact]
    public void HashCodeNotEqualsWithHashSet()
    {
        var left = new UserImport
        {
            EmailAddress = "john.doe@email.com",
            FirstName = "John",
            LastName = "Doe",
            DisplayName = "Doe, John",
            History = [
                new DateTimeOffset(2024, 9, 1, 11, 30, 32, TimeSpan.Zero),
                new DateTimeOffset(2024, 9, 1, 11, 45, 15, TimeSpan.Zero),
            ],
            Permissions = new Dictionary<string, int>
            {
                ["read"] = 0,
                ["write"] = 1
            },
            Roles = ["reader", "writer", "administrator"]
        };

        var right = new UserImport
        {
            EmailAddress = "John.Doe@email.com",
            FirstName = "John",
            LastName = "Doe",
            DisplayName = "Doe, John",
            History = [
                new DateTimeOffset(2024, 9, 1, 11, 30, 32, TimeSpan.Zero),
                new DateTimeOffset(2024, 9, 1, 11, 45, 15, TimeSpan.Zero),
            ],
            Permissions = new Dictionary<string, int>
            {
                ["read"] = 0,
                ["write"] = 1
            },
            Roles = ["reader", "writer"]
        };

        var leftCode = left.GetHashCode();
        var rightCode = right.GetHashCode();
        Assert.NotEqual(rightCode, leftCode);
    }

    [Fact]
    public void HashCodeNotEqualsSequence()
    {
        var left = new UserImport
        {
            EmailAddress = "john.doe@email.com",
            FirstName = "John",
            LastName = "Doe",
            DisplayName = "Doe, John",
            History = [
                new DateTimeOffset(2024, 9, 1, 11, 30, 32, TimeSpan.Zero),
                new DateTimeOffset(2024, 9, 1, 11, 45, 15, TimeSpan.Zero),
            ],
            Permissions = new Dictionary<string, int>
            {
                ["read"] = 0,
                ["write"] = 1
            },
            Roles = ["reader", "writer", "administrator"]
        };

        var right = new UserImport
        {
            EmailAddress = "John.Doe@email.com",
            FirstName = "John",
            LastName = "Doe",
            DisplayName = "Doe, John",
            History = [
                new DateTimeOffset(2024, 9, 1, 11, 30, 32, TimeSpan.Zero),
                new DateTimeOffset(2024, 9, 1, 11, 45, 15, TimeSpan.Zero),
                new DateTimeOffset(2024, 9, 1, 11, 50, 15, TimeSpan.Zero),
            ],
            Permissions = new Dictionary<string, int>
            {
                ["write"] = 1,
                ["read"] = 0
            },
            Roles = ["administrator", "reader", "writer"]
        };

        var leftCode = left.GetHashCode();
        var rightCode = right.GetHashCode();
        Assert.NotEqual(rightCode, leftCode);
    }
}
