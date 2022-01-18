using System;
using System.Collections.Generic;

namespace Apollo.Core.Dal.Interface
{
    public enum QueryTypeEnum
    {
        SELECT,
        DELETE,
        INSERT,
        UPDATE
    };

    public interface IQueryBuilder
    {
        string GetQuery();

        IConditionBuilder GetNewCondition();

        string GetLastInsertIDQuery();

        QueryParameter[] GetParameter();

        IQueryBuilder QueryType(QueryTypeEnum type);

        IQueryBuilder Table(Type tableName);

        IQueryBuilder SelectColumn(Column column);

        IQueryBuilder SetCondition(Column column, OperationType operation, object value, string paramName = null);

        IQueryBuilder AddAnd(Column column, OperationType operation, object value, string paramName = null);

        IQueryBuilder AddAnd(IConditionBuilder condition);

        IQueryBuilder AddOr(Column column, OperationType operation, object value, string paramName = null);

        IQueryBuilder AddOr(IConditionBuilder condition);

        IQueryBuilder Columns(IEnumerable<Column> columns);

        IQueryBuilder OrderBy(OrderParameter orderBy);

        IQueryBuilder OrderByColumns(IEnumerable<OrderParameter> orderColumns);

        IQueryBuilder SetColumn(Column column, object value);

        IQueryBuilder JoinTable(Column exisingTable, Column joinedTable);

        IQueryBuilder GroupBy(Column column);

        IQueryBuilder Limit(int limit);
    }
}
