using Apollo.Core.Dal.Common;
using Apollo.Core.Dal.Dao.MappingTables;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using Xunit;

namespace Apollo.Core.Test
{
    public class MysqlQueryBuilderTest
    {
        [Fact]
        public void TestSimpleSelect()
        {
            string expected = "SELECT * FROM `person`;";
            IQueryBuilder qb = new MysqlQueryBuilder();

            qb.Table(typeof(Person));

            Assert.Equal(expected, qb.GetQuery());
        }

        [Fact]
        public void TestSelectColumns()
        {
            string expected = "SELECT `person`.`id`, `person`.`name` FROM `person`;";
            IQueryBuilder qb = new MysqlQueryBuilder();

            string qbStr = qb.Table(typeof(Person))
                .Columns(new[]{
                    Column.Create<Person>("id"),
                    Column.Create<Person>("name")})
                .GetQuery();

            Assert.Equal(expected, qbStr);
        }

        [Fact]
        public void TestSimpleWhere()
        {
            string expected = "SELECT * FROM `person` WHERE (`person`.`id` = @id);";
            IQueryBuilder qb = new MysqlQueryBuilder();

            qb.Table(typeof(Person))
                .SetCondition(new Column(typeof(Person), "id"), OperationType.Equals, 1);

            Assert.Equal(expected, qb.GetQuery());
        }

        [Fact]
        public void TestComplexWhere()
        {
            string expected = "SELECT * FROM `person` WHERE ((`person`.`id` = @id) AND (`person`.`name` = @name));";
            IQueryBuilder qb = new MysqlQueryBuilder();

            qb.Table(typeof(Person))
                .SetCondition(new Column(typeof(Person), "id"), OperationType.Equals, 1)
                .AddAnd(new Column(typeof(Person), "name"), OperationType.Equals, "Michael Hochp�chler");

            Assert.Equal(expected, qb.GetQuery());
        }

        [Fact]
        public void TestSelectNestedWhere()
        {
            string expected = "SELECT * FROM `person` WHERE (((`person`.`id` = @id) AND (`person`.`name` = @name2)) OR (`person`.`name` = @name1));";

            IQueryBuilder qb = new MysqlQueryBuilder();

            IConditionBuilder cb = qb.GetNewCondition();
            cb.AddAnd(new Column(typeof(Person), "name"), OperationType.Equals, "Michael Hochp�chler", "name1");


            qb.Table(typeof(Person))
                .SetCondition(new Column(typeof(Person), "id"), OperationType.Equals, 1)
                .AddAnd(new Column(typeof(Person), "name"), OperationType.Equals, "Michael", "name2")
                .AddOr(cb);

            Assert.Equal(expected, qb.GetQuery());
        }

        [Fact]
        public void TestSimpleInsertStatement()
        {
            string expected = "INSERT INTO `person` (`name`) VALUES (@name);";

            IQueryBuilder qb = new MysqlQueryBuilder();

            qb.QueryType(QueryTypeEnum.INSERT)
                .Table(typeof(Person))
                .SetColumn(new Column(typeof(Person), "name"), "TestInsert");


            Assert.Equal(expected, qb.GetQuery());

        }

        [Fact]
        public void TestOrderBy()
        {
            string expected = "SELECT * FROM `person` ORDER BY `person`.`id`;";

            IQueryBuilder qb = new MysqlQueryBuilder();
            qb.Table(typeof(Person))
                .OrderBy(OrderParameter.Create<Person>("id"));

            Assert.Equal(expected, qb.GetQuery());
        }

        [Fact]
        public void TestJoin()
        {
            string expected = "SELECT `movie`.* FROM `movie` JOIN `movieGenre` ON (`movie`.`id` = `movieGenre`.`movieId`);";

            IQueryBuilder qb = new MysqlQueryBuilder();
            qb.Table(typeof(Movie))
                .SelectColumn(Column.Create<Movie>("*"))
                .JoinTable(Column.Create<Movie>("id"), Column.Create<MovieGenre>("movieId"));

            Assert.Equal(expected, qb.GetQuery());
        }
    }
}