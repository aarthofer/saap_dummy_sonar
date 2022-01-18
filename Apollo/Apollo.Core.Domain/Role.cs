using System;

namespace Apollo.Core.Domain
{
    [TableName("movieRole")]
    public class Role
    {
        [Column("id", true), PK]
        public int Id { get; set; }

        [Column("name", false)]
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Role role &&
                   Name == role.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }

        public override string ToString()
        {
            return $"{nameof(Role)} -> {this.Id}: {this.Name}";
        }
    }
}
