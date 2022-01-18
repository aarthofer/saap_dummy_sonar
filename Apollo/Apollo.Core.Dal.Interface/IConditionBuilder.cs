using System.Collections.Generic;

namespace Apollo.Core.Dal.Interface
{
    public enum OperationType
    {
        Equals, NotEquals, LessThen, LessThanEqual, GreaterThen, GreaterEqualThen, Contains, Like, NotLike
    }

    public interface IConditionBuilder
    {

        List<QueryParameter> Params { get; }
        IConditionBuilder NewCondition(Column column, OperationType operation, object value, string paramName = null);

        IConditionBuilder AddAnd(Column column, OperationType operation, object value, string paramName = null);

        IConditionBuilder AddAnd(IConditionBuilder condition);

        IConditionBuilder AddOr(Column column, OperationType operation, object value, string paramName = null);

        IConditionBuilder AddOr(IConditionBuilder condition);

        string getString();
    }
}