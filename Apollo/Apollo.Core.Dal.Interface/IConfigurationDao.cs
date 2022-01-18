using Apollo.Core.Domain;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Interface
{
    public interface IConfigurationDao : IApolloDao<Configuration>
    {
        Task<Configuration> FindByKeyAsync(string key);
    }
}
