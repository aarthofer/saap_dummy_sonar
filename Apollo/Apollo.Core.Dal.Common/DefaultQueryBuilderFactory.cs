using Apollo.Core.Dal.Interface;

namespace Apollo.Core.Dal.Common
{
    public class DefaultQueryBuilderFactory : IQueryBuilderFactory
    {

        public string QueryBuilderName { get; }

        public DefaultQueryBuilderFactory(string QueryBuilder)
        {
            this.QueryBuilderName = QueryBuilder;
        }

        public IQueryBuilder GetQueryBuilder()
        {
            switch (QueryBuilderName)
            {
                case "Apollo.Core.Common.QueryBuilder":
                    return new MysqlQueryBuilder();

                default:
                    return new MysqlQueryBuilder();
            }
        }
    }
}
