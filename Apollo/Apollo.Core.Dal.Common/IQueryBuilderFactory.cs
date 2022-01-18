
using Apollo.Core.Dal.Interface;

namespace Apollo.Core.Dal.Common
{
    public interface IQueryBuilderFactory
    {
        string QueryBuilderName { get; }

        IQueryBuilder GetQueryBuilder();
    }
}
