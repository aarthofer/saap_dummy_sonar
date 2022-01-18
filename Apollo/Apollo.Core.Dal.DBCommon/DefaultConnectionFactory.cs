using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Factory
{
    public class DefaultConnectionFactory : IConnectionFactory
    {
        private DbProviderFactory dbProviderFactory;

        public string ConnectionString { get; }

        public string ProviderName { get; }

        public DefaultConnectionFactory(string connectionString, string providerName)
        {
            this.ConnectionString = connectionString;
            this.ProviderName = providerName;

            switch (providerName)
            {
                case "Microsoft.Data.SqlClient":
                    this.dbProviderFactory = Microsoft.Data.SqlClient.SqlClientFactory.Instance;
                    break;
                case "MySql.Data.MySqlClient":
                    this.dbProviderFactory = MySql.Data.MySqlClient.MySqlClientFactory.Instance;
                    break;
                default:
                    throw new ArgumentException($"No provider found for {providerName}.");
            }
        }

        public async Task<DbConnection> CreateConnection()
        {
            var connection = dbProviderFactory.CreateConnection();
            connection.ConnectionString = this.ConnectionString;
            await connection.OpenAsync();

            return connection;
        }
    }
}
