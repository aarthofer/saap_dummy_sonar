using Apollo.Core.Dal.Common;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Dao
{
    public class ConfigurationDao : ApolloDao<Configuration>, IConfigurationDao
    {
        public ConfigurationDao(IConnectionFactory connectionFactory, IQueryBuilderFactory qbFactory) : base(connectionFactory, qbFactory)
        {
        }

        public async Task<Configuration> FindByKeyAsync(string key)
        {
            var query = GetQueryBuilder()
                .Table(typeof(Configuration))
                .AddAnd(Column.Create<Configuration>("key"), OperationType.Equals, key);
            return await template.QuerySingleAsync(query, template.GenericRowMapper<Configuration>);
        }
    }
}
