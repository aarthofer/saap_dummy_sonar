using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Apollo.Core.Dal.Common
{
    public class MysqlConditionBuilder : IConditionBuilder
    {
        private string ConditionString { get; set; }
        private IConditionBuilder RightSide { get; set; }
        public List<QueryParameter> Params { get; private set; }
        private OperationType Operation { get; set; }

        public MysqlConditionBuilder()
        {
            ConditionString = "";
            Params = new List<QueryParameter>();
        }

        public IConditionBuilder NewCondition(Column column, OperationType operation, object value, string paramName = null)
        {
            ConditionString = "";
            Params = new List<QueryParameter>();
            AddCondition(column, operation, value, "AND", paramName);
            return this;
        }

        public IConditionBuilder AddAnd(Column column, OperationType operation, object value, string paramName = null)
        {
            AddCondition(column, operation, value, "AND", paramName);
            return this;
        }

        public IConditionBuilder AddAnd(IConditionBuilder condition)
        {
            if (ConditionString.Length == 0)
            {
                ConditionString = condition.getString();
            }
            else
            {
                ConditionString = $"({ConditionString}) AND {condition.getString()}";
            }
            Params.AddRange(condition.Params);
            return this;
        }

        public IConditionBuilder AddOr(Column column, OperationType operation, object value, string paramName = null)
        {
            AddCondition(column, operation, value, "OR", paramName);
            return this;
        }

        public IConditionBuilder AddOr(IConditionBuilder condition)
        {
            if (ConditionString.Length == 0)
            {
                ConditionString = condition.getString();
            }
            else
            {
                ConditionString = $"({ConditionString}) OR {condition.getString()}";
            }
            Params.AddRange(condition.Params);
            return this;
        }

        public string getString()
        {
            if (ConditionString.Length == 0) { return ""; }

            return $"({ConditionString})";
        }

        private void AddCondition(Column column, OperationType operation, object value, string Conjunctor = "AND", string paramName = null)
        {
            string operationToken = GetOperationToken(operation);

            string parameter = string.IsNullOrEmpty(paramName) ? column.ColumnName : paramName;

            Params.Add(new QueryParameter($"@{parameter}", value));

            string newCondition;
            if (operation == OperationType.Like || operation == OperationType.NotLike)
            {
                newCondition = $"LOWER(`{GetTableName(column.DomainObject)}`.`{column.ColumnName}`) {operationToken} LOWER(CONCAT('%', @{parameter}, '%'))";
            }
            else
            {
                newCondition = $"`{GetTableName(column.DomainObject)}`.`{column.ColumnName}`{operationToken}@{parameter}";
            }

            if (ConditionString.Length != 0)
            {
                ConditionString = $"({ConditionString}) {Conjunctor} ({newCondition})";
            }
            else
            {
                ConditionString = $"{newCondition}";
            }
        }

        private string GetOperationToken(OperationType operation)
        {
            switch (operation)
            {
                case OperationType.Equals:
                    return " = ";
                case OperationType.NotEquals:
                    return " <> ";
                case OperationType.Like:
                    return " LIKE ";
                case OperationType.NotLike:
                    return " NOT LIKE ";
                case OperationType.LessThanEqual:
                    return " <= ";
                case OperationType.LessThen:
                    return " < ";
                case OperationType.GreaterEqualThen:
                    return " >= ";
                case OperationType.GreaterThen:
                    return " > ";
            }

            throw new NotImplementedException($"Operation {operation} not implemented");
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