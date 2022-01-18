using Apollo.Core.Domain;

namespace Apollo.Core.Dal.Dao.MappingTables
{
    [TableName("movieRole")]
    public class MovieRole
    {
        [Column("id"), PK]
        public int Id { get; set; }
        [Column("name"), PK]
        public int Name { get; set; }
    }
}
