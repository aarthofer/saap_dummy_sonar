using System;

namespace Apollo.Core.Domain
{
    [TableName("person")]
    public class Person
    {
        [Column("id", true), PK]
        public int Id { get; set; } = -1;

        [Column("name")]
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Person person &&
                   Name == person.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }

        public override string ToString()
        {
            return $"{nameof(Person)} -> {this.Id}: {this.Name}";
        }
    }
}
