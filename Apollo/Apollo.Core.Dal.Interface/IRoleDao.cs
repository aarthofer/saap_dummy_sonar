using Apollo.Core.Domain;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Interface
{
    public interface IRoleDao : IApolloDao<Role>
    {
        Task<Role> GetOrAddRoleByNameAsync(string name);
    }
}
