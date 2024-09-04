using Equatable.Attributes;

namespace Equatable.Entities;

public class Nested
{
    //[Equatable]
    public partial class Animal
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
