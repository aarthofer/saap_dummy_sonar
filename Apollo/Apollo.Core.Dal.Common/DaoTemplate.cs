using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Common
{
    public struct QueryResolver
    {
        public Type Type { get; set; }
        public string QueryName { get; set; }
    }

    public delegate T RowMapper<T>(IDataRecord recod);
    public class DaoTemplate
    {
        private readonly IConnectionFactory connectionFactory;
        public int LastInsertId { get; private set; } = 0;

        public DaoTemplate(IConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<T> QuerySingleAsync<T>(IQueryBuilder queryBuilder, RowMapper<T> rowMapper = null)
        {
            return (await QueryAsync<T>(queryBuilder, rowMapper)).SingleOrDefault();
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(IQueryBuilder queryBuilder, RowMapper<T> rowMapper = null)
        {
            rowMapper = rowMapper ?? GenericRowMapper<T>;

            using (DbConnection connection = await connectionFactory.CreateConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = queryBuilder.GetQuery();
                    setParameters(queryBuilder.GetParameter(), command);

                    var items = new List<T>();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(rowMapper(reader));
                        }
                        return items;
                    }
                }
            }
        }

        public async Task<int> ExecuteAsync(IQueryBuilder queryBuilder)
        {
            using (DbConnection connection = await connectionFactory.CreateConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = queryBuilder.GetQuery();
                    setParameters(queryBuilder.GetParameter(), command);

                    var result = command.ExecuteNonQuery();
                    LastInsertId = GetLastInsertId(connection, queryBuilder);
                    return result;
                }
            }
        }

        public int GetLastInsertId(DbConnection connection, IQueryBuilder qb)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = qb.GetLastInsertIDQuery();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Convert.ToInt32(reader["id"]);
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }

        private static void setParameters(QueryParameter[] parameters, IDbCommand command)
        {
            foreach (var param in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = param.ParamName;
                parameter.Value = param.Value;
                command.Parameters.Add(parameter);
            }
        }

        public T GenericRowMapper<T>(IDataRecord row)
        {
            var item = (T)Activator.CreateInstance(typeof(T));

            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                ColumnAttribute column =
               (ColumnAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(ColumnAttribute));

                if (column != null && row[column.ColumnName] != DBNull.Value)
                {
                    if (propertyInfo.PropertyType.IsEnum)
                    {
                        propertyInfo.SetValue(item, Enum.Parse(propertyInfo.PropertyType, (string)row[column.ColumnName]));
                    }
                    else
                    {
                        propertyInfo.SetValue(item, row[column.ColumnName]);
                    }
                }
            }
            return item;
        }
    }
}
