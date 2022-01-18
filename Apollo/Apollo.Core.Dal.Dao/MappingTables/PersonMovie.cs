using Apollo.Core.Domain;

namespace Apollo.Core.Dal.Dao.MappingTables
{
    [TableName("personMovie")]
    public class PersonMovie
    {
        [Column("personId"), PK]
        public int PersonId { get; set; }
        [Column("movieId"), PK]
        public int MovieId { get; set; }
        [Column("roleId"), PK]
        public int RoleId { get; set; }
    }
}
