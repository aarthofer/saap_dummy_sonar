using Apollo.Core.Dal.Common;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Dao
{
    public class ApolloDao<T> : IApolloDao<T>
    {
        protected readonly IConnectionFactory connectionFactory;
        protected readonly IQueryBuilderFactory qbFactory;
        protected readonly DaoTemplate template;

        public ApolloDao(IConnectionFactory connectionFactory, IQueryBuilderFactory qbFactory)
        {
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            this.qbFactory = qbFactory ?? throw new ArgumentNullException(nameof(qbFactory));
            this.template = new DaoTemplate(this.connectionFactory);
        }

        /**************     INTERFACE METHODS       ***************/
        public virtual async Task<IEnumerable<T>> FindAllAsync()
        {
            return await FindAllAsync<T>();
        }

        public virtual async Task<T> FindByIdAsync(int id)
        {
            return await FindByIdAsync<T>(id);
        }

        public virtual async Task<T> CreateAsync(T dao)
        {
            return await CreateAsync<T>(dao);
        }

        public virtual async Task<T> UpdateAsync(T dao)
        {
            return await UpdateAsync<T>(dao);
        }

        public virtual async Task<int> DeleteByIdAsync(int id)
        {
            return await DeleteByIdAsync<T>(id);
        }

        public virtual async Task<IEnumerable<T>> FindByColumn(string column, object value)
        {
            return await FindByColumn<T>(column, value);
        }

        public IQueryBuilder GetQueryBuilder()
        {
            return qbFactory.GetQueryBuilder();
        }

        /************* GENERIC VERSION OF INTERFACE METHODS ********************/
        public virtual async Task<DAO> FindByIdAsync<DAO>(int id)
        {
            var queryBuilder = GetQueryBuilder()
                .Table(typeof(DAO))
                .SetCondition(new Column(typeof(DAO), "id"), OperationType.Equals, id)
                .Limit(1);


            return await template.QuerySingleAsync<DAO>(queryBuilder, template.GenericRowMapper<DAO>);
        }

        public virtual async Task<DAO> FindByPrimaryKey<DAO>(DAO dao)
        {
            var primaryKeys = typeof(DAO).GetProperties()
                    .Where(prop => Attribute.IsDefined(prop, typeof(PKAttribute)));

            var queryBuilder = GetQueryBuilder()
               .Table(typeof(DAO));

            foreach (var primaryKey in primaryKeys)
            {
                var primaryKeyColumn = (ColumnAttribute)Attribute.GetCustomAttribute(primaryKey, typeof(ColumnAttribute));

                queryBuilder.AddAnd(
                    Column.Create<DAO>(primaryKeyColumn.ColumnName),
                    OperationType.Equals,
                    primaryKey.GetValue(dao))
                .Limit(1);

            }
            return await template.QuerySingleAsync<DAO>(queryBuilder, template.GenericRowMapper<DAO>);
        }

        public virtual async Task<int> DeleteByPKAsync(T dao)
        {
            var primaryKey = typeof(T).GetProperties()
                    .Where(prop => Attribute.IsDefined(prop, typeof(PKAttribute)))
                    .SingleOrDefault();

            var primaryKeyColumn = (ColumnAttribute)Attribute.GetCustomAttribute(primaryKey, typeof(ColumnAttribute));

            var queryBuilder = GetQueryBuilder()
                .Table(typeof(T))
                .QueryType(QueryTypeEnum.DELETE)
                .SetCondition(
                    Column.Create<T>(primaryKeyColumn.ColumnName),
                    OperationType.Equals,
                    primaryKey.GetValue(dao))
                .Limit(1);

            return await template.ExecuteAsync(queryBuilder);
        }

        public virtual async Task<IEnumerable<DAO>> FindAllAsync<DAO>()
        {
            var queryBuilder = GetQueryBuilder()
                .Table(typeof(DAO));
            return await template.QueryAsync<DAO>(queryBuilder, template.GenericRowMapper<DAO>);
        }

        public virtual async Task<IEnumerable<DAO>> FindByColumn<DAO>(string column, object value)
        {
            var queryBuilder = GetQueryBuilder()
                .Table(typeof(DAO))
                .SetCondition(Column.Create<DAO>(column), OperationType.Equals, value);

            return await template.QueryAsync<DAO>(queryBuilder, template.GenericRowMapper<DAO>);
        }

        public virtual async Task<int> DeleteByIdAsync<DAO>(int id)
        {
            var queryBuilder = GetQueryBuilder()
                .QueryType(QueryTypeEnum.DELETE)
                .Table(typeof(DAO))
                .SetCondition(new Column(typeof(DAO), "id"), OperationType.Equals, id)
                .Limit(1);

            try
            {
                return await template.ExecuteAsync(queryBuilder);
            }
            catch (DbException dbEx)
            {
                if (dbEx.ErrorCode == -2147467259)
                {
                    throw new AdoException("a foreign key constraint fails", dbEx);
                }
                else
                {
                    throw new AdoException("undefined sql error", dbEx);
                }
            }
        }

        public virtual async Task<DAO> CreateAsync<DAO>(DAO dao)
        {
            await ExecuteAsync(dao, QueryTypeEnum.INSERT);
            if (ContainsAutoIncrementColumn(typeof(DAO)))
            {
                return await FindByIdAsync<DAO>(template.LastInsertId);
            }
            else
            {
                return await FindByPrimaryKey<DAO>(dao);
            }
        }

        public virtual async Task<DAO> UpdateAsync<DAO>(DAO dao)
        {
            await ExecuteAsync(dao, QueryTypeEnum.UPDATE);

            return await FindByPrimaryKey<DAO>(dao);
        }

        public virtual async Task<int> ExecuteAsync<DAO>(DAO dao, QueryTypeEnum queryType)
        {
            var queryBuilder = GetQueryBuilder()
                .Table(typeof(DAO))
                .QueryType(queryType);

            if (queryType == Interface.QueryTypeEnum.UPDATE)
            {
                var primaryKey = typeof(DAO).GetProperties()
                    .Where(prop => Attribute.IsDefined(prop, typeof(PKAttribute)))
                    .SingleOrDefault();

                var primaryKeyColumn = (ColumnAttribute)Attribute.GetCustomAttribute(primaryKey, typeof(ColumnAttribute));

                queryBuilder.SetCondition(
                    Column.Create<DAO>(primaryKeyColumn.ColumnName),
                    OperationType.Equals,
                    primaryKey.GetValue(dao));
            }

            foreach (var propertyInfo in typeof(DAO).GetProperties())
            {
                //current property is not part of DB schema
                if (!Attribute.IsDefined(propertyInfo, typeof(ColumnAttribute))) { continue; }

                if (queryType == Interface.QueryTypeEnum.UPDATE && Attribute.IsDefined(propertyInfo, typeof(PKAttribute))) { continue; }

                ColumnAttribute attribute = (ColumnAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(ColumnAttribute));

                if (attribute != null && !attribute.AutoIncrement)
                {
                    queryBuilder.SetColumn(Column.Create<DAO>(attribute.ColumnName),
                        propertyInfo.PropertyType.IsEnum ? propertyInfo.GetValue(dao).ToString() : propertyInfo.GetValue(dao));
                }

            }

            return await template.ExecuteAsync(queryBuilder);
        }

        private bool ContainsAutoIncrementColumn(Type type)
        {
            foreach (var propertyInfo in type.GetProperties())
            {
                //current property is not part of DB schema
                if (!Attribute.IsDefined(propertyInfo, typeof(ColumnAttribute))) { continue; }

                ColumnAttribute attribute = (ColumnAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(ColumnAttribute));

                if (attribute != null && attribute.AutoIncrement)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
