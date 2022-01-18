using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Apollo.Core.Dal.Common
{
    public class MysqlQueryBuilder : IQueryBuilder
    {
        private QueryTypeEnum queryType { get; set; } = Interface.QueryTypeEnum.SELECT;
        private IConditionBuilder conditionBuilder { get; set; } = new MysqlConditionBuilder();
        private List<Column> selectedColumns { get; set; } = new List<Column>();
        private List<Tuple<Column, Column>> joinedTables { get; set; } = new List<Tuple<Column, Column>>();
        private Dictionary<Column, object> setColumns { get; set; } = new Dictionary<Column, object>();
        private Column groupBy { get; set; } = null;
        private List<OrderParameter> orderBy { get; set; } = new List<OrderParameter>();
        private int limit { get; set; } = 0;
        private string table { get; set; } = "";

        public string GetQuery()
        {
            switch (queryType)
            {
                case Interface.QueryTypeEnum.SELECT:
                    return CreateSelectStatement();

                case Interface.QueryTypeEnum.INSERT:
                    return CreateInsertStatement();

                case Interface.QueryTypeEnum.DELETE:
                    return CreateDeleteFromStatement();

                case Interface.QueryTypeEnum.UPDATE:
                    return CreateUpdateStatement();

                default:
                    throw new NotImplementedException($"{queryType} not implemented");
            }
        }

        public string GetLastInsertIDQuery()
        {
            return "SELECT LAST_INSERT_ID() as id;";
        }

        public IConditionBuilder GetNewCondition()
        {
            return new MysqlConditionBuilder();
        }

        public QueryParameter[] GetParameter()
        {
            List<QueryParameter> tempParameter = new List<QueryParameter>();

            if (queryType == Interface.QueryTypeEnum.INSERT || queryType == Interface.QueryTypeEnum.UPDATE)
            {
                tempParameter.AddRange(setColumns.Select(t => new QueryParameter($"@{t.Key.ColumnName}", t.Value)));
            }

            tempParameter.AddRange(conditionBuilder.Params);

            return tempParameter.ToArray();
        }

        public IQueryBuilder QueryType(QueryTypeEnum type)
        {
            this.queryType = type;
            return this;
        }

        public IQueryBuilder Columns(IEnumerable<Column> columns)
        {
            this.selectedColumns = columns.ToList<Column>();
            return this;
        }

        public IQueryBuilder SetCondition(Column column, OperationType operation, object value, string paramName = null)
        {
            conditionBuilder.NewCondition(column, operation, value, paramName);
            return this;
        }

        public IQueryBuilder AddAnd(Column column, OperationType operation, object value, string paramName = null)
        {
            conditionBuilder.AddAnd(column, operation, value, paramName);
            return this;
        }

        public IQueryBuilder AddAnd(IConditionBuilder condition)
        {
            conditionBuilder.AddAnd(condition);
            return this;
        }

        public IQueryBuilder AddOr(Column column, OperationType operation, object value, string paramName = null)
        {
            conditionBuilder.AddOr(column, operation, value, paramName);
            return this;
        }

        public IQueryBuilder AddOr(IConditionBuilder condition)
        {
            conditionBuilder.AddOr(condition);
            return this;
        }

        public IQueryBuilder GroupBy(Column column)
        {
            groupBy = column;
            return this;
        }

        public IQueryBuilder JoinTable(Column existingTable, Column joinedTable)
        {
            joinedTables.Add(new Tuple<Column, Column>(existingTable, joinedTable));
            return this;
        }

        public IQueryBuilder Limit(int limit)
        {
            if (limit < 0)
            {
                throw new ArgumentException("Limit has to be > 0");
            }
            this.limit = limit;
            return this;
        }

        public IQueryBuilder OrderBy(OrderParameter column)
        {
            orderBy.Add(column);
            return this;
        }

        public IQueryBuilder OrderByColumns(IEnumerable<OrderParameter> orderColumns)
        {
            foreach (var column in orderColumns)
            {
                OrderBy(column);
            }

            return this;
        }

        public IQueryBuilder QueryTypeEnum(QueryTypeEnum type)
        {
            queryType = type;
            return this;
        }

        public IQueryBuilder SelectColumn(Column column)
        {
            selectedColumns.Add(column);
            return this;
        }

        public IQueryBuilder SetColumn(Column column, object value)
        {
            setColumns.Add(column, value);
            return this;
        }

        public IQueryBuilder Table(Type table)
        {
            this.table = GetTableName(table);
            return this;
        }

        private string CreateSelectStatement()
        {
            StringBuilder sql = new StringBuilder();
            CreateSelect(sql);
            CreateFrom(sql);
            CreateJoin(sql);
            CreateWhere(sql);
            CreateGroupBy(sql);
            CreateOrderBy(sql);
            CreateLimit(sql);
            sql.Append(";");

            return sql.ToString();
        }

        private string CreateDeleteFromStatement()
        {
            StringBuilder sql = new StringBuilder();
            CreateDelete(sql);
            CreateFrom(sql);
            CreateWhere(sql);
            CreateLimit(sql);
            sql.Append(";");
            return sql.ToString();
        }

        private string CreateInsertStatement()
        {
            StringBuilder sql = new StringBuilder();
            CreateInsert(sql);
            sql.Append(";");
            return sql.ToString();
        }

        private string CreateUpdateStatement()
        {
            StringBuilder sql = new StringBuilder();
            CreateUpdate(sql);
            CreateSetColumn(sql);
            CreateWhere(sql);
            sql.Append(";");

            return sql.ToString();
        }

        private StringBuilder CreateSelect(StringBuilder sql)
        {
            sql.Append("SELECT ");

            if (selectedColumns.Count == 0)
            {
                sql.Append("*");
                return sql;
            }

            sql.Append(string.Join(", ", selectedColumns.Select(column =>
            {
                if (column.ColumnName == "*")
                {
                    return $"`{GetTableName(column.DomainObject)}`.*";
                }
                else if (column.Alias != null && column.Alias.Length > 0)
                {
                    return $"`{GetTableName(column.DomainObject)}`.`{column.ColumnName}` AS `{column.Alias}`";
                }
                else
                {
                    return $"`{GetTableName(column.DomainObject)}`.`{column.ColumnName}`";
                }
            })));

            return sql;
        }

        private StringBuilder CreateUpdate(StringBuilder sql)
        {
            sql.Append("UPDATE ");
            sql.Append(table);
            return sql;
        }

        private StringBuilder CreateDelete(StringBuilder sql)
        {
            return sql.Append("DELETE");
        }

        private StringBuilder CreateInsert(StringBuilder sql)
        {
            StringBuilder parameter = new StringBuilder("(");
            StringBuilder values = new StringBuilder("(");

            int i = 0;
            foreach (var column in setColumns)
            {
                if (i > 0)
                {
                    parameter.Append(", ");
                    values.Append(", ");
                }

                parameter.Append("`");
                parameter.Append(column.Key.ColumnName);
                parameter.Append("`");

                values.Append($"@{column.Key.ColumnName}");

                ++i;
            }

            parameter.Append(")");
            values.Append(")");

            sql.Append($"INSERT INTO `{table}` ");
            sql.Append(parameter);
            sql.Append(" VALUES ");
            sql.Append(values);

            return sql;
        }

        private StringBuilder CreateFrom(StringBuilder sql)
        {
            return sql.Append($" FROM `{table}`");
        }

        private StringBuilder CreateJoin(StringBuilder sql)
        {
            string.Join(
                ", ",
                joinedTables.Select(
                    join => sql.Append(
                            $" JOIN `{GetTableName(join.Item2.DomainObject)}` ON (`{GetTableName(join.Item1.DomainObject)}`.`{join.Item1.ColumnName}` = `{GetTableName(join.Item2.DomainObject)}`.`{join.Item2.ColumnName}`)"
                    )
                )
            );

            return sql;
        }

        private StringBuilder CreateWhere(StringBuilder sql)
        {
            string whereClause = conditionBuilder.getString();

            if (whereClause.Length == 0)
            {
                return sql;
            }

            return sql.Append($" WHERE {whereClause}");
        }

        private StringBuilder CreateOrderBy(StringBuilder sql)
        {
            if (orderBy.Count() == 0) { return sql; }

            sql.Append(" ORDER BY ");
            string.Join(", ", orderBy.Select(column => sql.Append($"`{GetTableName(column.DomainObject)}`.`{column.ColumnName}`")));

            return sql;
        }

        private StringBuilder CreateGroupBy(StringBuilder sql)
        {
            if (groupBy == null) { return sql; }

            return sql.Append($" GROUP BY `{GetTableName(groupBy.DomainObject)}`.`{groupBy.ColumnName}`");
        }

        private StringBuilder CreateLimit(StringBuilder sql)
        {
            if (limit == 0) { return sql; }

            return sql.Append($" LIMIT {limit}");
        }

        private StringBuilder CreateSetColumn(StringBuilder sql)
        {
            sql.Append(" SET ");

            int i = 0;
            foreach (var col in setColumns)
            {
                if (i > 0) { sql.Append(", "); }

                sql.Append($"`{GetTableName(col.Key.DomainObject)}`.`{col.Key.ColumnName}`= @{col.Key.ColumnName}");

                ++i;
            }
            return sql;
        }

        private string GetTableName(Type type)
        {
            TableNameAttribute TableName = (TableNameAttribute)Attribute.GetCustomAttributes(type, typeof(TableNameAttribute)).FirstOrDefault();

            if (TableName == null)
            {
                throw new Exception($"Tablename not found for {type.Name}!");
            }

            return TableName.TableName;
        }
    }
}
