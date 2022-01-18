using Apollo.Core.Dal.Common;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;

namespace Apollo.Core.Dal.Dao
{
    public class UserDao : ApolloDao<User>, IUserDao
    {

        public UserDao(IConnectionFactory connectionFactory, IQueryBuilderFactory qbFactory) : base(connectionFactory, qbFactory)
        {
        }
    }
}
