using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Interface
{
    public interface IApolloDao<T>
    {
        Task<IEnumerable<T>> FindAllAsync();

        Task<T> FindByIdAsync(int id);

        Task<T> CreateAsync(T dao);

        Task<T> UpdateAsync(T dao);

        Task<int> DeleteByIdAsync(int id);
        Task<int> DeleteByPKAsync(T dao);

        IQueryBuilder GetQueryBuilder();
    }
}
