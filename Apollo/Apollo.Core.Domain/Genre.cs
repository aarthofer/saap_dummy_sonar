using System;

namespace Apollo.Core.Domain
{
    [TableName("genre")]
    public class Genre
    {
        [Column("id", true), PK]
        public int Id { get; set; }

        [Column("name", false)]
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Genre genre &&
                   Name == genre.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }

        public override string ToString()
        {
            return $"{nameof(Genre)} -> {this.Id}: {this.Name}";
        }
    }
}
