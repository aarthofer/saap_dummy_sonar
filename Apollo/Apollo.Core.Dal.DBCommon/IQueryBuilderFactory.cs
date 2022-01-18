
using Apollo.Core.Dal.Interface;

namespace Apollo.Core.Dal.Factory
{
    public interface IQueryBuilderFactory
    {
        string QueryBuilderName { get; }

        IQueryBuilder GetQueryBuilder();
    }
}
