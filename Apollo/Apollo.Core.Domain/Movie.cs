using System;
using System.Collections.Generic;

namespace Apollo.Core.Domain
{
    [TableName("movie")]
    public class Movie
    {
        [Column("id", true), PK]
        public int Id { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string Description { get; set; }

        public IEnumerable<Genre> Genre { get; set; } = new List<Genre>();

        public IEnumerable<Person> Actors { get; set; } = new List<Person>();

        public IEnumerable<KeyValuePair<Person, string>> Crew { get; set; } = new List<KeyValuePair<Person, string>>();

        [Column("durationMinutes")]
        public int DurationMinutes { get; set; }

        [Column("image")]
        public string Image { get; set; }

        [Column("trailer")]
        public string Trailer { get; set; }


        [Column("releasedate")]
        public DateTime ReleaseDate { get; set; }

        public override string ToString()
        {
            return $"{nameof(Movie)} -> {this.Id}: {this.Title}";
        }
    }

    public class MovieIdComparer : IEqualityComparer<Movie>
    {
        public bool Equals(Movie x, Movie y)
        {
            return x?.Id == y?.Id;
        }

        public int GetHashCode(Movie obj)
        {
            return HashCode.Combine(obj?.Id);
        }
    }
}
