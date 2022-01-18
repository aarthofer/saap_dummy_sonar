using Apollo.Core.Domain;

namespace Apollo.Core.Dal.Dao.MappingTables
{
    [TableName("movieGenre")]
    public class MovieGenre
    {
        [Column("genreId"), PK]
        public int GenreId { get; set; }
        [Column("movieId"), PK]
        public int MovieId { get; set; }
    }
}
