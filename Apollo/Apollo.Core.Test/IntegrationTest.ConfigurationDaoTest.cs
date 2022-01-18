using Apollo.Core.Dal.Common;
using Apollo.Core.Dal.Dao;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.Transactions;
using Xunit;

namespace Apollo.Core.Test
{
    public class ConfigurationDaoTest
    {
        private IConnectionFactory connectionFactory;
        private IQueryBuilderFactory qbFactory;
        private IConfigurationDao configurationDao;

        public ConfigurationDaoTest()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false).Build();

            var dbConfig = config.GetSection("ConnectionStrings")
                .GetSection("ApolloDbConnection");

            var connectionString = dbConfig.GetValue<string>("ConnectionString");
            var providerName = dbConfig.GetValue<string>("ProviderName");

            connectionFactory = new DefaultConnectionFactory(connectionString, providerName);
            qbFactory = new DefaultQueryBuilderFactory(dbConfig.GetValue<string>("QueryBuilder"));

            configurationDao = new ConfigurationDao(connectionFactory, qbFactory);
        }

        [Fact]
        public async Task InsertSimpleValueTest()
        {
            using (TransactionScope transactionScope = TestUtils.CreateTransaction())
            {
                var c1 = await configurationDao.CreateAsync(new Configuration()
                {
                    Key = "KeyInt",
                    Value = 25
                });
                Assert.NotNull(c1);

                var c2 = await configurationDao.CreateAsync(new Configuration()
                {
                    Key = "KeyString",
                    Value = "no Test!"
                });
                Assert.NotNull(c2);

                var c3 = await configurationDao.CreateAsync(new Configuration()
                {
                    Key = "KeyDate",
                    Value = new DateTime(2020, 11, 11)
                });

                Assert.NotNull(c3);

                var cc1 = await configurationDao.FindByKeyAsync("KeyInt");
                var cc2 = await configurationDao.FindByKeyAsync("KeyString");
                var cc3 = await configurationDao.FindByKeyAsync("KeyDate");

                Assert.Equal("25", cc1.JsonValue);
                Assert.Equal("no Test!", cc2.Value);
                Assert.Equal(new DateTime(2020, 11, 11), cc3.Value);

            } // do not commit transacionScope
        }

        [Fact]
        public async Task InsertComplexValueTest()
        {
            using (TransactionScope transactionScope = TestUtils.CreateTransaction())
            {
                Person person = new Person()
                {
                    Id = 1,
                    Name = "Person1"
                };

                Person person2 = new Person()
                {
                    Id = 2,
                    Name = "Person2"
                };

                var pArr = new[] { person, person2 };
                var c1 = await configurationDao.CreateAsync(new Configuration()
                {
                    Key = "PersonListKey",
                    Value = pArr
                });

                Assert.NotNull(c1);
                Assert.NotNull(c1.Value);

                var c2 = await configurationDao.FindByKeyAsync("PersonListKey");
                Assert.NotNull(c2);
                Assert.Equal(pArr, ((JArray)c2.Value).ToObject<Person[]>());

            }
        }

        [Fact]
        public async Task DeleteTest()
        {
            using (TransactionScope transactionScope = TestUtils.CreateTransaction())
            {
                var c1 = await configurationDao.CreateAsync(new Configuration()
                {
                    Key = "KeyString",
                    Value = "no Test!"
                });
                Assert.NotNull(c1);

                int count = await configurationDao.DeleteByPKAsync(c1);
                Assert.Equal(1, count);

                var notFound = await configurationDao.FindByKeyAsync("KeyString");
                Assert.Null(notFound);

            } // do not commit transacionScope
        }

        [Fact]
        public async Task UpdateTest()
        {
            using (TransactionScope transactionScope = TestUtils.CreateTransaction())
            {
                var c1 = await configurationDao.CreateAsync(new Configuration()
                {
                    Key = "KeyString",
                    Value = "no Test!"
                });
                Assert.NotNull(c1);

                c1.Value = "updated";
                var c2 = await configurationDao.UpdateAsync(c1);
                Assert.NotNull(c2);
                Assert.Equal("updated", c2.Value);

                var c3 = await configurationDao.FindByKeyAsync("KeyString");
                Assert.NotNull(c3);
                Assert.Equal("updated", c3.Value);

            } // do not commit transacionScope
        }
    }
}
