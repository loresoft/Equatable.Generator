namespace Equatable.SourceGenerator.Models;

public enum ComparerTypes
{
    Default,
    Dictionary,
    HashSet,
    Reference,
    Sequence,
    String,
    ValueType,
    Custom,
    // a fully-composed IEqualityComparer<T> expression built by the generator for nested collections
    Expression
}
