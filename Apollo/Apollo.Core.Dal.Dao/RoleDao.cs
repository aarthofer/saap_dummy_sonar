using Apollo.Core.Dal.Common;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Dao
{
    public class RoleDao : ApolloDao<Role>, IRoleDao
    {
        public RoleDao(IConnectionFactory connectionFactory, IQueryBuilderFactory qbFactory) : base(connectionFactory, qbFactory)
        {
        }

        public async Task<Role> GetOrAddRoleByNameAsync(string name)
        {
            var findQuery = GetQueryBuilder()
                .Table(typeof(Role))
                .AddAnd(Column.Create<Role>("name"), OperationType.Like, name);
            IEnumerable<Role> r = await template.QueryAsync<Role>(findQuery, template.GenericRowMapper<Role>);
            var single = r.Where(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (single != null)
            {
                return single;
            }
            else
            {
                return await this.CreateAsync(new Role() { Name = name });
            }
        }
    }
}
