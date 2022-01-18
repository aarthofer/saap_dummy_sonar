using Apollo.Core.Dal.Interface;

namespace Apollo.Core.Dal.Factory
{
    public class DefaultQueryBuilderFactory : IQueryBuilderFactory
    {
        private IQueryBuilder qb;

        public string QueryBuilderName { get; }

        public DefaultQueryBuilderFactory(string QueryBuilder)
        {
            switch(QueryBuilder)
            {
                case "Apollo.Core.Common.QueryBuilder":
                    qb = new Apollo.Core.Dal.Common.MysqlQueryBuilder();
                    break;

                default:
                    qb = new MysqlQueryBuilder();
                    break;
            }
        }

        IQueryBuilder GetQueryBuilder()
        {
            return qb;
        }
    }
}
